namespace MKS.Identity.Domain.Shared.Constants
{
    /// <summary>
    /// Defines stable Identity protocol and claim values shared across layers.
    /// </summary>
    public static class IdentityConstants
    {
        /// <summary>Gets the built-in platform administrator role code.</summary>
        public const string SuperAdminRoleCode = "SuperAdmin";

        /// <summary>Gets the custom OAuth grant type for break-glass access.</summary>
        public const string BreakGlassGrantType = "urn:mks:break-glass";

        /// <summary>Contains custom claim type names emitted by Identity.</summary>
        public static class Claims
        {
            /// <summary>The authenticated user identifier.</summary>
            public const string Subject = "sub";

            /// <summary>The sign-in session identifier.</summary>
            public const string SessionId = "sid";

            /// <summary>The application identifier.</summary>
            public const string AppId = "aid";

            /// <summary>The tenant identifier.</summary>
            public const string TenantId = "tid";

            /// <summary>The break-glass grant identifier.</summary>
            public const string BreakGlassGrantId = "bg";

            /// <summary>The impersonating user identifier.</summary>
            public const string ImpersonatorId = "impersonator_id";

            /// <summary>Indicates that the user must change a temporary password.</summary>
            public const string MustChangePassword = "must_change_password";

            /// <summary>The authorization membership version.</summary>
            public const string MembershipVersion = "membership_version";

            /// <summary>The refresh-token generation for a session.</summary>
            public const string RefreshGeneration = "refresh_generation";
        }

        /// <summary>Contains route constants used by Identity middleware.</summary>
        public static class ApiPaths
        {
            /// <summary>The endpoint used to change a password.</summary>
            public const string ChangePassword = "/api/identity/users/change-password";
        }

        /// <summary>Contains audit action codes emitted by Identity.</summary>
        public static class AuditActions
        {
            /// <summary>An audited break-glass request.</summary>
            public const string BreakGlassRequest = "break_glass.request";

            /// <summary>An app administrator was added.</summary>
            public const string AppAdminAdded = "app_admin.added";

            /// <summary>An app administrator was removed.</summary>
            public const string AppAdminRemoved = "app_admin.removed";

            /// <summary>A tenant user profile was updated.</summary>
            public const string TenantUserProfileUpdated = "tenant_user.profile_updated";

            /// <summary>A user's membership in a tenant was deactivated.</summary>
            public const string TenantUserMembershipDeactivated = "tenant_user.membership_deactivated";

            /// <summary>A tenant administrator was added.</summary>
            public const string TenantAdminAdded = "tenant_admin.added";

            /// <summary>A tenant administrator was removed.</summary>
            public const string TenantAdminRemoved = "tenant_admin.removed";

            /// <summary>A break-glass grant was created.</summary>
            public const string BreakGlassCreated = "break_glass.created";

            /// <summary>A break-glass grant was revoked.</summary>
            public const string BreakGlassRevoked = "break_glass.revoked";
        }

        /// <summary>Contains permission codes used by Identity application services.</summary>
        public static class Permissions
        {
            /// <summary>Create an application.</summary>
            public const string PlatformAppsCreate = "platform:apps:create";

            /// <summary>Create a break-glass grant.</summary>
            public const string PlatformBreakGlassCreate = "platform:break-glass:create";

            /// <summary>Revoke a break-glass grant.</summary>
            public const string PlatformBreakGlassRevoke = "platform:break-glass:revoke";

            /// <summary>Create a user in a tenant.</summary>
            public const string TenantAdminUsersCreate = "tenant:admin:users:create";

            /// <summary>Read users in a tenant.</summary>
            public const string TenantAdminUsersRead = "tenant:admin:users:read";

            /// <summary>Update user profiles in a tenant.</summary>
            public const string TenantAdminUsersUpdate = "tenant:admin:users:update";

            /// <summary>Deactivate user memberships in a tenant.</summary>
            public const string TenantAdminUsersDelete = "tenant:admin:users:delete";

            /// <summary>Manage tenant administrators.</summary>
            public const string TenantAdminAdminsManage = "tenant:admin:admins:manage";

            /// <summary>Wildcard permission granted to platform super administrators.</summary>
            public const string PlatformSuperAdminWildcard = "platform:SuperAdmin:*";
        }

        /// <summary>Contains permission-code prefixes used for scope checks.</summary>
        public static class PermissionPrefixes
        {
            /// <summary>Platform-scoped permissions.</summary>
            public const string Platform = "platform:";

            /// <summary>Tenant-administration permissions.</summary>
            public const string TenantAdmin = "tenant:admin:";
        }

        /// <summary>Contains Identity configuration keys.</summary>
        public static class ConfigurationKeys
        {
            /// <summary>The Identity database connection string.</summary>
            public const string IdentityConnectionString = "ConnectionStrings:Identity";

            /// <summary>The Identity issuer URL.</summary>
            public const string Issuer = "Identity:Issuer";

            /// <summary>The access-token lifetime in minutes.</summary>
            public const string AccessTokenMinutes = "Identity:AccessTokenMinutes";

            /// <summary>The refresh-token lifetime in days.</summary>
            public const string RefreshTokenDays = "Identity:RefreshTokenDays";

            /// <summary>The temporary password used when provisioning users.</summary>
            public const string DefaultPassword = "Identity:DefaultPassword";

            /// <summary>The failed sign-in threshold.</summary>
            public const string FailedSignInThreshold = "Identity:FailedSignInThreshold";

            /// <summary>The sign-in lockout duration in minutes.</summary>
            public const string LockoutMinutes = "Identity:LockoutMinutes";

            /// <summary>The maximum break-glass grant duration in minutes.</summary>
            public const string BreakGlassMaximumMinutes = "Identity:BreakGlassMaximumMinutes";

            /// <summary>The initial platform administrator email address.</summary>
            public const string SuperAdminEmail = "Identity:SuperAdmin:Email";

            /// <summary>The initial platform administrator password.</summary>
            public const string SuperAdminPassword = "Identity:SuperAdmin:Password";
        }

        /// <summary>Contains configuration failure messages used by Identity startup.</summary>
        public static class ConfigurationErrors
        {
            /// <summary>The issuer setting is missing.</summary>
            public const string IssuerRequired = "Identity:Issuer is required.";

            /// <summary>The Identity database connection string is missing.</summary>
            public const string IdentityConnectionStringRequired = "ConnectionStrings:Identity is required.";

            /// <summary>The Identity migration connection string is missing.</summary>
            public const string IdentityMigrationConnectionStringRequired =
                "ConnectionStrings:IdentityMigration is required outside development.";

            /// <summary>The Identity options contain invalid values.</summary>
            public const string IdentityOptionsInvalid =
                "Identity options require an absolute issuer, positive token lifetimes, and positive lockout settings.";
            /// <summary>The issuer setting is not an absolute URI.</summary>
            public const string IssuerMustBeAbsolute = "Identity:Issuer must be an absolute URI.";

            /// <summary>An access-token or refresh-token lifetime is zero or negative.</summary>
            public const string TokenLifetimeMustBePositive =
                "Identity:AccessTokenMinutes and Identity:RefreshTokenDays must be greater than zero.";

            /// <summary>The bootstrap administrator credentials are missing.</summary>
            public const string SuperAdminCredentialsRequired =
                "Identity:SuperAdmin:Email and Identity:SuperAdmin:Password must be set using deployment secrets.";

            /// <summary>The signing certificate settings are invalid.</summary>
            public const string CertificateConfigurationInvalid =
                "Identity:{0}Certificate:Path and Password must reference a valid certificate in non-development environments.";

            /// <summary>A tenant scope cannot be entered by the design-time context.</summary>
            public const string TenantScopeUnavailableAtDesignTime =
                "Tenant scope is unavailable at design time.";
        }

        /// <summary>Contains validation exception messages used by Identity entities.</summary>
        public static class ValidationMessages
        {
            /// <summary>The email address must be a single address.</summary>
            public const string SingleEmailAddressRequired = "The email must be a single address.";

            /// <summary>The phone number must use E.164 format.</summary>
            public const string PhoneMustUseE164 = "The phone must use E.164 format.";

            /// <summary>The username does not meet the supported format.</summary>
            public const string UsernameInvalid = "The username is invalid.";

            /// <summary>The email address is invalid.</summary>
            public const string EmailInvalid = "The email address is invalid.";

            /// <summary>The phone number must use E.164 format.</summary>
            public const string PhoneMustBeE164 = "Phone numbers must be provided in E.164 format.";

            /// <summary>The username must contain between 3 and 64 printable characters.</summary>
            public const string UsernameLengthInvalid = "The username must contain between 3 and 64 printable characters.";

            /// <summary>The permission catalog must include its app, functions, and permissions.</summary>
            public const string PermissionCatalogRequired =
                "Permission catalog app code, functions, and permissions are required.";

            /// <summary>An integration event identifier must be present.</summary>
            public const string IntegrationEventIdRequired =
                "An integration event must have a non-empty event identifier.";

            /// <summary>No consumer is registered for the event type.</summary>
            public const string IntegrationEventConsumerNotRegistered =
                "No consumer is registered for integration-event type '{0}'.";

            /// <summary>The serialized integration event is invalid.</summary>
            public const string IntegrationEventPayloadInvalid =
                "Outbox payload '{0}' is not a valid integration event.";

            /// <summary>The event identifier in the payload does not match the outbox row.</summary>
            public const string IntegrationEventIdMismatch =
                "Outbox event identifier does not match the payload for '{0}'.";

            /// <summary>The event scope metadata does not match its payload.</summary>
            public const string IntegrationEventScopeMismatch =
                "Outbox event scope metadata does not match its payload for '{0}'.";

            /// <summary>The requested message scope filter has inconsistent identifiers.</summary>
            public const string IntegrationEventScopeFilterInvalid =
                "The message scope filter has an invalid identifier combination.";

            /// <summary>The persisted message scope metadata is inconsistent.</summary>
            public const string IntegrationEventScopeMetadataInvalid =
                "The message scope metadata has an invalid identifier combination.";

            /// <summary>Outbox retry delays must be positive.</summary>
            public const string RetryDelaysMustBePositive = "Retry delays must be positive.";

            /// <summary>Outbox configuration contains invalid values.</summary>
            public const string OutboxOptionsInvalid =
                "Outbox processing options must use positive values and a max retry delay no smaller than the initial delay.";
        }

        /// <summary>Contains stable resource identifiers.</summary>
        public static class Resources
        {
            /// <summary>The Identity resource server.</summary>
            public const string Identity = "identity";

            /// <summary>The API gateway resource server.</summary>
            public const string Gateway = "gateway";
        }

        /// <summary>Contains stable audit entity type codes.</summary>
        public static class EntityTypes
        {
            /// <summary>An HTTP request.</summary>
            public const string HttpRequest = "HttpRequest";
        }
    }
}
