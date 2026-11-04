namespace MKS.Gateway.API.Constants
{
    public static class GatewayConstants
    {
        public const string IdentityAuthorityConfigurationKey = "Identity:Authority";
        public const string CorsAllowedOriginsConfigurationSection = "Cors:AllowedOrigins";
        public const string CorsPolicyName = "ConfiguredOrigins";
        public const string ReverseProxyConfigurationSection = "ReverseProxy";
        public const string TenantHeaderPrefix = "X-Tenant-";
        public const string CorrelationIdHeader = "X-Correlation-ID";
        public const string UnknownRateLimitPartition = "unknown";
        public const string TestDisplayName = "Marketing Super App";

        public static class LocalizationKeys
        {
            public const string Title = "Test.Title";
            public const string Greeting = "Test.Greeting";
            public const string Welcome = "Test.Welcome";
        }
    }
}
