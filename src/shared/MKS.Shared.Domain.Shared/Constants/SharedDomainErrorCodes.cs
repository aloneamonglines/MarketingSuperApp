namespace MKS.Shared.Domain.Shared.Constants
{
    /// <summary>
    /// Defines localization keys for shared domain errors.
    /// </summary>
    public static class SharedDomainErrorCodes
    {
        /// <summary>The required localization key was not provided.</summary>
        public const string LocalizationKeyRequired = "Shared.Errors.LocalizationKeyRequired";

        /// <summary>Contains localization keys for address value object validation.</summary>
        public static class Address
        {
            public const string RequiredComponent = "Shared.Errors.Address.RequiredComponent";
            public const string ComponentTooLong = "Shared.Errors.Address.ComponentTooLong";
            public const string CountryCodeRequired = "Shared.Errors.Address.CountryCodeRequired";
            public const string CountryCodeInvalid = "Shared.Errors.Address.CountryCodeInvalid";
        }

        /// <summary>Contains localization keys for image value object validation.</summary>
        public static class Image
        {
            public const string UrlRequired = "Shared.Errors.Image.UrlRequired";
            public const string UrlTooLong = "Shared.Errors.Image.UrlTooLong";
            public const string UrlSchemeInvalid = "Shared.Errors.Image.UrlSchemeInvalid";
            public const string UrlHostInvalid = "Shared.Errors.Image.UrlHostInvalid";
            public const string UrlNetworkPathNotAllowed = "Shared.Errors.Image.UrlNetworkPathNotAllowed";
            public const string UrlBackslashNotAllowed = "Shared.Errors.Image.UrlBackslashNotAllowed";
            public const string FileNameTooLong = "Shared.Errors.Image.FileNameTooLong";
            public const string FileNameInvalid = "Shared.Errors.Image.FileNameInvalid";
            public const string AltTooLong = "Shared.Errors.Image.AltTooLong";
            public const string ContentTypeTooLong = "Shared.Errors.Image.ContentTypeTooLong";
            public const string ContentTypeInvalid = "Shared.Errors.Image.ContentTypeInvalid";
            public const string FileSizeInvalid = "Shared.Errors.Image.FileSizeInvalid";
            public const string WidthInvalid = "Shared.Errors.Image.WidthInvalid";
            public const string HeightInvalid = "Shared.Errors.Image.HeightInvalid";
            public const string StorageKeyTooLong = "Shared.Errors.Image.StorageKeyTooLong";
            public const string StorageKeyAbsolute = "Shared.Errors.Image.StorageKeyAbsolute";
            public const string StorageKeyBackslashNotAllowed = "Shared.Errors.Image.StorageKeyBackslashNotAllowed";
            public const string StorageKeyUrlNotAllowed = "Shared.Errors.Image.StorageKeyUrlNotAllowed";
        }
    }
}
