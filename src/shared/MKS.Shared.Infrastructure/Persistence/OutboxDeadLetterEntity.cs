using MKS.Shared.Domain.Common;
using MKS.Shared.Domain.Apps.Entities;
using MKS.Shared.Domain.MultiTenancy.Entities;
using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Shared.Infrastructure.Persistence
{
    /// <summary>Retains integration events that exhausted their automatic delivery attempts.</summary>
    public sealed class OutboxDeadLetterEntity : BaseEntity, IMayHaveAppEntity, IMayHaveTenantEntity
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
        /// <summary>Gets or sets when the event entered the dead-letter queue.</summary>
        public DateTimeOffset FailedAt { get; set; }
        /// <summary>Gets or sets the number of delivery attempts.</summary>
        public int AttemptCount { get; set; }
        /// <summary>Gets or sets the last delivery error.</summary>
        public string LastError { get; set; } = string.Empty;
    }
}
