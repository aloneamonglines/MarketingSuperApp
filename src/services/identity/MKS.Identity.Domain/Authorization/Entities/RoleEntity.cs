using MKS.Shared.Domain.Apps.Entities;
using MKS.Shared.Domain.Common;
using MKS.Shared.Domain.MultiTenancy.Entities;
using MKS.Shared.Domain.Shared.Attributes;
using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Identity.Domain.Authorization.Entities
{
    /// <summary>
    /// Represents an application-wide or tenant-specific permission role.
    /// </summary>
    [EntityScope(CEntityScope.App)]
    public sealed class RoleEntity : FullAuditEntity, IHasMasterStatusEntity, IHasAppEntity, IMayHaveTenantEntity
    {
        /// <summary>Gets or sets the application identifier.</summary>
        public Guid AppId { get; set; }
        /// <summary>Gets or sets the tenant identifier; null represents an app-wide role.</summary>
        public Guid? TenantId { get; set; }
        /// <summary>Gets or sets the role name.</summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>Gets or sets the unique role code within its scope.</summary>
        public string Code { get; set; } = string.Empty;
        /// <inheritdoc />
        public CMasterStatus MasterStatus { get; set; } = CMasterStatus.Active;
    }
}
