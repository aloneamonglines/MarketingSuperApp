using Microsoft.Extensions.Localization;
using MKS.Identity.Domain.Shared.Localization;
using MKS.Shared.Domain.Shared.Localization;
using MKS.Shared.Host.Errors;

namespace MKS.Identity.API.Infrastructure
{
    /// <summary>Resolves localized Identity and Shared API error codes.</summary>
    public sealed class IdentityApiErrorMessageLocalizer(
        IStringLocalizer<IdentityResource> identityLocalizer,
        IStringLocalizer<SharedResource> sharedLocalizer) : IApiErrorMessageLocalizer
    {
        public string Localize(string code, params object[] arguments)
        {
            IStringLocalizer localizer = code.StartsWith("Identity.", StringComparison.Ordinal)
                ? identityLocalizer
                : sharedLocalizer;
            var localized = localizer[code, arguments];
            return localized.ResourceNotFound ? code : localized.Value;
        }
    }
}
