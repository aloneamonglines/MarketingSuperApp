using System.Text.Json;
using MKS.Identity.Application.Contracts.Common.Interfaces;
using MKS.Identity.Application.Contracts.Common.Repositories;
using MKS.Identity.Domain.Shared.Constants;
using MKS.Shared.Domain.Shared.Common.Events;

namespace MKS.Identity.Application.Common
{
    /// <summary>Publishes integration events through the transaction-bound outbox repository.</summary>
    public sealed class IntegrationEventPublisher(
        IOutboxRepository outboxRepository,
        TimeProvider timeProvider) : IIntegrationEventPublisher
    {
        /// <inheritdoc />
        public Task PublishAsync(
            IIntegrationEvent integrationEvent,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(integrationEvent);
            if (integrationEvent.EventId == Guid.Empty)
            {
                throw new ArgumentException(
                    IdentityConstants.ValidationMessages.IntegrationEventIdRequired,
                    nameof(integrationEvent));
            }

            var scope = IntegrationEventScope.Resolve(integrationEvent);
            var eventType = integrationEvent.GetType();
            return outboxRepository.AddAsync(
                integrationEvent.EventId,
                scope,
                integrationEvent.AppId,
                integrationEvent.TenantId,
                eventType.FullName ?? eventType.Name,
                JsonSerializer.Serialize(integrationEvent, eventType),
                integrationEvent.OccurredAt,
                timeProvider.GetUtcNow(),
                cancellationToken);
        }
    }
}
