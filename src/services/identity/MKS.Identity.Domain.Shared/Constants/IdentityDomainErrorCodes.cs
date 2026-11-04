namespace MKS.Identity.Domain.Shared.Constants
{
    /// <summary>
    /// Defines localization keys for Identity domain and application errors.
    /// </summary>
    public static class IdentityDomainErrorCodes
    {
        /// <summary>Invalid input was supplied.</summary>
        public const string InvalidInput = "Identity.Errors.InvalidInput";

        /// <summary>The requested scope is not permitted.</summary>
        public const string InvalidScope = "Identity.Errors.InvalidScope";

        /// <summary>The supplied credentials are invalid.</summary>
        public const string InvalidCredentials = "Identity.Errors.InvalidCredentials";

        /// <summary>An account is temporarily locked.</summary>
        public const string AccountLocked = "Identity.Errors.AccountLocked";

        /// <summary>A unique identifier is already in use.</summary>
        public const string IdentifierAlreadyExists = "Identity.Errors.IdentifierAlreadyExists";

        /// <summary>At least one identifier is required.</summary>
        public const string IdentifierRequired = "Identity.Errors.IdentifierRequired";

        /// <summary>The identifier is invalid.</summary>
        public const string InvalidIdentifier = "Identity.Errors.InvalidIdentifier";

        /// <summary>The password does not satisfy the configured policy.</summary>
        public const string PasswordPolicy = "Identity.Errors.PasswordPolicy";

        /// <summary>The last administrator assignment cannot be removed.</summary>
        public const string LastAdministrator = "Identity.Errors.LastAdministrator";

        /// <summary>The break-glass grant has expired or is no longer active.</summary>
        public const string BreakGlassExpired = "Identity.Errors.BreakGlassExpired";

        /// <summary>A break-glass reason is required.</summary>
        public const string BreakGlassReason = "Identity.Errors.BreakGlassReason";

        /// <summary>The break-glass duration is invalid.</summary>
        public const string BreakGlassDuration = "Identity.Errors.BreakGlassDuration";

        /// <summary>The account must change its temporary password.</summary>
        public const string MustChangePassword = "Identity.Errors.MustChangePassword";
    }
}
