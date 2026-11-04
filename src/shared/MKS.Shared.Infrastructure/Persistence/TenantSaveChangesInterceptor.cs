using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MKS.Shared.Application.Contracts.Common.Interfaces;
using MKS.Shared.Domain.Apps.Entities;
using MKS.Shared.Domain.MultiTenancy.Entities;

namespace MKS.Shared.Infrastructure.Persistence
{
    /// <summary>
    /// Assigns the authenticated tenant to new tenant entities and rejects cross-tenant writes.
    /// </summary>
    public sealed class TenantSaveChangesInterceptor(
        ICurrentContext currentContext,
        MKS.Shared.Application.Contracts.Common.Interfaces.ITenantScopeAccessor tenantScope) : SaveChangesInterceptor
    {
        /// <inheritdoc />
        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            ValidateTenantWrites(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        /// <inheritdoc />
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            ValidateTenantWrites(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void ValidateTenantWrites(DbContext? context)
        {
            if (context is null)
            {
                return;
            }

            foreach (var entry in context.ChangeTracker.Entries<IHasTenantEntity>())
            {
                if (entry.State is EntityState.Detached or EntityState.Unchanged)
                {
                    continue;
                }

                var tenantId = tenantScope.TenantId ?? currentContext.TenantId;
                if (tenantId is null)
                {
                    throw new InvalidOperationException("A verified tenant context is required to write tenant-scoped data.");
                }

                if (entry.State == EntityState.Added)
                {
                    entry.Entity.TenantId = tenantId.Value;
                    continue;
                }

                var tenantProperty = entry.Property(entity => entity.TenantId);
                if (entry.Entity.TenantId != tenantId.Value ||
                    tenantProperty.OriginalValue != tenantProperty.CurrentValue)
                {
                    throw new InvalidOperationException("Cross-tenant writes and tenant reassignment are not allowed.");
                }
            }

            foreach (var entry in context.ChangeTracker.Entries<IHasAppEntity>())
            {
                if (entry.State is EntityState.Detached or EntityState.Unchanged ||
                    currentContext.AppId is not { } appId)
                {
                    continue;
                }

                var appProperty = entry.Property(entity => entity.AppId);
                if (entry.Entity.AppId != appId ||
                    entry.State != EntityState.Added &&
                    appProperty.OriginalValue != appProperty.CurrentValue)
                {
                    throw new InvalidOperationException("Cross-application writes are not allowed.");
                }
            }
        }
    }
}
