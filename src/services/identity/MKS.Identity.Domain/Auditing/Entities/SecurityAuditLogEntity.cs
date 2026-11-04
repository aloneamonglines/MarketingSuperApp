using MKS.Shared.Domain.Common;
using MKS.Shared.Domain.Shared.Attributes;
using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Identity.Domain.Auditing.Entities
{
    /// <summary>
    /// Append-only record of a security-sensitive operation.
    /// </summary>
    [EntityScope(CEntityScope.Global)]
    public sealed class SecurityAuditLogEntity : BaseEntity
    {
        /// <summary>Gets or sets the acting user identifier.</summary>
        public Guid? ActingAsUserId { get; set; }
        /// <summary>Gets or sets the impersonator identifier, when applicable.</summary>
        public Guid? ImpersonatorId { get; set; }
        /// <summary>Gets or sets the break-glass grant identifier, when applicable.</summary>
        public Guid? BreakGlassGrantId { get; set; }
        /// <summary>Gets or sets the action name.</summary>
        public string Action { get; set; } = string.Empty;
        /// <summary>Gets or sets the target entity type.</summary>
        public string EntityType { get; set; } = string.Empty;
        /// <summary>Gets or sets the target identifier.</summary>
        public string? EntityId { get; set; }
        /// <summary>Gets or sets a safe, non-secret description of the action.</summary>
        public string? Details { get; set; }
        /// <summary>Gets or sets when the operation occurred.</summary>
        public DateTimeOffset OccurredAt { get; set; }
    }
}
