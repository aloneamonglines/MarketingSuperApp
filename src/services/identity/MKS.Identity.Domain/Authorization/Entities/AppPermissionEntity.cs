using MKS.Shared.Domain.Apps.Entities;
using MKS.Shared.Domain.Common;
using MKS.Shared.Domain.Shared.Attributes;
using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Identity.Domain.Authorization.Entities
{
    /// <summary>
    /// Represents a code-declared application permission.
    /// </summary>
    [EntityScope(CEntityScope.App)]
    public sealed class AppPermissionEntity : FullAuditEntity, IHasAppEntity
    {
        /// <inheritdoc />
        public Guid AppId { get; set; }
        /// <summary>Gets or sets the stable permission code.</summary>
        public string Code { get; set; } = string.Empty;
        /// <summary>Gets or sets the parent function code.</summary>
        public string? FunctionCode { get; set; }
        /// <summary>Gets or sets the localized display key.</summary>
        public string NameKey { get; set; } = string.Empty;
        /// <summary>Gets or sets whether this catalog entry is deprecated.</summary>
        public bool Deprecated { get; set; }
    }
}
