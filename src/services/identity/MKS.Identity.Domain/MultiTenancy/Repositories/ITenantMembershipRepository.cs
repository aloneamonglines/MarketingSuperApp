using MKS.Identity.Domain.MultiTenancy.Entities;

namespace MKS.Identity.Domain.MultiTenancy.Repositories
{
    /// <summary>
    /// Defines persistence operations for tenant membership.
    /// </summary>
    public interface ITenantMembershipRepository
    {
        /// <summary>Finds a user's membership in a tenant.</summary>
        Task<TenantMembershipEntity?> FindAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);
        /// <summary>Adds a membership.</summary>
        Task AddAsync(TenantMembershipEntity membership, CancellationToken cancellationToken = default);
        /// <summary>Persists membership and user version changes.</summary>
        Task UpdateAsync(TenantMembershipEntity membership, CancellationToken cancellationToken = default);
    }
}
