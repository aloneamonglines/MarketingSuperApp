using Microsoft.EntityFrameworkCore;
using MKS.Identity.Domain.Users.Entities;
using MKS.Identity.Domain.Users.Repositories;
using MKS.Identity.Infrastructure.Persistence;
using MKS.Identity.Domain.MultiTenancy.Entities;
using MKS.Identity.Domain.Shared.Enums;
using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Identity.Infrastructure.Users
{
    /// <summary>
    /// Persists global user profiles, identifiers, and credentials.
    /// </summary>
    public sealed class UserRepository(IdentityDbContext dbContext) : IUserRepository
    {
        /// <inheritdoc />
        public async Task<(UserEntity UserEntity, UserIdentifierEntity Identifier, UserCredentialEntity Credential)?> FindForSignInAsync(
            string normalizedIdentifier,
            CancellationToken cancellationToken = default)
        {
            var identifier = await dbContext.UserIdentifiers
                .SingleOrDefaultAsync(
                    item => item.NormalizedValue == normalizedIdentifier,
                    cancellationToken);
            if (identifier is null)
            {
                return null;
            }

            var user = await dbContext.Users.SingleOrDefaultAsync(
                item => item.Id == identifier.UserId,
                cancellationToken);
            var credential = await dbContext.UserCredentials.SingleOrDefaultAsync(
                item => item.UserId == identifier.UserId,
                cancellationToken);
            return user is null || credential is null ? null : (user, identifier, credential);
        }

        /// <inheritdoc />
        public Task<UserEntity?> FindByAnyIdentifierAsync(
            IReadOnlyCollection<string> normalizedIdentifiers,
            CancellationToken cancellationToken = default)
        {
            return dbContext.Users
                .Where(user => dbContext.UserIdentifiers.Any(identifier =>
                    identifier.UserId == user.Id &&
                    normalizedIdentifiers.Contains(identifier.NormalizedValue)))
                .SingleOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc />
        public Task<UserCredentialEntity?> GetCredentialAsync(Guid userId, CancellationToken cancellationToken = default) =>
            dbContext.UserCredentials.SingleOrDefaultAsync(
                credential => credential.UserId == userId,
                cancellationToken);

        /// <inheritdoc />
        public Task<UserEntity?> GetAsync(Guid userId, CancellationToken cancellationToken = default) =>
            dbContext.Users.SingleOrDefaultAsync(user => user.Id == userId, cancellationToken);

        /// <inheritdoc />
        public Task<UserEntity?> GetInTenantAsync(
            Guid userId,
            Guid tenantId,
            Guid appId,
            CancellationToken cancellationToken = default,
            bool includeInactiveMembership = false) =>
            TenantUsers(tenantId, appId, includeInactiveMembership)
                .AsNoTracking()
                .SingleOrDefaultAsync(user => user.Id == userId, cancellationToken);

        /// <inheritdoc />
        public async Task<IReadOnlyList<UserEntity>> GetManyInTenantAsync(
            IReadOnlyCollection<Guid> userIds,
            Guid tenantId,
            Guid appId,
            CancellationToken cancellationToken = default,
            bool includeInactiveMembership = false)
        {
            ArgumentNullException.ThrowIfNull(userIds);
            if (userIds.Count == 0)
            {
                return [];
            }

            var ids = userIds.Distinct().ToArray();
            return await TenantUsers(tenantId, appId, includeInactiveMembership)
                .AsNoTracking()
                .Where(user => ids.Contains(user.Id))
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<(long TotalCount, IReadOnlyList<UserEntity> Items)> GetPagedInTenantAsync(
            Guid tenantId,
            Guid appId,
            int skipCount,
            int maxResultCount,
            string? filter,
            CMasterStatus? status,
            string sortBy,
            bool descending,
            CancellationToken cancellationToken = default)
        {
            var query = TenantUsers(tenantId, appId, false).AsNoTracking();
            if (!string.IsNullOrWhiteSpace(filter))
            {
                var pattern = $"%{filter.Trim()}%";
                query = query.Where(user =>
                    user.FirstName != null && EF.Functions.ILike(user.FirstName, pattern) ||
                    user.LastName != null && EF.Functions.ILike(user.LastName, pattern));
            }

            if (status is { } masterStatus)
            {
                query = query.Where(user => user.MasterStatus == masterStatus);
            }

            var count = await query.LongCountAsync(cancellationToken);
            IOrderedQueryable<UserEntity> ordered = sortBy switch
            {
                "firstName" => descending
                    ? query.OrderByDescending(user => user.FirstName).ThenBy(user => user.Id)
                    : query.OrderBy(user => user.FirstName).ThenBy(user => user.Id),
                "lastName" => descending
                    ? query.OrderByDescending(user => user.LastName).ThenBy(user => user.Id)
                    : query.OrderBy(user => user.LastName).ThenBy(user => user.Id),
                "createdAt" => descending
                    ? query.OrderByDescending(user => user.CreatedAt).ThenBy(user => user.Id)
                    : query.OrderBy(user => user.CreatedAt).ThenBy(user => user.Id),
                "modifiedAt" => descending
                    ? query.OrderByDescending(user => user.ModifiedAt).ThenBy(user => user.Id)
                    : query.OrderBy(user => user.ModifiedAt).ThenBy(user => user.Id),
                _ => descending
                    ? query.OrderByDescending(user => user.Id)
                    : query.OrderBy(user => user.Id)
            };

            var items = await ordered
                .Skip(skipCount)
                .Take(maxResultCount)
                .ToListAsync(cancellationToken);
            return (count, items);
        }

        /// <inheritdoc />
        public async Task AddAsync(UserEntity user, UserCredentialEntity credential, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(user);
            ArgumentNullException.ThrowIfNull(credential);
            await dbContext.Users.AddAsync(user, cancellationToken);
            await dbContext.UserCredentials.AddAsync(credential, cancellationToken);
        }

        /// <inheritdoc />
        public Task UpdateAsync(UserEntity user, UserCredentialEntity credential, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(user);
            ArgumentNullException.ThrowIfNull(credential);
            dbContext.Users.Update(user);
            dbContext.UserCredentials.Update(credential);
            return Task.CompletedTask;
        }

        private IQueryable<UserEntity> TenantUsers(
            Guid tenantId,
            Guid appId,
            bool includeInactiveMembership) =>
            dbContext.Users.Where(user => dbContext.Set<TenantMembershipEntity>().Any(membership =>
                membership.UserId == user.Id &&
                membership.TenantId == tenantId &&
                membership.AppId == appId &&
                (includeInactiveMembership || membership.Status == CMembershipStatus.Active)));
    }
}
