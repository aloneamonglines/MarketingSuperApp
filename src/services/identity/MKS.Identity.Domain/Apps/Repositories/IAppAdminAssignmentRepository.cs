using MKS.Identity.Domain.Apps.Entities;

namespace MKS.Identity.Domain.Apps.Repositories
{
    /// <summary>
    /// Defines concurrency-safe administration assignment operations.
    /// </summary>
    public interface IAppAdminAssignmentRepository
    {
        Task AddAsync(AppAdminAssignmentEntity assignment, CancellationToken cancellationToken = default);
        /// <summary>Finds an administrator assignment in an application.</summary>
        Task<AppAdminAssignmentEntity?> FindAsync(Guid appId, Guid userId, CancellationToken cancellationToken = default);
        /// <summary>Persists an administrator assignment update.</summary>
        Task UpdateAsync(AppAdminAssignmentEntity assignment, CancellationToken cancellationToken = default);
        /// <summary>Serializes administrator changes for the specified application.</summary>
        Task ExecuteForAppAsync(Guid appId, Func<CancellationToken, Task> action, CancellationToken cancellationToken = default);
        /// <summary>Counts active application administrator assignments.</summary>
        Task<int> CountActiveAsync(Guid appId, CancellationToken cancellationToken = default);
    }
}
