using MKS.Shared.Domain.Apps.Entities;
using MKS.Shared.Domain.Common;
using MKS.Shared.Domain.Shared.Attributes;
using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Identity.Domain.Authorization.Entities
{
    /// <summary>
    /// Represents a code-declared feature grouping for permissions.
    /// </summary>
    [EntityScope(CEntityScope.App)]
    public sealed class AppFunctionEntity : FullAuditEntity, IHasAppEntity
    {
        /// <inheritdoc />
        public Guid AppId { get; set; }
        /// <summary>Gets or sets the stable function code.</summary>
        public string Code { get; set; } = string.Empty;
        /// <summary>Gets or sets the localized display key.</summary>
        public string NameKey { get; set; } = string.Empty;
        /// <summary>Gets or sets whether this catalog entry is deprecated.</summary>
        public bool Deprecated { get; set; }
    }
}
