using Microsoft.EntityFrameworkCore;
using MKS.Identity.Domain.Platform.Entities;
using MKS.Identity.Domain.Platform.Repositories;
using MKS.Identity.Domain.Shared.Enums;
using MKS.Identity.Infrastructure.Persistence;
using MKS.Shared.Application.Contracts.Common.Constants;
using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Identity.Infrastructure.Platform
{
    /// <summary>
    /// Persists break-glass authorization grants.
    /// </summary>
    public sealed class BreakGlassGrantRepository(IdentityDbContext dbContext) : IBreakGlassGrantRepository
    {
        /// <inheritdoc />
        public Task<BreakGlassGrantEntity?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
            ActiveTenantGrants()
                .SingleOrDefaultAsync(grant => grant.Id == id, cancellationToken);

        /// <inheritdoc />
        public async Task<IReadOnlyList<BreakGlassGrantEntity>> GetManyAsync(
            IReadOnlyCollection<Guid> ids,
            Guid? tenantId = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(ids);
            if (ids.Count == 0)
            {
                return [];
            }

            var distinctIds = ids.Distinct().ToArray();
            var query = ActiveTenantGrants().Where(grant => distinctIds.Contains(grant.Id));
            if (tenantId is { } targetTenantId)
            {
                query = query.Where(grant => grant.TenantId == targetTenantId);
            }

            return await query.ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<(IReadOnlyList<BreakGlassGrantEntity> Items, long TotalCount)> GetPagedAsync(
            int skipCount,
            int maxResultCount,
            Guid? tenantId,
            BreakGlassGrantSortField sortField,
            bool descending,
            CancellationToken cancellationToken = default)
        {
            var query = ActiveTenantGrants();
            if (tenantId is { } targetTenantId)
            {
                query = query.Where(grant => grant.TenantId == targetTenantId);
            }

            var totalCount = await query.LongCountAsync(cancellationToken);
            IOrderedQueryable<BreakGlassGrantEntity> ordered = sortField switch
            {
                BreakGlassGrantSortField.Id => descending
                    ? query.OrderByDescending(grant => grant.Id)
                    : query.OrderBy(grant => grant.Id),
                BreakGlassGrantSortField.TenantId => descending
                    ? query.OrderByDescending(grant => grant.TenantId)
                    : query.OrderBy(grant => grant.TenantId),
                BreakGlassGrantSortField.ExpiresAt => descending
                    ? query.OrderByDescending(grant => grant.ExpiresAt)
                    : query.OrderBy(grant => grant.ExpiresAt),
                BreakGlassGrantSortField.Status => descending
                    ? query.OrderByDescending(grant => grant.Status)
                    : query.OrderBy(grant => grant.Status),
                BreakGlassGrantSortField.RevokedAt => descending
                    ? query.OrderByDescending(grant => grant.RevokedAt)
                    : query.OrderBy(grant => grant.RevokedAt),
                _ => throw new ArgumentOutOfRangeException(nameof(sortField))
            };

            var items = await ordered.ThenBy(grant => grant.Id)
                .Skip(skipCount)
                .Take(Math.Clamp(maxResultCount, 1, PagingConstants.MaxMaxResultCount))
                .ToListAsync(cancellationToken);
            return (items, totalCount);
        }

        /// <inheritdoc />
        public async Task AddAsync(BreakGlassGrantEntity grant, CancellationToken cancellationToken = default) =>
            await dbContext.BreakGlassGrants.AddAsync(grant, cancellationToken);

        /// <inheritdoc />
        public Task UpdateAsync(BreakGlassGrantEntity grant, CancellationToken cancellationToken = default)
        {
            dbContext.BreakGlassGrants.Update(grant);
            return Task.CompletedTask;
        }

        private IQueryable<BreakGlassGrantEntity> ActiveTenantGrants() =>
            dbContext.BreakGlassGrants.Where(grant =>
                dbContext.Tenants.Any(tenant =>
                    tenant.Id == grant.TenantId &&
                    tenant.MasterStatus == CMasterStatus.Active));
    }
}
