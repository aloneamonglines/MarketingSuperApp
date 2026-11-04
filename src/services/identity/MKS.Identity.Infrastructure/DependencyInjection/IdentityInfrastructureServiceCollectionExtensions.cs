using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore;
using MKS.Identity.Domain.Shared.Constants;
using MKS.Identity.Domain.Apps.Repositories;
using MKS.Identity.Domain.Authorization.Repositories;
using MKS.Identity.Application.Contracts.Authorization.Interfaces;
using MKS.Identity.Domain.MultiTenancy.Repositories;
using MKS.Identity.Domain.Platform.Repositories;
using MKS.Identity.Domain.Users.Repositories;
using MKS.Identity.Application.Contracts.Common.Interfaces;
using MKS.Identity.Infrastructure.Apps;
using MKS.Identity.Infrastructure.Auditing;
using MKS.Identity.Infrastructure.Authorization;
using MKS.Identity.Infrastructure.MultiTenancy;
using MKS.Identity.Infrastructure.Persistence;
using MKS.Identity.Infrastructure.Messaging;
using MKS.Identity.Application.Contracts.Common.Options;
using MKS.Identity.Application.Contracts.Common.Repositories;
using MKS.Identity.Infrastructure.Platform;
using MKS.Identity.Infrastructure.Users;
using MKS.Identity.Domain.Apps.Entities;
using MKS.Identity.Domain.MultiTenancy.Entities;
using MKS.Shared.Application.Contracts.Common.Interfaces;
using MKS.Shared.Domain.Apps.Repositories;
using MKS.Shared.Domain.MultiTenancy.Repositories;
using MKS.Shared.Infrastructure.Persistence;
using MKS.Shared.Application.Contracts.Common.Events;

namespace MKS.Identity.Infrastructure.DependencyInjection
{
    /// <summary>
    /// Registers PostgreSQL-backed Identity infrastructure services.
    /// </summary>
    public static class IdentityInfrastructureServiceCollectionExtensions
    {
        /// <summary>Registers the Identity database context and repository implementations.</summary>
        public static IServiceCollection AddIdentityInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);
            services.AddOptions<IdentityConnectionStringsOptions>()
                .Bind(configuration.GetSection(IdentityConnectionStringsOptions.SectionName))
                .Validate(
                    options => !string.IsNullOrWhiteSpace(options.Identity),
                    IdentityConstants.ConfigurationErrors.IdentityConnectionStringRequired)
                .ValidateOnStart();
            services.AddOptions<SharedPersistenceOptions>();
            services.AddOptions<OutboxProcessingOptions>()
                .Bind(configuration.GetSection(OutboxProcessingOptions.SectionName))
                .Validate(
                    OutboxProcessingOptionsValidator.IsValid,
                    IdentityConstants.ValidationMessages.OutboxOptionsInvalid)
                .ValidateOnStart();
            services.TryAddSingleton(TimeProvider.System);
            services.TryAddScoped<ITenantScopeAccessor, TenantScopeAccessor>();
            services.TryAddScoped<AuditSaveChangesInterceptor>();
            services.TryAddScoped<TenantSaveChangesInterceptor>();
            services.TryAddScoped<TenantRlsTransactionInterceptor>();
            services.TryAddScoped<EntityLifecycleSaveChangesInterceptor>();
            services.AddDbContext<IdentityDbContext>((serviceProvider, options) =>
            {
                var connectionString = serviceProvider
                    .GetRequiredService<Microsoft.Extensions.Options.IOptions<IdentityConnectionStringsOptions>>()
                    .Value.Identity;
                options.UseNpgsql(connectionString);
                options.AddInterceptors(
                    serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>(),
                    serviceProvider.GetRequiredService<TenantSaveChangesInterceptor>(),
                    serviceProvider.GetRequiredService<TenantRlsTransactionInterceptor>(),
                    serviceProvider.GetRequiredService<EntityLifecycleSaveChangesInterceptor>());
            });

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserSessionRepository, UserSessionRepository>();
            services.AddScoped<IAppRepository<AppEntity>, AppRepository>();
            services.AddScoped<ITenantRepository<TenantEntity>, TenantRepository>();
            services.AddScoped<ITenantMembershipRepository, TenantMembershipRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IAuthorizationRepository, AuthorizationRepository>();
            services.AddScoped<IAppPermissionCatalogSynchronizer, AppPermissionCatalogSynchronizer>();
            services.AddScoped<IBreakGlassGrantRepository, BreakGlassGrantRepository>();
            services.AddScoped<ISecurityAuditLogWriter, SecurityAuditLogWriter>();
            services.AddScoped<IAppAdminAssignmentRepository, AppAdminAssignmentRepository>();
            services.AddScoped<ITenantAdminAssignmentRepository, TenantAdminAssignmentRepository>();
            services.AddScoped<IIdentityUnitOfWork, IdentityUnitOfWork>();
            services.AddScoped<IOutboxRepository, OutboxRepository>();
            services.AddScoped<IInboxRepository, InboxRepository>();
            services.AddScoped(
                typeof(IIdentityEntityRepository<>),
                typeof(IdentityEntityRepository<>));
            services.AddScoped<IOutboxLeaseRenewer, OutboxLeaseRenewer>();
            services.AddSingleton<IPasswordService, Pbkdf2PasswordService>();
            return services;
        }
    }
}
