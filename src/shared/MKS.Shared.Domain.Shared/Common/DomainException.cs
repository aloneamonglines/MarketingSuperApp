using MKS.Shared.Domain.Shared.Constants;

namespace MKS.Shared.Domain.Shared.Common
{
    /// <summary>
    /// Represents a business rule violation identified by a localization key.
    /// </summary>
    public sealed class DomainException : Exception
    {
        public DomainException(
            string localizationKey,
            Exception? innerException = null,
            params object[] localizationArguments)
            : base(localizationKey, innerException)
        {
            LocalizationKey = string.IsNullOrWhiteSpace(localizationKey)
                ? throw new ArgumentException(
                    SharedDomainErrorCodes.LocalizationKeyRequired,
                    nameof(localizationKey))
                : localizationKey;
            LocalizationArguments = localizationArguments ?? [];
        }

        /// <summary>Gets the localization key describing the violated rule.</summary>
        public string LocalizationKey { get; }

        /// <summary>Gets the values used to format the localized error message.</summary>
        public object[] LocalizationArguments { get; }
    }
}
