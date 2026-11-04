using MKS.Shared.Domain.Shared.Common.Events;

namespace MKS.Identity.Application.Contracts.Common.Interfaces
{
    /// <summary>Provides a strongly typed base for transactional integration-event consumers.</summary>
    public abstract class IntegrationEventHandler<TEvent> : IIntegrationEventHandler
        where TEvent : class, IIntegrationEvent
    {
        private const string ExpectedEventTypeMessage = "Expected event type '{0}'.";

        /// <inheritdoc />
        public Type EventType => typeof(TEvent);

        /// <inheritdoc />
        public Task HandleAsync(
            IIntegrationEvent integrationEvent,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(integrationEvent);
            return integrationEvent is TEvent typedEvent
                ? HandleAsync(typedEvent, cancellationToken)
                : throw new ArgumentException(
                    string.Format(
                        System.Globalization.CultureInfo.InvariantCulture,
                        ExpectedEventTypeMessage,
                        typeof(TEvent).FullName),
                    nameof(integrationEvent));
        }

        /// <summary>Handles the typed integration event.</summary>
        protected abstract Task HandleAsync(TEvent integrationEvent, CancellationToken cancellationToken);
    }
}
