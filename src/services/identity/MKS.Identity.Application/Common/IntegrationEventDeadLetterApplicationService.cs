using MKS.Identity.Application.Contracts.Common.Interfaces;
using MKS.Identity.Application.Contracts.Common.Repositories;

namespace MKS.Identity.Application.Common
{
    /// <summary>Provides application-level replay operations for dead-lettered events.</summary>
    public sealed class IntegrationEventDeadLetterApplicationService(
        IOutboxRepository outboxRepository,
        TimeProvider timeProvider) : IIntegrationEventDeadLetterService
    {
        /// <inheritdoc />
        public Task<bool> ReplayAsync(Guid eventId, CancellationToken cancellationToken = default) =>
            outboxRepository.ReplayDeadLetterAsync(
                eventId,
                timeProvider.GetUtcNow(),
                cancellationToken);
    }
}
