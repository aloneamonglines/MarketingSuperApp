namespace MKS.Shared.Infrastructure.Persistence
{
    /// <summary>
    /// Configures conventions used by shared persistence infrastructure.
    /// </summary>
    public sealed class SharedPersistenceOptions
    {
        /// <summary>Gets or sets the PostgreSQL session variable used for tenant isolation.</summary>
        public string TenantSettingName { get; set; } = "app.current_tenant_id";
    }
}
