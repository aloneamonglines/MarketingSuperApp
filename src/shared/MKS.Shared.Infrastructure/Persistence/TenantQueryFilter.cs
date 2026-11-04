using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MKS.Shared.Domain.MultiTenancy.Entities;

namespace MKS.Shared.Infrastructure.Persistence
{
    /// <summary>
    /// Adds a tenant query filter to every tenant-scoped entity in a model.
    /// </summary>
    public static class TenantQueryFilter
    {
        /// <summary>Applies tenant filters using the supplied context's active tenant identifier.</summary>
        public static void Apply(ModelBuilder modelBuilder, DbContext context)
        {
            ArgumentNullException.ThrowIfNull(modelBuilder);
            ArgumentNullException.ThrowIfNull(context);

            foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                         .Where(type => typeof(IHasTenantEntity).IsAssignableFrom(type.ClrType)))
            {
                var entityParameter = Expression.Parameter(entityType.ClrType, "entity");
                var tenantProperty = Expression.Property(
                    Expression.Convert(entityParameter, typeof(IHasTenantEntity)),
                    nameof(IHasTenantEntity.TenantId));
                var contextExpression = Expression.Constant(context);
                var tenantIdProperty = Expression.Property(
                    Expression.Convert(contextExpression, typeof(ITenantFilterContext)),
                    nameof(ITenantFilterContext.CurrentTenantId));
                var predicate = Expression.Lambda(
                    Expression.Equal(tenantProperty, tenantIdProperty),
                    entityParameter);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(predicate);
            }
        }
    }

    /// <summary>Provides the tenant id used by shared tenant query filters.</summary>
    public interface ITenantFilterContext
    {
        /// <summary>Gets the currently authenticated tenant identifier.</summary>
        Guid CurrentTenantId { get; }
    }
}
