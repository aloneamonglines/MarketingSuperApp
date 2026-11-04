using MKS.Shared.Domain.Common;
using MKS.Shared.Domain.MultiTenancy.Entities;
using MKS.Shared.Domain.Shared.Attributes;
using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Identity.Domain.Authorization.Entities
{
    /// <summary>
    /// Assigns a role to a tenant membership.
    /// </summary>
    [EntityScope(CEntityScope.Tenant)]
    public sealed class RoleAssignmentEntity : FullAuditEntity, IHasTenantEntity
    {
        /// <inheritdoc />
        public Guid TenantId { get; set; }
        /// <summary>Gets or sets the assigned user identifier.</summary>
        public Guid UserId { get; set; }
        /// <summary>Gets or sets the role identifier.</summary>
        public Guid RoleId { get; set; }
        /// <summary>Gets or sets the application identifier used to validate role scope.</summary>
        public Guid AppId { get; set; }
        /// <summary>Gets or sets whether the assignment is active.</summary>
        public bool IsActive { get; set; } = true;
    }
}
