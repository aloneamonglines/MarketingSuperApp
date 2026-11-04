using MKS.Identity.Application.Contracts.Common.DTOs;
using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Identity.Application.Contracts.Common.Repositories
{
    /// <summary>Persists and leases durable integration-event deliveries.</summary>
    public interface IOutboxRepository
    {
        Task AddAsync(
            Guid eventId,
            CEntityScope scope,
            Guid? appId,
            Guid? tenantId,
            string type,
            string payload,
            DateTimeOffset occurredAt,
            DateTimeOffset nextAttemptAt,
            CancellationToken cancellationToken = default);

        Task<OutboxMessageLease?> ClaimNextAsync(
            DateTimeOffset now,
            TimeSpan leaseDuration,
            OutboxMessageFilter? filter = null,
            CancellationToken cancellationToken = default);

        Task<bool> RenewLeaseAsync(
            Guid messageId,
            Guid lockId,
            DateTimeOffset lockedUntil,
            CancellationToken cancellationToken = default);

        Task MarkProcessedAsync(
            OutboxMessageLease lease,
            DateTimeOffset processedAt,
            CancellationToken cancellationToken = default);

        Task<bool> RecordFailureAsync(
            OutboxMessageLease lease,
            string error,
            DateTimeOffset failedAt,
            int maxAttempts,
            TimeSpan retryDelay,
            CancellationToken cancellationToken = default);

        Task<bool> ReplayDeadLetterAsync(
            Guid eventId,
            DateTimeOffset nextAttemptAt,
            CancellationToken cancellationToken = default);
    }
}
