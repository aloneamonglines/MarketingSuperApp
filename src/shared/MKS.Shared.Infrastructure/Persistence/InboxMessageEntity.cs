using MKS.Shared.Domain.Common;
using MKS.Shared.Domain.Apps.Entities;
using MKS.Shared.Domain.MultiTenancy.Entities;
using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Shared.Infrastructure.Persistence
{
    /// <summary>
    /// Records integration events already consumed for idempotent processing.
    /// </summary>
    public sealed class InboxMessageEntity : BaseEntity, IMayHaveAppEntity, IMayHaveTenantEntity
    {
        /// <summary>Gets or sets the external event identifier.</summary>
        public Guid EventId { get; set; }
        /// <summary>Gets or sets whether this message is global, app-scoped, or tenant-scoped.</summary>
        public CEntityScope Scope { get; set; }
        /// <summary>Gets or sets the application identifier, when app-scoped.</summary>
        public Guid? AppId { get; set; }
        /// <summary>Gets or sets the tenant identifier, when tenant-scoped.</summary>
        public Guid? TenantId { get; set; }
        /// <summary>Gets or sets the event handler that consumed the event.</summary>
        public string Consumer { get; set; } = string.Empty;
        /// <summary>Gets or sets the event type.</summary>
        public string Type { get; set; } = string.Empty;
        /// <summary>Gets or sets the total number of consumer attempts.</summary>
        public int AttemptCount { get; set; }
        /// <summary>Gets or sets the most recent consumer error.</summary>
        public string? LastError { get; set; }
        /// <summary>Gets or sets when the event was processed.</summary>
        public DateTimeOffset? ProcessedAt { get; set; }
    }
}
