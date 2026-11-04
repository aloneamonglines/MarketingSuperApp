using MKS.Identity.Domain.Shared.Constants;
using MKS.Shared.Domain.Common;
using MKS.Shared.Domain.Shared.Attributes;
using MKS.Shared.Domain.Shared.Common;
using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Identity.Domain.Apps.Entities
{
    /// <summary>
    /// Represents a globally registered application.
    /// </summary>
    [EntityScope(CEntityScope.Global)]
    public sealed class AppEntity : FullAuditEntity, IHasMasterStatusEntity
    {
        private AppEntity()
        {
        }

        /// <summary>Gets or sets the display name.</summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>Gets or sets the unique application code.</summary>
        public string Code { get; set; } = string.Empty;
        /// <summary>Gets or sets the application description.</summary>
        public string? Description { get; set; }
        /// <summary>Gets or sets the application logo URL.</summary>
        public string? LogoUrl { get; set; }
        /// <inheritdoc />
        public CMasterStatus MasterStatus { get; set; } = CMasterStatus.Active;

        /// <summary>Creates an application with a valid active status.</summary>
        public static AppEntity Create(string name, string code, string? description = null, string? logoUrl = null)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(code))
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }

            return new AppEntity
            {
                Name = name.Trim(),
                Code = code.Trim().ToLowerInvariant(),
                Description = description?.Trim(),
                LogoUrl = logoUrl?.Trim(),
                MasterStatus = CMasterStatus.Active
            };
        }
    }
}
