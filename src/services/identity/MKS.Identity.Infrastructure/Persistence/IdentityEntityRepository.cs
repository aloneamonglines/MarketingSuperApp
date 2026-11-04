using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;
using MKS.Identity.Application.Contracts.Common.Repositories;
using MKS.Shared.Application.Contracts.Common.DTOs;
using MKS.Shared.Application.Contracts.Common.Interfaces;
using MKS.Shared.Application.Contracts.Common.Constants;
using MKS.Shared.Domain.Common;
using MKS.Shared.Application.Contracts.Common.Events;
using MKS.Shared.Domain.Shared.Common.Events;

namespace MKS.Identity.Infrastructure.Persistence
{
    /// <summary>
    /// Implements shared persistence primitives for audited Identity entities.
    /// </summary>
    public sealed class IdentityEntityRepository<TEntity>(
        IdentityDbContext dbContext,
        ILocalEventDispatcher localEventDispatcher,
        TimeProvider timeProvider)
        : IIdentityEntityRepository<TEntity>
        where TEntity : FullAuditEntity
    {
        public Task<TEntity?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
            dbContext.Set<TEntity>().SingleOrDefaultAsync(entity => entity.Id == id, cancellationToken);

        public async Task<IReadOnlyList<TEntity>> GetManyAsync(
            IReadOnlyCollection<Guid> ids,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(ids);
            if (ids.Count == 0)
            {
                return [];
            }

            var distinctIds = ids.Distinct().ToArray();
            return await dbContext.Set<TEntity>()
                .Where(entity => distinctIds.Contains(entity.Id))
                .ToListAsync(cancellationToken);
        }

        public async Task<PagedResultDto<TEntity>> GetPagedAsync(
            IPagedAndSortedResultRequest request,
            System.Linq.Expressions.Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            var query = dbContext.Set<TEntity>().AsNoTracking();
            if (filter is not null)
            {
                query = query.Where(filter);
            }

            var totalCount = await query.LongCountAsync(cancellationToken);
            var ordered = orderBy is null ? query.OrderBy(entity => entity.Id) : orderBy(query);
            var skipCount = Math.Max(0, request.SkipCount);
            var maxResultCount = Math.Clamp(
                request.MaxResultCount,
                1,
                PagingConstants.MaxMaxResultCount);
            var entities = await ordered
                .Skip(skipCount)
                .Take(maxResultCount)
                .ToListAsync(cancellationToken);
            return new PagedResultDto<TEntity>(totalCount, entities);
        }

        public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entity);
            await dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
        }

        public Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entity);
            dbContext.Set<TEntity>().Update(entity);
            return Task.CompletedTask;
        }

        public Task UpdatePropertiesAsync(
            TEntity entity,
            IReadOnlyCollection<string> propertyNames,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entity);
            ValidateMappedPropertyNames(propertyNames);
            var entry = dbContext.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                dbContext.Attach(entity);
                entry = dbContext.Entry(entity);
            }

            if (entry.State != EntityState.Added)
            {
                foreach (var propertyName in propertyNames)
                {
                    entry.Property(propertyName).IsModified = true;
                }
            }

            return Task.CompletedTask;
        }

        public async Task SoftDeleteAsync(
            Guid id,
            Guid? deleterId,
            DateTimeOffset deletedAt,
            CancellationToken cancellationToken = default)
        {
            await InTransactionAsync(async token =>
            {
                var entity = await GetAsync(id, token)
                    ?? throw new KeyNotFoundException($"Identity entity '{id}' was not found.");
                entity.IsDeleted = true;
                entity.DeleterId = deleterId;
                entity.DeletedAt = deletedAt;
                await IdentityDbContextSaveChanges.SaveChangesWithLocalEventsAsync(dbContext, token);
            }, cancellationToken);
        }

        public async Task BulkInsertAsync(
            IReadOnlyCollection<TEntity> entities,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entities);
            if (entities.Count == 0)
            {
                return;
            }

            await InTransactionAsync(
                async token =>
                {
                    await dbContext.BulkInsertAsync(entities.ToList(), cancellationToken: token);
                    MarkBulkPersistedEntitiesUnchanged(entities);
                    await DispatchLifecycleEventsAsync(
                        entities,
                        (entity, occurredAt) => new EntityCreatedEvent(entity, entity.Id, occurredAt),
                        token);
                },
                cancellationToken);
        }

        public async Task BulkUpdateAsync(
            IReadOnlyCollection<TEntity> entities,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entities);
            if (entities.Count == 0)
            {
                return;
            }

            await InTransactionAsync(
                async token =>
                {
                    await dbContext.BulkUpdateAsync(entities.ToList(), cancellationToken: token);
                    MarkBulkPersistedEntitiesUnchanged(entities);
                    await DispatchLifecycleEventsAsync(
                        entities,
                        (entity, occurredAt) => entity.IsDeleted
                            ? new EntityDeletedEvent(entity, entity.Id, occurredAt)
                            : new EntityUpdatedEvent(entity, entity.Id, occurredAt),
                        token);
                },
                cancellationToken);
        }

        public async Task BulkUpdatePropertiesAsync(
            IReadOnlyCollection<TEntity> entities,
            IReadOnlyCollection<string> propertyNames,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(entities);
            ValidateMappedPropertyNames(propertyNames);
            if (entities.Count == 0)
            {
                return;
            }

            await InTransactionAsync(
                async token =>
                {
                    await dbContext.BulkUpdateAsync(
                        entities.ToList(),
                        new BulkConfig { PropertiesToInclude = propertyNames.ToList() },
                        cancellationToken: token);
                    MarkBulkPersistedEntitiesUnchanged(entities);
                    await DispatchLifecycleEventsAsync(
                        entities,
                        (entity, occurredAt) => entity.IsDeleted
                            ? new EntityDeletedEvent(entity, entity.Id, occurredAt)
                            : new EntityUpdatedEvent(entity, entity.Id, occurredAt),
                        token);
                },
                cancellationToken);
        }

        public async Task BulkSoftDeleteAsync(
            IReadOnlyCollection<Guid> ids,
            Guid? deleterId,
            DateTimeOffset deletedAt,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(ids);
            if (ids.Count == 0)
            {
                return;
            }

            var entities = await GetManyAsync(ids, cancellationToken);
            var requestedIds = ids.Distinct().ToHashSet();
            if (entities.Count != requestedIds.Count)
            {
                throw new KeyNotFoundException("One or more Identity entities were not found.");
            }

            foreach (var entity in entities)
            {
                entity.IsDeleted = true;
                entity.DeleterId = deleterId;
                entity.DeletedAt = deletedAt;
            }

            await InTransactionAsync(
                async token =>
                {
                    await dbContext.BulkUpdateAsync(
                        entities.ToList(),
                        new BulkConfig
                        {
                            PropertiesToInclude = [
                                nameof(FullAuditEntity.IsDeleted),
                                nameof(FullAuditEntity.DeleterId),
                                nameof(FullAuditEntity.DeletedAt)
                            ]
                        },
                        cancellationToken: token);
                    MarkBulkPersistedEntitiesUnchanged(entities);
                    await DispatchLifecycleEventsAsync(
                        entities,
                        (entity, occurredAt) => new EntityDeletedEvent(entity, entity.Id, occurredAt),
                        token);
                },
                cancellationToken);
        }

        private async Task DispatchLifecycleEventsAsync(
            IReadOnlyCollection<TEntity> entities,
            Func<TEntity, DateTimeOffset, EntityLifecycleEvent> eventFactory,
            CancellationToken cancellationToken)
        {
            var occurredAt = timeProvider.GetUtcNow();
            foreach (var entity in entities)
            {
                if (entity is IHasLocalEvent aggregate)
                {
                    foreach (var localEvent in aggregate.LocalEvents)
                    {
                        await localEventDispatcher.DispatchAsync(localEvent, cancellationToken);
                    }

                    aggregate.ClearLocalEvents();
                }

                await localEventDispatcher.DispatchAsync(
                    eventFactory(entity, occurredAt),
                    cancellationToken);
            }
        }

        private void MarkBulkPersistedEntitiesUnchanged(IReadOnlyCollection<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                var entry = dbContext.Entry(entity);
                if (entry.State != EntityState.Detached)
                {
                    entry.State = EntityState.Unchanged;
                }
            }
        }

        private async Task InTransactionAsync(
            Func<CancellationToken, Task> operation,
            CancellationToken cancellationToken)
        {
            if (dbContext.Database.CurrentTransaction is not null)
            {
                await operation(cancellationToken);
                return;
            }

            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            await operation(cancellationToken);
            await IdentityDbContextSaveChanges.SaveChangesWithLocalEventsAsync(dbContext, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }

        private void ValidateMappedPropertyNames(IReadOnlyCollection<string> propertyNames)
        {
            ArgumentNullException.ThrowIfNull(propertyNames);
            if (propertyNames.Count == 0 || propertyNames.Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException("At least one mapped property name is required.", nameof(propertyNames));
            }

            var entityType = dbContext.Model.FindEntityType(typeof(TEntity));
            if (entityType is null || propertyNames.Any(name => entityType.FindProperty(name) is null))
            {
                throw new ArgumentException(
                    $"One or more property names are not mapped on '{typeof(TEntity).Name}'.",
                    nameof(propertyNames));
            }
        }
    }
}
