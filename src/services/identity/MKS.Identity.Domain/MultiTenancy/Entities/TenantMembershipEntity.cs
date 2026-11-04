using MKS.Identity.Domain.Shared.Enums;
using MKS.Shared.Domain.Common;
using MKS.Shared.Domain.MultiTenancy.Entities;
using MKS.Shared.Domain.Shared.Attributes;
using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Identity.Domain.MultiTenancy.Entities
{
    /// <summary>
    /// Associates a global user with an application tenant.
    /// </summary>
    [EntityScope(CEntityScope.Tenant)]
    public sealed class TenantMembershipEntity : FullAuditEntity, IHasTenantEntity
    {
        /// <inheritdoc />
        public Guid TenantId { get; set; }
        /// <summary>Gets or sets the associated user identifier.</summary>
        public Guid UserId { get; set; }
        /// <summary>Gets or sets the application identifier used to validate scope.</summary>
        public Guid AppId { get; set; }
        /// <summary>Gets or sets the membership lifecycle state.</summary>
        public CMembershipStatus Status { get; set; } = CMembershipStatus.Active;
    }
}
