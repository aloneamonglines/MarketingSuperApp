namespace MKS.Identity.Application.Contracts.Common.Interfaces
{
    /// <summary>Provides operational actions for failed integration events.</summary>
    public interface IIntegrationEventDeadLetterService
    {
        /// <summary>Requeues a dead-lettered event for a fresh delivery attempt.</summary>
        Task<bool> ReplayAsync(Guid eventId, CancellationToken cancellationToken = default);
    }
}
