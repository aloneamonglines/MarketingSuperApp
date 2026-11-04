using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MKS.Identity.Application.Contracts.Common.DTOs;
using MKS.Identity.Application.Contracts.Common.Interfaces;
using MKS.Identity.Application.Contracts.Common.Options;
using MKS.Identity.Application.Contracts.Common.Repositories;
using MKS.Identity.Domain.Shared.Constants;
using MKS.Shared.Domain.Shared.Common.Events;

namespace MKS.Identity.Application.Common
{
    /// <summary>Orchestrates delivery, retries, and dead-lettering through application contracts.</summary>
    public sealed class OutboxProcessor(
        IOutboxRepository outboxRepository,
        IIntegrationEventDispatcher dispatcher,
        IOutboxLeaseRenewer leaseRenewer,
        IEnumerable<IIntegrationEventHandler> handlers,
        IOptions<OutboxProcessingOptions> options,
        TimeProvider timeProvider,
        ILogger<OutboxProcessor> logger) : IOutboxProcessor
    {
        private readonly OutboxProcessingOptions _options = options.Value;
        private readonly Type[] _eventTypes = handlers.Select(handler => handler.EventType).Distinct().ToArray();

        /// <inheritdoc />
        public async Task<int> ProcessBatchAsync(CancellationToken cancellationToken = default)
        {
            var processedCount = 0;
            while (processedCount < _options.BatchSize)
            {
                var lease = await outboxRepository.ClaimNextAsync(
                    timeProvider.GetUtcNow(),
                    _options.LeaseDuration,
                    cancellationToken: cancellationToken);
                if (lease is null)
                {
                    break;
                }

                processedCount++;
                using var deliveryCancellation =
                    CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                var leaseRenewal = leaseRenewer.RenewUntilCancelledAsync(
                    lease.Id,
                    lease.LockId,
                    deliveryCancellation);
                try
                {
                    var integrationEvent = Deserialize(lease);
                    await dispatcher.DispatchAsync(integrationEvent, deliveryCancellation.Token);
                    await outboxRepository.MarkProcessedAsync(
                        lease,
                        timeProvider.GetUtcNow(),
                        cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    var error = TruncateError(exception.ToString());
                    var deadLettered = await outboxRepository.RecordFailureAsync(
                        lease,
                        error,
                        timeProvider.GetUtcNow(),
                        _options.MaxAttempts,
                        OutboxRetryPolicy.GetDelay(lease.AttemptCount, _options),
                        cancellationToken);
                    logger.LogWarning(
                        exception,
                        "Outbox event {EventId} failed delivery on attempt {AttemptCount}. Dead-lettered: {DeadLettered}.",
                        lease.EventId,
                        lease.AttemptCount,
                        deadLettered);
                }
                finally
                {
                    deliveryCancellation.Cancel();
                    try
                    {
                        await leaseRenewal;
                    }
                    catch (OperationCanceledException) when (deliveryCancellation.IsCancellationRequested)
                    {
                    }
                }
            }

            return processedCount;
        }

        private IIntegrationEvent Deserialize(OutboxMessageLease lease)
        {
            var eventType = _eventTypes.SingleOrDefault(type =>
                string.Equals(type.FullName, lease.Type, StringComparison.Ordinal));
            if (eventType is null)
            {
                throw new InvalidOperationException(string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    IdentityConstants.ValidationMessages.IntegrationEventConsumerNotRegistered,
                    lease.Type));
            }

            var integrationEvent = JsonSerializer.Deserialize(lease.Payload, eventType) as IIntegrationEvent
                ?? throw new InvalidOperationException(string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    IdentityConstants.ValidationMessages.IntegrationEventPayloadInvalid,
                    lease.Type));
            if (integrationEvent.EventId != lease.EventId)
            {
                throw new InvalidOperationException(string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    IdentityConstants.ValidationMessages.IntegrationEventIdMismatch,
                    lease.EventId));
            }

            var scope = IntegrationEventScope.Resolve(integrationEvent);
            if (scope != lease.Scope ||
                integrationEvent.AppId != lease.AppId ||
                integrationEvent.TenantId != lease.TenantId)
            {
                throw new InvalidOperationException(string.Format(
                    System.Globalization.CultureInfo.InvariantCulture,
                    IdentityConstants.ValidationMessages.IntegrationEventScopeMismatch,
                    lease.EventId));
            }

            return integrationEvent;
        }

        private string TruncateError(string error) =>
            error.Length <= _options.MaxErrorLength
                ? error
                : error[.._options.MaxErrorLength];
    }
}
