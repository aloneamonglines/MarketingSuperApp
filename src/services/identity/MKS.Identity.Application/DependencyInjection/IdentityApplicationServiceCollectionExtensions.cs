using Microsoft.Extensions.DependencyInjection;
using MKS.Identity.Application.Authorization;
using MKS.Identity.Application.Authorization.Events.Handlers;
using MKS.Identity.Application.Authorization.Events.Processors;
using MKS.Identity.Application.Apps;
using MKS.Identity.Application.MultiTenancy;
using MKS.Identity.Application.Platform;
using MKS.Identity.Application.Users;
using MKS.Identity.Application.Common;
using MKS.Identity.Application.Contracts.Common.Interfaces;
using MKS.Identity.Application.Contracts.Common.Options;
using MKS.Identity.Application.Contracts.Users.Interfaces;
using MKS.Identity.Application.Contracts.MultiTenancy.Interfaces;
using MKS.Identity.Application.Contracts.Authorization.Interfaces;
using MKS.Identity.Application.Contracts.Platform.Interfaces;
using MKS.Identity.Domain.Platform.Repositories;
using MKS.Identity.Domain.Authorization.Services;
using MKS.Identity.Domain.Apps.Services;
using MKS.Identity.Domain.MultiTenancy.Services;
using MKS.Identity.Domain.Platform.Services;
using MKS.Shared.Application.Contracts.Apps.Interfaces;
using MKS.Shared.Application.Contracts.Common.Events;
using MKS.Shared.Application.Events;
using MKS.Shared.Domain.Shared.Common.Events;

namespace MKS.Identity.Application.DependencyInjection
{
    /// <summary>
    /// Registers Identity application and domain services.
    /// </summary>
    public static class IdentityApplicationServiceCollectionExtensions
    {
        /// <summary>Registers application workflows and their authorization services.</summary>
        public static IServiceCollection AddIdentityApplication(
            this IServiceCollection services,
            Action<IdentityWorkflowOptions> configure)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configure);
            services.Configure(configure);
            services.AddScoped<ILocalEventDispatcher, LocalEventDispatcher>();
            services.AddLocalEventHandler<EntityCreatedEventHandler>();
            services.AddLocalEventHandler<EntityUpdatedEventHandler>();
            services.AddLocalEventHandler<EntityDeletedEventHandler>();
            services.AddScoped<ITenantAdminAssignmentEventProcessor, TenantAdminAssignmentEventProcessor>();
            services.AddLocalEventHandler<TenantAdminAssignmentCreatedEventHandler>();
            services.AddLocalEventHandler<TenantAdminAssignmentUpdatedEventHandler>();
            services.AddScoped<PermissionEvaluator>();
            services.AddScoped<IIntegrationEventPublisher, IntegrationEventPublisher>();
            services.AddScoped<IIntegrationEventDispatcher, IntegrationEventDispatcher>();
            services.AddScoped<IOutboxProcessor, OutboxProcessor>();
            services.AddScoped<IIntegrationEventDeadLetterService, IntegrationEventDeadLetterApplicationService>();
            services.AddScoped<AppAdminInvariantService>();
            services.AddScoped<TenantAdminInvariantService>();
            services.AddScoped(serviceProvider => new BreakGlassService(
                serviceProvider.GetRequiredService<IBreakGlassGrantRepository>(),
                serviceProvider.GetRequiredService<TimeProvider>(),
                serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<IdentityWorkflowOptions>>()
                    .Value.BreakGlassMaximumDuration));
            services.AddScoped<IUserApplicationService, UserApplicationService>();
            services.AddScoped<IAuthenticationFlowService, AuthenticationFlowService>();
            services.AddScoped<ITenantApplicationService, TenantApplicationService>();
            services.AddScoped<IRoleApplicationService, RoleApplicationService>();
            services.AddScoped<IAdminAssignmentApplicationService, AdminAssignmentApplicationService>();
            services.AddScoped<IBreakGlassApplicationService, BreakGlassApplicationService>();
            services.AddScoped<IAppService, AppApplicationService>();
            return services;
        }
    }
}
