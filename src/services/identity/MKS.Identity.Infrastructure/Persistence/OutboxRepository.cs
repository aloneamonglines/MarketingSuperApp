using Microsoft.EntityFrameworkCore;
using MKS.Identity.Application.Contracts.Common.DTOs;
using MKS.Identity.Application.Contracts.Common.Repositories;
using MKS.Identity.Domain.Shared.Constants;
using MKS.Shared.Domain.Shared.Enums;
using MKS.Shared.Application.Contracts.Common.Events;
using MKS.Shared.Domain.Shared.Common.Events;
using MKS.Shared.Infrastructure.Persistence;

namespace MKS.Identity.Infrastructure.Persistence
{
    /// <summary>Implements durable queue leasing and transitions with PostgreSQL transactions.</summary>
    public sealed class OutboxRepository(
        IdentityDbContext dbContext,
        ILocalEventDispatcher localEventDispatcher,
        TimeProvider timeProvider) : IOutboxRepository
    {
        public async Task AddAsync(
            Guid eventId,
            CEntityScope scope,
            Guid? appId,
            Guid? tenantId,
            string type,
            string payload,
            DateTimeOffset occurredAt,
            DateTimeOffset nextAttemptAt,
            CancellationToken cancellationToken = default)
        {
            if (!IsValidScope(scope, appId, tenantId))
            {
                throw new ArgumentException(
                    IdentityConstants.ValidationMessages.IntegrationEventScopeMetadataInvalid,
                    nameof(scope));
            }

            await dbContext.OutboxMessages.AddAsync(new OutboxMessageEntity
            {
                EventId = eventId,
                Scope = scope,
                AppId = appId,
                TenantId = tenantId,
                Type = type,
                Payload = payload,
                OccurredAt = occurredAt,
                Status = OutboxMessageStatus.Pending,
                NextAttemptAt = nextAttemptAt
            }, cancellationToken);
        }

        public async Task<OutboxMessageLease?> ClaimNextAsync(
            DateTimeOffset now,
            TimeSpan leaseDuration,
            OutboxMessageFilter? filter = null,
            CancellationToken cancellationToken = default)
        {
            if (filter is not null)
            {
                ValidateFilter(filter);
            }

            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            var message = await dbContext.OutboxMessages
                .FromSqlInterpolated(BuildClaimQuery(now, filter))
                .AsNoTracking()
                .SingleOrDefaultAsync(cancellationToken);
            if (message is null)
            {
                await transaction.CommitAsync(cancellationToken);
                return null;
            }

            var lockId = Guid.CreateVersion7();
            message.Status = OutboxMessageStatus.Processing;
            message.AttemptCount++;
            message.LockedUntil = now.Add(leaseDuration);
            message.LockId = lockId;
            dbContext.OutboxMessages.Update(message);
            await IdentityDbContextSaveChanges.SaveChangesWithLocalEventsAsync(dbContext, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new OutboxMessageLease(
                message.Id,
                message.EventId,
                message.Scope,
                message.AppId,
                message.TenantId,
                message.Type,
                message.Payload,
                message.OccurredAt,
                message.AttemptCount,
                lockId);
        }

        public async Task<bool> RenewLeaseAsync(
            Guid messageId,
            Guid lockId,
            DateTimeOffset lockedUntil,
            CancellationToken cancellationToken = default)
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            var affected = await dbContext.OutboxMessages
                .Where(message =>
                    message.Id == messageId &&
                    message.Status == OutboxMessageStatus.Processing &&
                    message.LockId == lockId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(
                    message => message.LockedUntil,
                    lockedUntil), cancellationToken);
            if (affected == 1)
            {
                var message = await dbContext.OutboxMessages
                    .AsNoTracking()
                    .SingleAsync(item => item.Id == messageId, cancellationToken);
                await localEventDispatcher.DispatchAsync(
                    new EntityUpdatedEvent(message, message.Id, timeProvider.GetUtcNow()),
                    cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);
            return affected == 1;
        }

        public async Task MarkProcessedAsync(
            OutboxMessageLease lease,
            DateTimeOffset processedAt,
            CancellationToken cancellationToken = default)
        {
            var message = await GetClaimedMessageAsync(lease, cancellationToken);
            if (message is null)
            {
                return;
            }

            message.Status = OutboxMessageStatus.Processed;
            message.ProcessedAt = processedAt;
            message.LockedUntil = null;
            message.LockId = null;
            message.LastError = null;
            await IdentityDbContextSaveChanges.SaveChangesWithLocalEventsAsync(dbContext, cancellationToken);
        }

        public async Task<bool> RecordFailureAsync(
            OutboxMessageLease lease,
            string error,
            DateTimeOffset failedAt,
            int maxAttempts,
            TimeSpan retryDelay,
            CancellationToken cancellationToken = default)
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            var message = await GetClaimedMessageAsync(lease, cancellationToken);
            if (message is null)
            {
                await transaction.CommitAsync(cancellationToken);
                return false;
            }

            message.LastError = error;
            message.LockedUntil = null;
            message.LockId = null;
            var deadLettered = message.AttemptCount >= maxAttempts;
            if (deadLettered)
            {
                message.Status = OutboxMessageStatus.DeadLettered;
                message.DeadLetteredAt = failedAt;
                await dbContext.OutboxDeadLetters.AddAsync(new OutboxDeadLetterEntity
                {
                    EventId = message.EventId,
                    Scope = message.Scope,
                    AppId = message.AppId,
                    TenantId = message.TenantId,
                    Type = message.Type,
                    Payload = message.Payload,
                    OccurredAt = message.OccurredAt,
                    FailedAt = failedAt,
                    AttemptCount = message.AttemptCount,
                    LastError = error
                }, cancellationToken);
            }
            else
            {
                message.Status = OutboxMessageStatus.Pending;
                message.NextAttemptAt = failedAt.Add(retryDelay);
            }

            await IdentityDbContextSaveChanges.SaveChangesWithLocalEventsAsync(dbContext, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return deadLettered;
        }

        public async Task<bool> ReplayDeadLetterAsync(
            Guid eventId,
            DateTimeOffset nextAttemptAt,
            CancellationToken cancellationToken = default)
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            var message = await dbContext.OutboxMessages.SingleOrDefaultAsync(
                item => item.EventId == eventId && item.Status == OutboxMessageStatus.DeadLettered,
                cancellationToken);
            if (message is null)
            {
                await transaction.CommitAsync(cancellationToken);
                return false;
            }

            message.Status = OutboxMessageStatus.Pending;
            message.AttemptCount = 0;
            message.NextAttemptAt = nextAttemptAt;
            message.LockedUntil = null;
            message.LockId = null;
            message.LastError = null;
            message.DeadLetteredAt = null;
            await IdentityDbContextSaveChanges.SaveChangesWithLocalEventsAsync(dbContext, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return true;
        }

        private Task<OutboxMessageEntity?> GetClaimedMessageAsync(
            OutboxMessageLease lease,
            CancellationToken cancellationToken) =>
            dbContext.OutboxMessages.SingleOrDefaultAsync(
                message => message.Id == lease.Id &&
                           message.Status == OutboxMessageStatus.Processing &&
                           message.LockId == lease.LockId,
                cancellationToken);

        private static void ValidateFilter(OutboxMessageFilter filter)
        {
            if (!IsValidScope(filter.Scope, filter.AppId, filter.TenantId))
            {
                throw new ArgumentException(
                    IdentityConstants.ValidationMessages.IntegrationEventScopeFilterInvalid,
                    nameof(filter));
            }
        }

        private static bool IsValidScope(CEntityScope scope, Guid? appId, Guid? tenantId) =>
            scope switch
            {
                CEntityScope.Global => appId is null && tenantId is null,
                CEntityScope.App => appId is { } app && app != Guid.Empty && tenantId is null,
                CEntityScope.Tenant =>
                    appId is { } tenantApp && tenantApp != Guid.Empty &&
                    tenantId is { } tenant && tenant != Guid.Empty,
                _ => false
            };

        private static FormattableString BuildClaimQuery(DateTimeOffset now, OutboxMessageFilter? filter)
        {
            if (filter is null)
            {
                return $"""
                        SELECT *
                        FROM identity."OutboxMessages"
                        WHERE (
                            ("Status" = {(int)OutboxMessageStatus.Pending} AND "NextAttemptAt" <= {now})
                            OR
                            ("Status" = {(int)OutboxMessageStatus.Processing} AND "LockedUntil" <= {now})
                        )
                        ORDER BY "NextAttemptAt", "OccurredAt"
                        LIMIT 1
                        FOR UPDATE SKIP LOCKED
                        """;
            }

            return $"""
                    SELECT *
                    FROM identity."OutboxMessages"
                    WHERE (
                        ("Status" = {(int)OutboxMessageStatus.Pending} AND "NextAttemptAt" <= {now})
                        OR
                        ("Status" = {(int)OutboxMessageStatus.Processing} AND "LockedUntil" <= {now})
                    )
                    AND "Scope" = {(int)filter.Scope}
                    AND "AppId" IS NOT DISTINCT FROM {filter.AppId}
                    AND "TenantId" IS NOT DISTINCT FROM {filter.TenantId}
                    ORDER BY "NextAttemptAt", "OccurredAt"
                    LIMIT 1
                    FOR UPDATE SKIP LOCKED
                    """;
        }
    }
}
