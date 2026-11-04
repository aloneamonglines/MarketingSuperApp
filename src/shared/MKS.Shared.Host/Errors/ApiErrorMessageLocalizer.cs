using Microsoft.Extensions.Localization;
using MKS.Shared.Domain.Shared.Localization;

namespace MKS.Shared.Host.Errors
{
    /// <summary>Resolves localized messages for standardized API error codes.</summary>
    public interface IApiErrorMessageLocalizer
    {
        string Localize(string code, params object[] arguments);
    }

    /// <summary>Resolves shared error messages and falls back to the error code.</summary>
    public sealed class SharedApiErrorMessageLocalizer(
        IStringLocalizer<SharedResource> localizer) : IApiErrorMessageLocalizer
    {
        public string Localize(string code, params object[] arguments)
        {
            var localized = localizer[code, arguments];
            return localized.ResourceNotFound ? code : localized.Value;
        }
    }
}
