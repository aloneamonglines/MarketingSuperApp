using MKS.Identity.Domain.Users.Entities;
using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Identity.Domain.Users.Repositories
{
    /// <summary>
    /// Defines persistence operations for global identities and sign-in identifiers.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>Finds a user by any normalized login identifier.</summary>
        Task<(UserEntity UserEntity, UserIdentifierEntity Identifier, UserCredentialEntity Credential)?> FindForSignInAsync(
            string normalizedIdentifier,
            CancellationToken cancellationToken = default);
        /// <summary>Finds an existing user owning any normalized identifier in the set.</summary>
        Task<UserEntity?> FindByAnyIdentifierAsync(
            IReadOnlyCollection<string> normalizedIdentifiers,
            CancellationToken cancellationToken = default);
        /// <summary>Gets a user's credential record.</summary>
        Task<UserCredentialEntity?> GetCredentialAsync(Guid userId, CancellationToken cancellationToken = default);
        /// <summary>Gets a user's profile.</summary>
        Task<UserEntity?> GetAsync(Guid userId, CancellationToken cancellationToken = default);
        /// <summary>Gets a profile only when the user has an active membership in the supplied tenant and app.</summary>
        Task<UserEntity?> GetInTenantAsync(
            Guid userId,
            Guid tenantId,
            Guid appId,
            CancellationToken cancellationToken = default,
            bool includeInactiveMembership = false);
        /// <summary>Gets only active tenant members from the supplied IDs.</summary>
        Task<IReadOnlyList<UserEntity>> GetManyInTenantAsync(
            IReadOnlyCollection<Guid> userIds,
            Guid tenantId,
            Guid appId,
            CancellationToken cancellationToken = default,
            bool includeInactiveMembership = false);
        /// <summary>Gets a filtered page of active members without exposing global user rows outside tenant membership.</summary>
        Task<(long TotalCount, IReadOnlyList<UserEntity> Items)> GetPagedInTenantAsync(
            Guid tenantId,
            Guid appId,
            int skipCount,
            int maxResultCount,
            string? filter,
            CMasterStatus? status,
            string sortBy,
            bool descending,
            CancellationToken cancellationToken = default);
        /// <summary>Adds a newly created global identity.</summary>
        Task AddAsync(UserEntity user, UserCredentialEntity credential, CancellationToken cancellationToken = default);
        /// <summary>Persists changes to an existing identity.</summary>
        Task UpdateAsync(UserEntity user, UserCredentialEntity credential, CancellationToken cancellationToken = default);
    }
}
