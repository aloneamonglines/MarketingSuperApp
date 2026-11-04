namespace MKS.Identity.Infrastructure.DependencyInjection
{
    /// <summary>Connection strings used by Identity runtime and database migrations.</summary>
    public sealed record IdentityConnectionStringsOptions
    {
        public const string SectionName = "ConnectionStrings";

        public string Identity { get; init; } = string.Empty;
        public string IdentityMigration { get; init; } = string.Empty;
    }
}
