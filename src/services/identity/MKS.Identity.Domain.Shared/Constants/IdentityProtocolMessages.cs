namespace MKS.Identity.Domain.Shared.Constants
{
    /// <summary>
    /// Defines localization keys for protocol descriptions returned by the Identity API.
    /// </summary>
    public static class IdentityProtocolMessages
    {
        /// <summary>Application scope is not accepted for break-glass authentication.</summary>
        public const string AppScopeNotAllowedForBreakGlass = "Identity.ProtocolErrors.AppScopeNotAllowedForBreakGlass";

        /// <summary>The application context is invalid.</summary>
        public const string InvalidAppContext = "Identity.ProtocolErrors.InvalidAppContext";

        /// <summary>The sign-in request is incomplete.</summary>
        public const string IncompleteSignInRequest = "Identity.ProtocolErrors.IncompleteSignInRequest";

        /// <summary>The tenant context is invalid.</summary>
        public const string InvalidTenantContext = "Identity.ProtocolErrors.InvalidTenantContext";

        /// <summary>A tenant context is required for break-glass authentication.</summary>
        public const string TenantRequiredForBreakGlass = "Identity.ProtocolErrors.TenantRequiredForBreakGlass";

        /// <summary>The break-glass grant identifier is invalid.</summary>
        public const string InvalidBreakGlassGrant = "Identity.ProtocolErrors.InvalidBreakGlassGrant";

        /// <summary>A break-glass grant identifier is only accepted for its custom grant type.</summary>
        public const string BreakGlassGrantOnlyForCustomGrant = "Identity.ProtocolErrors.BreakGlassGrantOnlyForCustomGrant";

        /// <summary>The credentials or access context are invalid.</summary>
        public const string InvalidCredentialsOrAccessContext = "Identity.ProtocolErrors.InvalidCredentialsOrAccessContext";

        /// <summary>The sign-in session is no longer active.</summary>
        public const string InactiveSignInSession = "Identity.ProtocolErrors.InactiveSignInSession";

        /// <summary>The refresh token scope does not match its sign-in session.</summary>
        public const string RefreshTokenScopeMismatch = "Identity.ProtocolErrors.RefreshTokenScopeMismatch";

        /// <summary>The refresh token has already been used.</summary>
        public const string RefreshTokenAlreadyUsed = "Identity.ProtocolErrors.RefreshTokenAlreadyUsed";

        /// <summary>The break-glass grant is no longer active.</summary>
        public const string InactiveBreakGlassGrant = "Identity.ProtocolErrors.InactiveBreakGlassGrant";

        /// <summary>The refresh-token principal is missing.</summary>
        public const string RefreshTokenPrincipalRequired = "The refresh-token principal is required.";

        /// <summary>The refresh-token principal has no mutable identity.</summary>
        public const string RefreshTokenIdentityRequired =
            "The refresh-token principal must contain a mutable identity.";
    }
}
