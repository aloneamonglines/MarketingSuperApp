namespace MKS.Identity.Application.Contracts.Common.Interfaces
{
    /// <summary>Keeps the active outbox lease valid while an event consumer is running.</summary>
    public interface IOutboxLeaseRenewer
    {
        Task RenewUntilCancelledAsync(
            Guid messageId,
            Guid lockId,
            CancellationTokenSource deliveryCancellation);
    }
}
