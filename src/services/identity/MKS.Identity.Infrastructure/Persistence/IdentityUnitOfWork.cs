using MKS.Identity.Application.Contracts.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MKS.Identity.Infrastructure.Persistence
{
    /// <summary>
    /// Commits a single Identity workflow in one database transaction.
    /// </summary>
    public sealed class IdentityUnitOfWork(IdentityDbContext dbContext) : IIdentityUnitOfWork
    {
        /// <inheritdoc />
        public async Task ExecuteAsync(
            Func<CancellationToken, Task> operation,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(operation);
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            await operation(cancellationToken);
            await IdentityDbContextSaveChanges.SaveChangesWithLocalEventsAsync(dbContext, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
    }
}
