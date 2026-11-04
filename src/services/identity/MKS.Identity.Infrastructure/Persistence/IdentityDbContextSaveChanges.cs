using Microsoft.EntityFrameworkCore;

namespace MKS.Identity.Infrastructure.Persistence
{
    /// <summary>Persists work created by local event handlers in the current transaction.</summary>
    internal static class IdentityDbContextSaveChanges
    {
        private const int MaxFlushRounds = 8;

        public static async Task SaveChangesWithLocalEventsAsync(
            IdentityDbContext dbContext,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(dbContext);
            for (var round = 0; round < MaxFlushRounds; round++)
            {
                await dbContext.SaveChangesAsync(cancellationToken);
                if (!dbContext.ChangeTracker.HasChanges())
                {
                    return;
                }
            }

            throw new InvalidOperationException(
                $"Local event handlers produced changes in more than {MaxFlushRounds} consecutive save rounds.");
        }
    }
}
