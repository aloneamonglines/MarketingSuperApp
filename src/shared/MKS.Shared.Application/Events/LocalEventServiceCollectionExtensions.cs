using Microsoft.Extensions.DependencyInjection;
using MKS.Shared.Application.Contracts.Common.Events;

namespace MKS.Shared.Application.Events
{
    /// <summary>Registers a typed local event handler for dispatch.</summary>
    public static class LocalEventServiceCollectionExtensions
    {
        public static IServiceCollection AddLocalEventHandler<THandler>(this IServiceCollection services)
            where THandler : class, ILocalEventHandler
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddScoped<ILocalEventHandler, THandler>();
            return services;
        }
    }
}
