using Microsoft.Extensions.DependencyInjection;
using MKS.Identity.Application.Contracts.Common.Interfaces;
using MKS.Shared.Domain.Shared.Common.Events;

namespace MKS.Identity.Infrastructure.Messaging
{
    /// <summary>Registers integration-event consumers for durable delivery.</summary>
    public static class IntegrationEventServiceCollectionExtensions
    {
        public static IServiceCollection AddIntegrationEventHandler<TEvent, THandler>(this IServiceCollection services)
            where TEvent : class, IIntegrationEvent
            where THandler : IntegrationEventHandler<TEvent>
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddScoped<IIntegrationEventHandler, THandler>();
            return services;
        }
    }
}
