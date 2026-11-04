using System.Linq.Expressions;
using MKS.Shared.Domain.Common;
using MKS.Shared.Domain.MultiTenancy.Entities;
using MKS.Shared.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace MKS.Identity.Infrastructure.Persistence.Configurations
{
    internal static class IdentityGlobalQueryFilters
    {
        public static void ApplyIdentityGlobalQueryFilters(this ModelBuilder modelBuilder, ITenantFilterContext context)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                var entityTypeParameter = Expression.Parameter(entityType.ClrType, "entity");
                Expression? filter = null;
                if (typeof(FullAuditEntity).IsAssignableFrom(entityType.ClrType))
                {
                    filter = Expression.Equal(
                        Expression.Property(entityTypeParameter, nameof(FullAuditEntity.IsDeleted)),
                        Expression.Constant(false));
                }

                if (typeof(IHasTenantEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var tenantProperty = Expression.Property(
                        Expression.Convert(entityTypeParameter, typeof(IHasTenantEntity)),
                        nameof(IHasTenantEntity.TenantId));
                    var tenantContext = Expression.Property(
                        Expression.Constant(context),
                        nameof(ITenantFilterContext.CurrentTenantId));
                    var tenantFilter = Expression.Equal(tenantProperty, tenantContext);
                    filter = filter is null ? tenantFilter : Expression.AndAlso(filter, tenantFilter);
                }

                if (filter is not null)
                {
                    modelBuilder.Entity(entityType.ClrType).HasQueryFilter(
                        Expression.Lambda(filter, entityTypeParameter));
                }
            }
        }
    }
}
