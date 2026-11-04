using MKS.Identity.Domain.MultiTenancy.Entities;

namespace MKS.Identity.Domain.MultiTenancy.Repositories
{
    /// <summary>
    /// Defines concurrency-safe tenant administrator assignment operations.
    /// </summary>
    public interface ITenantAdminAssignmentRepository
    {
        Task AddAsync(TenantAdminAssignmentEntity assignment, CancellationToken cancellationToken = default);
        /// <summary>Finds an administrator assignment in a tenant.</summary>
        Task<TenantAdminAssignmentEntity?> FindAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
        /// <summary>Persists an administrator assignment update.</summary>
        Task UpdateAsync(TenantAdminAssignmentEntity assignment, CancellationToken cancellationToken = default);
        /// <summary>Serializes administrator changes for the specified tenant.</summary>
        Task ExecuteForTenantAsync(Guid tenantId, Func<CancellationToken, Task> action, CancellationToken cancellationToken = default);
        /// <summary>Counts active tenant administrator assignments.</summary>
        Task<int> CountActiveAsync(Guid tenantId, CancellationToken cancellationToken = default);
    }
}
