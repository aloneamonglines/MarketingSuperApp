using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Identity.Application.Contracts.Common.DTOs
{
    /// <summary>Contains the immutable payload and fencing token for a claimed outbox message.</summary>
    public sealed record OutboxMessageLease(
        Guid Id,
        Guid EventId,
        CEntityScope Scope,
        Guid? AppId,
        Guid? TenantId,
        string Type,
        string Payload,
        DateTimeOffset OccurredAt,
        int AttemptCount,
        Guid LockId);
}
