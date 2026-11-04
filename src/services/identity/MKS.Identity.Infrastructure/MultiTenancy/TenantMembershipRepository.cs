using Microsoft.EntityFrameworkCore;
using MKS.Identity.Domain.MultiTenancy.Entities;
using MKS.Identity.Domain.MultiTenancy.Repositories;

namespace MKS.Identity.Infrastructure.MultiTenancy
{
    /// <summary>
    /// Persists user membership within tenants.
    /// </summary>
    public sealed class TenantMembershipRepository(Persistence.IdentityDbContext dbContext) : ITenantMembershipRepository
    {
        /// <inheritdoc />
        public Task<TenantMembershipEntity?> FindAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default) =>
            dbContext.TenantMemberships.SingleOrDefaultAsync(
                membership => membership.UserId == userId && membership.TenantId == tenantId,
                cancellationToken);

        /// <inheritdoc />
        public async Task AddAsync(TenantMembershipEntity membership, CancellationToken cancellationToken = default) =>
            await dbContext.TenantMemberships.AddAsync(membership, cancellationToken);

        /// <inheritdoc />
        public Task UpdateAsync(TenantMembershipEntity membership, CancellationToken cancellationToken = default)
        {
            dbContext.TenantMemberships.Update(membership);
            return Task.CompletedTask;
        }
    }
}
