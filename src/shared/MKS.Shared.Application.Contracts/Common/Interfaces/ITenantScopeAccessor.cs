namespace MKS.Shared.Application.Contracts.Common.Interfaces
{
    /// <summary>
    /// Establishes a short-lived, application-validated tenant scope for persistence operations.
    /// </summary>
    public interface ITenantScopeAccessor
    {
        /// <summary>Gets the active trusted tenant scope.</summary>
        Guid? TenantId { get; }
        /// <summary>Enters a tenant scope for the lifetime of the returned handle.</summary>
        IDisposable Enter(Guid tenantId);
    }
}
