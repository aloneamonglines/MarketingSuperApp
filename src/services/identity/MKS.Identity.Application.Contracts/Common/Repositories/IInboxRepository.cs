using MKS.Shared.Domain.Shared.Common.Events;

namespace MKS.Identity.Application.Contracts.Common.Repositories
{
    /// <summary>Executes integration-event consumers idempotently inside a persistence transaction.</summary>
    public interface IInboxRepository
    {
        Task<bool> ProcessAsync(
            IIntegrationEvent integrationEvent,
            string consumer,
            Func<CancellationToken, Task> handler,
            CancellationToken cancellationToken = default);
    }
}
