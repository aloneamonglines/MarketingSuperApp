using Microsoft.Extensions.DependencyInjection;
using MKS.Identity.Application.Contracts.Common.Interfaces;
using MKS.Identity.Application.Contracts.Common.Repositories;
using MKS.Identity.Domain.Shared.Constants;
using MKS.Shared.Domain.Shared.Common.Events;

namespace MKS.Identity.Application.Common
{
    /// <summary>Invokes registered event consumers with inbox-based idempotency.</summary>
    public sealed class IntegrationEventDispatcher(IServiceScopeFactory scopeFactory)
        : IIntegrationEventDispatcher
    {
        /// <inheritdoc />
        public async Task DispatchAsync(
            IIntegrationEvent integrationEvent,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(integrationEvent);
            await using var scope = scopeFactory.CreateAsyncScope();
            var services = scope.ServiceProvider;
            var handlers = services.GetServices<IIntegrationEventHandler>()
                .Where(handler => handler.EventType == integrationEvent.GetType())
                .ToArray();
            if (handlers.Length == 0)
            {
                throw new InvalidOperationException(string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    IdentityConstants.ValidationMessages.IntegrationEventConsumerNotRegistered,
                    integrationEvent.GetType().FullName));
            }

            var inboxRepository = services.GetRequiredService<IInboxRepository>();
            foreach (var handler in handlers)
            {
                var consumer = handler.GetType().FullName ?? handler.GetType().Name;
                await inboxRepository.ProcessAsync(
                    integrationEvent,
                    consumer,
                    token => handler.HandleAsync(integrationEvent, token),
                    cancellationToken);
            }
        }
    }
}
