using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MKS.Identity.Domain.MultiTenancy.Entities;
using MKS.Identity.Domain.MultiTenancy.Repositories;
using MKS.Identity.Infrastructure.Persistence;

namespace MKS.Identity.Infrastructure.MultiTenancy
{
    /// <summary>
    /// Serializes administrator assignment changes per tenant.
    /// </summary>
    public sealed class TenantAdminAssignmentRepository(IdentityDbContext dbContext) : ITenantAdminAssignmentRepository
    {
        /// <inheritdoc />
        public async Task AddAsync(TenantAdminAssignmentEntity assignment, CancellationToken cancellationToken = default) =>
            await dbContext.TenantAdminAssignments.AddAsync(assignment, cancellationToken);

        /// <inheritdoc />
        public Task<TenantAdminAssignmentEntity?> FindAsync(
            Guid tenantId,
            Guid userId,
            CancellationToken cancellationToken = default) =>
            dbContext.TenantAdminAssignments.SingleOrDefaultAsync(
                assignment => assignment.TenantId == tenantId && assignment.UserId == userId,
                cancellationToken);

        /// <inheritdoc />
        public Task UpdateAsync(TenantAdminAssignmentEntity assignment, CancellationToken cancellationToken = default)
        {
            dbContext.TenantAdminAssignments.Update(assignment);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task ExecuteForTenantAsync(
            Guid tenantId,
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
                parameter.Value = $"tenant-admin:{tenantId:N}";
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
        public Task<int> CountActiveAsync(Guid tenantId, CancellationToken cancellationToken = default) =>
            dbContext.TenantAdminAssignments.CountAsync(
                assignment => assignment.TenantId == tenantId && assignment.IsActive,
                cancellationToken);
    }
}
