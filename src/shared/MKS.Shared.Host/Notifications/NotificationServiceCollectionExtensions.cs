using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MKS.Shared.Application.Contracts.Notifications;
using MKS.Shared.Infrastructure.Notifications;

namespace MKS.Shared.Host.Notifications
{
    /// <summary>
    /// Registers the default notification sender.
    /// </summary>
    public static class NotificationServiceCollectionExtensions
    {
        /// <summary>Registers a logging-only sender that does not deliver messages.</summary>
        public static IServiceCollection AddSharedNotifications(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            services.TryAddSingleton<INotificationSender, NullNotificationSender>();
            return services;
        }
    }
}
