using MKS.Shared.Domain.Common;
using MKS.Shared.Domain.Shared.Attributes;
using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Identity.Domain.Platform.Entities
{
    /// <summary>
    /// Assigns a platform-level role to a global user.
    /// </summary>
    [EntityScope(CEntityScope.Global)]
    public sealed class PlatformRoleAssignmentEntity : FullAuditEntity
    {
        /// <summary>Gets or sets the user identifier.</summary>
        public Guid UserId { get; set; }
        /// <summary>Gets or sets the platform role code.</summary>
        public string RoleCode { get; set; } = string.Empty;
        /// <summary>Gets or sets whether the assignment is active.</summary>
        public bool IsActive { get; set; } = true;
    }
}
