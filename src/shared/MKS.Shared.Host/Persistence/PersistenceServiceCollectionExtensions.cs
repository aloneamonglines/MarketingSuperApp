using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MKS.Shared.Application.Contracts.Common.Interfaces;
using MKS.Shared.Infrastructure.Persistence;

namespace MKS.Shared.Host.Persistence
{
    /// <summary>
    /// Registers shared persistence interceptors and applies them to EF Core options.
    /// </summary>
    public static class PersistenceServiceCollectionExtensions
    {
        /// <summary>Registers shared tenant, audit, and RLS interceptors.</summary>
        public static IServiceCollection AddSharedPersistence(
            this IServiceCollection services,
            Action<SharedPersistenceOptions>? configure = null)
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddOptions<SharedPersistenceOptions>();
            if (configure is not null)
            {
                services.Configure(configure);
            }

            services.TryAddSingleton(TimeProvider.System);
            services.TryAddScoped<AuditSaveChangesInterceptor>();
            services.TryAddScoped<TenantSaveChangesInterceptor>();
            services.TryAddScoped<TenantRlsTransactionInterceptor>();
            return services;
        }

        /// <summary>Applies shared interceptors to a context's EF Core options.</summary>
        public static DbContextOptionsBuilder AddSharedPersistence(
            this DbContextOptionsBuilder builder,
            IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(serviceProvider);
            ArgumentNullException.ThrowIfNull(serviceProvider.GetRequiredService<ICurrentContext>());

            builder.AddInterceptors(
                serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>(),
                serviceProvider.GetRequiredService<TenantSaveChangesInterceptor>(),
                serviceProvider.GetRequiredService<TenantRlsTransactionInterceptor>());
            return builder;
        }
    }
}
