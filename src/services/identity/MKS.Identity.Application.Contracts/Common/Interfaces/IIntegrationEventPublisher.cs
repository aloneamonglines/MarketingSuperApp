using MKS.Shared.Domain.Shared.Common.Events;

namespace MKS.Identity.Application.Contracts.Common.Interfaces
{
    /// <summary>
    /// Publishes an integration event through the transactional outbox.
    /// </summary>
    public interface IIntegrationEventPublisher
    {
        /// <summary>Adds an event to the current transaction for asynchronous delivery.</summary>
        Task PublishAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
    }
}
