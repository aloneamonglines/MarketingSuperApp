using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MKS.Shared.Application.Contracts.Common.Interfaces;
using MKS.Shared.Domain.Common;

namespace MKS.Shared.Infrastructure.Persistence
{
    /// <summary>
    /// Applies audit metadata and soft-delete behavior before entities are persisted.
    /// </summary>
    public sealed class AuditSaveChangesInterceptor(ICurrentContext currentContext, TimeProvider timeProvider)
        : SaveChangesInterceptor
    {
        /// <inheritdoc />
        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            ApplyAudit(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        /// <inheritdoc />
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            ApplyAudit(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void ApplyAudit(DbContext? context)
        {
            if (context is null)
            {
                return;
            }

            var now = timeProvider.GetUtcNow();
            foreach (var entry in context.ChangeTracker.Entries<FullAuditEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = now;
                        entry.Entity.CreatorId ??= currentContext.UserId;
                        entry.Entity.IsDeleted = false;
                        break;
                    case EntityState.Modified:
                        entry.Entity.ModifiedAt = now;
                        entry.Entity.ModifierId = currentContext.UserId;
                        entry.Property(entity => entity.CreatedAt).IsModified = false;
                        entry.Property(entity => entity.CreatorId).IsModified = false;
                        break;
                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;
                        entry.Entity.IsDeleted = true;
                        entry.Entity.DeletedAt = now;
                        entry.Entity.DeleterId = currentContext.UserId;
                        entry.Entity.ModifiedAt = now;
                        entry.Entity.ModifierId = currentContext.UserId;
                        break;
                }
            }
        }
    }
}
