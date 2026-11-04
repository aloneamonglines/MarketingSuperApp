using MKS.Identity.Domain.Platform.Entities;

namespace MKS.Identity.Domain.Platform.Repositories
{
    /// <summary>Allowlisted ordering fields for break-glass grant queries.</summary>
    public enum BreakGlassGrantSortField
    {
        Id,
        TenantId,
        ExpiresAt,
        Status,
        RevokedAt
    }

    /// <summary>
    /// Defines persistence operations for break-glass grants.
    /// </summary>
    public interface IBreakGlassGrantRepository
    {
        /// <summary>Finds a grant by identifier.</summary>
        Task<BreakGlassGrantEntity?> GetAsync(Guid id, CancellationToken cancellationToken = default);
        /// <summary>Finds grants by identifiers, optionally constrained to one tenant.</summary>
        Task<IReadOnlyList<BreakGlassGrantEntity>> GetManyAsync(
            IReadOnlyCollection<Guid> ids,
            Guid? tenantId = null,
            CancellationToken cancellationToken = default);
        /// <summary>Returns a tenant-filtered page of grants ordered by an allowlisted field.</summary>
        Task<(IReadOnlyList<BreakGlassGrantEntity> Items, long TotalCount)> GetPagedAsync(
            int skipCount,
            int maxResultCount,
            Guid? tenantId,
            BreakGlassGrantSortField sortField,
            bool descending,
            CancellationToken cancellationToken = default);
        /// <summary>Persists a grant.</summary>
        Task AddAsync(BreakGlassGrantEntity grant, CancellationToken cancellationToken = default);
        /// <summary>Persists changes to a grant.</summary>
        Task UpdateAsync(BreakGlassGrantEntity grant, CancellationToken cancellationToken = default);
    }
}
