using MKS.Shared.Domain.Common;
using MKS.Shared.Domain.Apps.Entities;
using MKS.Shared.Domain.MultiTenancy.Entities;
using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Shared.Infrastructure.Persistence
{
    /// <summary>
    /// Stores an integration event until it can be published.
    /// </summary>
    public sealed class OutboxMessageEntity : BaseEntity, IMayHaveAppEntity, IMayHaveTenantEntity
    {
        /// <summary>Gets or sets the stable event identifier.</summary>
        public Guid EventId { get; set; }
        /// <summary>Gets or sets whether this message is global, app-scoped, or tenant-scoped.</summary>
        public CEntityScope Scope { get; set; }
        /// <summary>Gets or sets the application identifier, when app-scoped.</summary>
        public Guid? AppId { get; set; }
        /// <summary>Gets or sets the tenant identifier, when tenant-scoped.</summary>
        public Guid? TenantId { get; set; }
        /// <summary>Gets or sets the event type.</summary>
        public string Type { get; set; } = string.Empty;
        /// <summary>Gets or sets the serialized event payload.</summary>
        public string Payload { get; set; } = string.Empty;
        /// <summary>Gets or sets when the event occurred.</summary>
        public DateTimeOffset OccurredAt { get; set; }
        /// <summary>Gets or sets the delivery state.</summary>
        public OutboxMessageStatus Status { get; set; }
        /// <summary>Gets or sets the number of delivery attempts.</summary>
        public int AttemptCount { get; set; }
        /// <summary>Gets or sets the earliest time the message can be retried.</summary>
        public DateTimeOffset NextAttemptAt { get; set; }
        /// <summary>Gets or sets the current lease expiration time.</summary>
        public DateTimeOffset? LockedUntil { get; set; }
        /// <summary>Gets or sets the token identifying the active lease.</summary>
        public Guid? LockId { get; set; }
        /// <summary>Gets or sets when the event was published.</summary>
        public DateTimeOffset? ProcessedAt { get; set; }
        /// <summary>Gets or sets the most recent delivery error.</summary>
        public string? LastError { get; set; }
        /// <summary>Gets or sets when the message was moved to the dead-letter queue.</summary>
        public DateTimeOffset? DeadLetteredAt { get; set; }
    }
}
