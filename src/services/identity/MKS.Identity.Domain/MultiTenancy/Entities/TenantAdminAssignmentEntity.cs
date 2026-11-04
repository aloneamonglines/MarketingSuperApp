using MKS.Shared.Domain.Common;
using MKS.Shared.Domain.MultiTenancy.Entities;
using MKS.Shared.Domain.Shared.Attributes;
using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Identity.Domain.MultiTenancy.Entities
{
    /// <summary>
    /// Grants tenant administration rights to a member of that tenant.
    /// </summary>
    [EntityScope(CEntityScope.Tenant)]
    public sealed class TenantAdminAssignmentEntity : FullAuditEntity, IHasTenantEntity
    {
        /// <inheritdoc />
        public Guid TenantId { get; set; }
        /// <summary>Gets or sets the member user identifier.</summary>
        public Guid UserId { get; set; }
        /// <summary>Gets or sets whether the assignment is active.</summary>
        public bool IsActive { get; set; } = true;
    }
}
