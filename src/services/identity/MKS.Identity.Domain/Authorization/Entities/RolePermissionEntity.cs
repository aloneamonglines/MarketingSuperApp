using MKS.Shared.Domain.Apps.Entities;
using MKS.Shared.Domain.Common;
using MKS.Shared.Domain.Shared.Attributes;
using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Identity.Domain.Authorization.Entities
{
    /// <summary>
    /// Associates an application role with a permission catalog entry.
    /// </summary>
    [EntityScope(CEntityScope.App)]
    public sealed class RolePermissionEntity : FullAuditEntity, IHasAppEntity
    {
        /// <inheritdoc />
        public Guid AppId { get; set; }
        /// <summary>Gets or sets the role identifier.</summary>
        public Guid RoleId { get; set; }
        /// <summary>Gets or sets the permission code.</summary>
        public string PermissionCode { get; set; } = string.Empty;
    }
}
