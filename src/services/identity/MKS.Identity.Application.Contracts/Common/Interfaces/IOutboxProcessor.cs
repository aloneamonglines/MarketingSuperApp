namespace MKS.Identity.Application.Contracts.Common.Interfaces
{
    /// <summary>Processes a bounded number of durable integration-event deliveries.</summary>
    public interface IOutboxProcessor
    {
        Task<int> ProcessBatchAsync(CancellationToken cancellationToken = default);
    }
}
