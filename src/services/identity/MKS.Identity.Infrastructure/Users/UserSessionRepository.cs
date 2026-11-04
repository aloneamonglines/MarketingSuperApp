using Microsoft.EntityFrameworkCore;
using MKS.Identity.Domain.Users.Entities;
using MKS.Identity.Domain.Users.Repositories;
using MKS.Identity.Domain.Shared.Enums;
using MKS.Identity.Infrastructure.Persistence;
using MKS.Shared.Application.Contracts.Common.Events;
using MKS.Shared.Domain.Shared.Common.Events;

namespace MKS.Identity.Infrastructure.Users
{
    /// <summary>
    /// Persists revocable sign-in sessions.
    /// </summary>
    public sealed class UserSessionRepository(
        IdentityDbContext dbContext,
        ILocalEventDispatcher localEventDispatcher,
        TimeProvider timeProvider) : IUserSessionRepository
    {
        /// <inheritdoc />
        public async Task AddAsync(UserSessionEntity session, CancellationToken cancellationToken = default) =>
            await dbContext.UserSessions.AddAsync(session, cancellationToken);

        /// <inheritdoc />
        public Task<UserSessionEntity?> GetActiveAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken = default) =>
            dbContext.UserSessions.SingleOrDefaultAsync(
                session => session.Id == sessionId &&
                           session.UserId == userId &&
                           session.RevokedAt == null &&
                           dbContext.Users.Any(user =>
                               user.Id == session.UserId &&
                               user.MasterStatus == MKS.Shared.Domain.Shared.Enums.CMasterStatus.Active),
                cancellationToken);

        /// <inheritdoc />
        public async Task RevokeAllForUserAsync(
            Guid userId,
            DateTimeOffset revokedAt,
            CancellationToken cancellationToken = default)
        {
            var sessions = await dbContext.UserSessions
                .Where(session => session.UserId == userId && session.RevokedAt == null)
                .ToListAsync(cancellationToken);
            foreach (var session in sessions)
            {
                session.Revoke(revokedAt);
            }
        }

        /// <inheritdoc />
        public async Task<bool> TryAdvanceRefreshGenerationAsync(
            Guid userId,
            Guid sessionId,
            long expectedGeneration,
            CancellationToken cancellationToken = default)
        {
            if (expectedGeneration < 0 || expectedGeneration == long.MaxValue)
            {
                return false;
            }

            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            var affected = await dbContext.UserSessions
                .Where(session =>
                    session.Id == sessionId &&
                    session.UserId == userId &&
                    session.RevokedAt == null &&
                    session.RefreshTokenGeneration == expectedGeneration &&
                    dbContext.Users.Any(user =>
                        user.Id == session.UserId &&
                        user.MasterStatus == MKS.Shared.Domain.Shared.Enums.CMasterStatus.Active))
                .ExecuteUpdateAsync(
                    updates => updates.SetProperty(
                        session => session.RefreshTokenGeneration,
                        expectedGeneration + 1),
                    cancellationToken);
            if (affected != 1)
            {
                await transaction.CommitAsync(cancellationToken);
                return false;
            }

            var updatedSession = await dbContext.UserSessions
                .AsNoTracking()
                .SingleAsync(session => session.Id == sessionId, cancellationToken);
            await localEventDispatcher.DispatchAsync(
                new EntityUpdatedEvent(updatedSession, updatedSession.Id, timeProvider.GetUtcNow()),
                cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return true;
        }

        /// <inheritdoc />
        public Task UpdateAsync(UserSessionEntity session, CancellationToken cancellationToken = default)
        {
            dbContext.UserSessions.Update(session);
            return Task.CompletedTask;
        }
    }
}
