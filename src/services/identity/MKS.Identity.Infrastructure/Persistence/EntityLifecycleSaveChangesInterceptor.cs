using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MKS.Shared.Application.Contracts.Common.Events;
using MKS.Shared.Domain.Common;
using MKS.Shared.Domain.Shared.Common.Events;

namespace MKS.Identity.Infrastructure.Persistence
{
    /// <summary>Dispatches aggregate-local and standard entity lifecycle events after successful persistence.</summary>
    public sealed class EntityLifecycleSaveChangesInterceptor(
        ILocalEventDispatcher localEventDispatcher,
        TimeProvider timeProvider) : SaveChangesInterceptor
    {
        private readonly ConditionalWeakTable<DbContext, PendingEventCollection> _pendingEvents = new();

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            CaptureEvents(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            CaptureEvents(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override int SavedChanges(
            SaveChangesCompletedEventData eventData,
            int result)
        {
            DispatchPendingEvents(eventData.Context, CancellationToken.None).GetAwaiter().GetResult();
            return base.SavedChanges(eventData, result);
        }

        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData,
            int result,
            CancellationToken cancellationToken = default)
        {
            await DispatchPendingEvents(eventData.Context, cancellationToken).ConfigureAwait(false);
            return await base.SavedChangesAsync(eventData, result, cancellationToken).ConfigureAwait(false);
        }

        private void CaptureEvents(DbContext? context)
        {
            if (context is null)
            {
                return;
            }

            var events = new List<PendingLocalEvent>();
            var occurredAt = timeProvider.GetUtcNow();
            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry.Entity is IHasLocalEvent aggregate)
                {
                    events.AddRange(aggregate.LocalEvents.Select(localEvent =>
                        new PendingLocalEvent(aggregate, localEvent)));
                }

                if (!TryGetEntityId(entry, out var entityId))
                {
                    continue;
                }

                EntityLifecycleEvent? lifecycleEvent = entry.State switch
                {
                    EntityState.Added => new EntityCreatedEvent(entry.Entity, entityId, occurredAt),
                    EntityState.Deleted => new EntityDeletedEvent(entry.Entity, entityId, occurredAt),
                    EntityState.Modified when IsSoftDeleted(entry) =>
                        new EntityDeletedEvent(entry.Entity, entityId, occurredAt),
                    EntityState.Modified => new EntityUpdatedEvent(entry.Entity, entityId, occurredAt),
                    _ => null
                };
                if (lifecycleEvent is not null)
                {
                    events.Add(new PendingLocalEvent(null, lifecycleEvent));
                }
            }

            _pendingEvents.Remove(context);
            _pendingEvents.Add(context, new PendingEventCollection(events));
        }

        private async Task DispatchPendingEvents(DbContext? context, CancellationToken cancellationToken)
        {
            if (context is null || !_pendingEvents.TryGetValue(context, out var pending))
            {
                return;
            }

            foreach (var item in pending.Events)
            {
                await localEventDispatcher.DispatchAsync(item.Event, cancellationToken).ConfigureAwait(false);
            }

            foreach (var aggregate in pending.Events
                         .Select(item => item.Source)
                         .OfType<IHasLocalEvent>())
            {
                aggregate.ClearLocalEvents();
            }

            _pendingEvents.Remove(context);
        }

        private static bool IsSoftDeleted(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry) =>
            entry.Entity is FullAuditEntity { IsDeleted: true } &&
            entry.Property(nameof(FullAuditEntity.IsDeleted)).IsModified;

        private static bool TryGetEntityId(
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry,
            out Guid entityId)
        {
            if (entry.Entity is BaseEntity baseEntity)
            {
                entityId = baseEntity.Id;
                return true;
            }

            var idProperty = entry.Metadata.FindProperty(nameof(BaseEntity.Id));
            if (idProperty?.ClrType == typeof(Guid) &&
                entry.Property(idProperty.Name).CurrentValue is Guid id)
            {
                entityId = id;
                return true;
            }

            entityId = Guid.Empty;
            return false;
        }

        private sealed record PendingLocalEvent(IHasLocalEvent? Source, object Event);

        private sealed class PendingEventCollection(IReadOnlyList<PendingLocalEvent> events)
        {
            public IReadOnlyList<PendingLocalEvent> Events { get; } = events;
        }
    }
}
