namespace MKS.Shared.Domain.Shared.Constants
{
    /// <summary>Defines localization keys used by FluentValidation rules in shared contracts.</summary>
    public static class SharedValidationErrorCodes
    {
        public const string Required = "Shared.Validation.Required";
        public const string Invalid = "Shared.Validation.Invalid";
        public const string TooShort = "Shared.Validation.TooShort";
        public const string TooLong = "Shared.Validation.TooLong";
        public const string OutOfRange = "Shared.Validation.OutOfRange";
        public const string CollectionEmpty = "Shared.Validation.CollectionEmpty";
        public const string CollectionTooLarge = "Shared.Validation.CollectionTooLarge";
    }
}
