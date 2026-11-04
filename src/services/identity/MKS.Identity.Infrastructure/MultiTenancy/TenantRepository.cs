using Microsoft.EntityFrameworkCore;
using MKS.Identity.Domain.MultiTenancy.Entities;
using MKS.Shared.Domain.MultiTenancy.Repositories;

namespace MKS.Identity.Infrastructure.MultiTenancy
{
    /// <summary>
    /// Persists application tenants.
    /// </summary>
    public sealed class TenantRepository(Persistence.IdentityDbContext dbContext) : ITenantRepository<TenantEntity>
    {
        /// <inheritdoc />
        public Task<TenantEntity?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
            dbContext.Tenants.SingleOrDefaultAsync(tenant => tenant.Id == id, cancellationToken);

        /// <inheritdoc />
        public Task<bool> CodeExistsAsync(Guid appId, string normalizedCode, CancellationToken cancellationToken = default) =>
            dbContext.Tenants.AnyAsync(
                tenant => tenant.AppId == appId && tenant.Code == normalizedCode,
                cancellationToken);

        /// <inheritdoc />
        public Task<bool> AnyCodeExistsAsync(
            Guid appId,
            IReadOnlyCollection<string> normalizedCodes,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(normalizedCodes);
            if (normalizedCodes.Count == 0)
            {
                return Task.FromResult(false);
            }

            var codes = normalizedCodes.Distinct(StringComparer.Ordinal).ToArray();
            return dbContext.Tenants.AnyAsync(
                tenant => tenant.AppId == appId && codes.Contains(tenant.Code),
                cancellationToken);
        }

        /// <inheritdoc />
        public async Task AddAsync(TenantEntity tenant, CancellationToken cancellationToken = default) =>
            await dbContext.Tenants.AddAsync(tenant, cancellationToken);
    }
}
