using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MKS.Identity.Domain.Apps.Entities;
using MKS.Identity.Domain.Apps.Repositories;
using MKS.Identity.Infrastructure.Persistence;

namespace MKS.Identity.Infrastructure.Apps
{
    /// <summary>
    /// Serializes administrator assignment changes per application.
    /// </summary>
    public sealed class AppAdminAssignmentRepository(IdentityDbContext dbContext) : IAppAdminAssignmentRepository
    {
        /// <inheritdoc />
        public async Task AddAsync(AppAdminAssignmentEntity assignment, CancellationToken cancellationToken = default) =>
            await dbContext.AppAdminAssignments.AddAsync(assignment, cancellationToken);

        /// <inheritdoc />
        public Task<AppAdminAssignmentEntity?> FindAsync(
            Guid appId,
            Guid userId,
            CancellationToken cancellationToken = default) =>
            dbContext.AppAdminAssignments.SingleOrDefaultAsync(
                assignment => assignment.AppId == appId && assignment.UserId == userId,
                cancellationToken);

        /// <inheritdoc />
        public Task UpdateAsync(AppAdminAssignmentEntity assignment, CancellationToken cancellationToken = default)
        {
            dbContext.AppAdminAssignments.Update(assignment);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task ExecuteForAppAsync(
            Guid appId,
            Func<CancellationToken, Task> action,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(action);
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            await using (var command = dbContext.Database.GetDbConnection().CreateCommand())
            {
                command.Transaction = transaction.GetDbTransaction();
                command.CommandText = "SELECT pg_advisory_xact_lock(hashtextextended(@lock_key, 0))";
                var parameter = command.CreateParameter();
                parameter.ParameterName = "lock_key";
                parameter.Value = $"app-admin:{appId:N}";
                command.Parameters.Add(parameter);
                await command.ExecuteNonQueryAsync(cancellationToken);
            }

            await action(cancellationToken);
            await Persistence.IdentityDbContextSaveChanges.SaveChangesWithLocalEventsAsync(
                dbContext,
                cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }

        /// <inheritdoc />
        public Task<int> CountActiveAsync(Guid appId, CancellationToken cancellationToken = default) =>
            dbContext.AppAdminAssignments.CountAsync(
                assignment => assignment.AppId == appId && assignment.IsActive,
                cancellationToken);
    }
}
