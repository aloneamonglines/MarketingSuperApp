using MKS.Shared.Domain.Shared.Common.Events;

namespace MKS.Identity.Application.Contracts.Common.Interfaces
{
    /// <summary>Handles an integration event delivered by the durable message queue.</summary>
    public interface IIntegrationEventHandler
    {
        /// <summary>Gets the event type handled by this consumer.</summary>
        Type EventType { get; }

        /// <summary>Processes the event inside the consumer's inbox transaction.</summary>
        Task HandleAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
    }
}
