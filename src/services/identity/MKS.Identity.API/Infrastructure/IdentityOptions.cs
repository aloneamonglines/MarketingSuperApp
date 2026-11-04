namespace MKS.Identity.API.Infrastructure
{
    /// <summary>Options bound to the Identity configuration section.</summary>
    public sealed record IdentityOptions
    {
        public const string SectionName = "Identity";

        public string Issuer { get; init; } = string.Empty;
        public string DefaultPassword { get; init; } = string.Empty;
        public int FailedSignInThreshold { get; init; } = 5;
        public int LockoutMinutes { get; init; } = 15;
        public int BreakGlassMaximumMinutes { get; init; } = 60;
        public int AccessTokenMinutes { get; init; } = 8;
        public int RefreshTokenDays { get; init; } = 14;
        public CertificateOptions SigningCertificate { get; init; } = new();
        public CertificateOptions EncryptionCertificate { get; init; } = new();
        public SuperAdminOptions SuperAdmin { get; init; } = new();

        public sealed record CertificateOptions
        {
            public string Path { get; init; } = string.Empty;
            public string Password { get; init; } = string.Empty;
        }

        public sealed record SuperAdminOptions
        {
            public string Email { get; init; } = string.Empty;
            public string Password { get; init; } = string.Empty;
        }
    }
}
