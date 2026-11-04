using MKS.Identity.Domain.Shared.Constants;
using MKS.Shared.Domain.Apps.Entities;
using MKS.Shared.Domain.Common;
using MKS.Shared.Domain.Shared.Attributes;
using MKS.Shared.Domain.Shared.Common;
using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Identity.Domain.MultiTenancy.Entities
{
    /// <summary>
    /// Represents a tenant owned by exactly one application.
    /// </summary>
    [EntityScope(CEntityScope.App)]
    public sealed class TenantEntity : FullAuditEntity, IHasMasterStatusEntity, IHasAppEntity
    {
        private TenantEntity()
        {
        }

        /// <inheritdoc />
        public Guid AppId { get; set; }
        /// <summary>Gets or sets the display name.</summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>Gets or sets the unique tenant code within its application.</summary>
        public string Code { get; set; } = string.Empty;
        /// <inheritdoc />
        public CMasterStatus MasterStatus { get; set; } = CMasterStatus.Active;

        /// <summary>Creates a tenant with a valid active status.</summary>
        public static TenantEntity Create(Guid appId, string name, string code)
        {
            if (appId == Guid.Empty)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(code))
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }
            return new TenantEntity
            {
                AppId = appId,
                Name = name.Trim(),
                Code = code.Trim().ToLowerInvariant(),
                MasterStatus = CMasterStatus.Active
            };
        }
    }
}
