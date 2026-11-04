using MKS.Identity.Domain.Users.Entities;

namespace MKS.Identity.Domain.Users.Repositories
{
    /// <summary>
    /// Defines persistence operations for revocable sessions.
    /// </summary>
    public interface IUserSessionRepository
    {
        /// <summary>Adds a newly authenticated session.</summary>
        Task AddAsync(UserSessionEntity session, CancellationToken cancellationToken = default);
        /// <summary>Finds an active session owned by the user.</summary>
        Task<UserSessionEntity?> GetActiveAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default);
        /// <summary>Revokes all active sessions owned by the user.</summary>
        Task RevokeAllForUserAsync(Guid userId, DateTimeOffset revokedAt, CancellationToken cancellationToken = default);
        /// <summary>Atomically advances a session's refresh-token generation if the supplied generation is current.</summary>
        Task<bool> TryAdvanceRefreshGenerationAsync(
            Guid userId,
            Guid sessionId,
            long expectedGeneration,
            CancellationToken cancellationToken = default);
        /// <summary>Persists a session update.</summary>
        Task UpdateAsync(UserSessionEntity session, CancellationToken cancellationToken = default);
    }
}
