using MKS.Shared.Domain.Shared.Common.Events;

namespace MKS.Identity.Application.Contracts.Common.Interfaces
{
    /// <summary>Dispatches an integration event to all registered consumers.</summary>
    public interface IIntegrationEventDispatcher
    {
        Task DispatchAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
    }
}
