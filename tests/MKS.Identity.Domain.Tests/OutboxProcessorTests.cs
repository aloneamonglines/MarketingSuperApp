using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MKS.Identity.Application.Common;
using MKS.Identity.Application.Contracts.Common.DTOs;
using MKS.Identity.Application.Contracts.Common.Interfaces;
using MKS.Identity.Application.Contracts.Common.Options;
using MKS.Identity.Application.Contracts.Common.Repositories;
using MKS.Shared.Domain.Shared.Common.Events;
using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Identity.Domain.Tests;

public sealed class OutboxProcessorTests
{
    [Fact]
    public async Task FailedDispatchIsRecordedForRetryByOutboxRepository()
    {
        var integrationEvent = new TestIntegrationEvent(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            DateTimeOffset.UtcNow);
        var appId = integrationEvent.AppId;
        var tenantId = integrationEvent.TenantId;
        var lease = new OutboxMessageLease(
            Guid.CreateVersion7(),
            integrationEvent.EventId,
            CEntityScope.Tenant,
            appId,
            tenantId,
            typeof(TestIntegrationEvent).FullName!,
            System.Text.Json.JsonSerializer.Serialize(integrationEvent),
            integrationEvent.OccurredAt,
            1,
            Guid.CreateVersion7());
        var repository = new FakeOutboxRepository(lease);
        var dispatcher = new FakeDispatcher(new InvalidOperationException("consumer failed"));
        var processor = new OutboxProcessor(
            repository,
            dispatcher,
            new FakeLeaseRenewer(),
            [new FakeHandler()],
            Options.Create(new OutboxProcessingOptions
            {
                BatchSize = 1,
                InitialRetryDelay = TimeSpan.FromSeconds(3),
                MaxRetryDelay = TimeSpan.FromMinutes(1)
            }),
            TimeProvider.System,
            NullLogger<OutboxProcessor>.Instance);

        var processedCount = await processor.ProcessBatchAsync();

        Assert.Equal(1, processedCount);
        Assert.Equal(1, dispatcher.DispatchCount);
        Assert.Equal(lease.EventId, repository.FailedEventId);
        Assert.Equal(TimeSpan.FromSeconds(3), repository.RetryDelay);
        Assert.Null(repository.ProcessedEventId);
    }

    private sealed record TestIntegrationEvent(
        Guid EventId,
        Guid? AppId,
        Guid? TenantId,
        DateTimeOffset OccurredAt,
        int SchemaVersion = 1) : IIntegrationEvent;

    private sealed class FakeHandler : IIntegrationEventHandler
    {
        public Type EventType => typeof(TestIntegrationEvent);

        public Task HandleAsync(
            IIntegrationEvent integrationEvent,
            CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeDispatcher(Exception exception) : IIntegrationEventDispatcher
    {
        public int DispatchCount { get; private set; }

        public Task DispatchAsync(
            IIntegrationEvent integrationEvent,
            CancellationToken cancellationToken = default)
        {
            DispatchCount++;
            return Task.FromException(exception);
        }
    }

    private sealed class FakeLeaseRenewer : IOutboxLeaseRenewer
    {
        public Task RenewUntilCancelledAsync(
            Guid messageId,
            Guid lockId,
            CancellationTokenSource deliveryCancellation) =>
            Task.Delay(Timeout.InfiniteTimeSpan, deliveryCancellation.Token);
    }

    private sealed class FakeOutboxRepository(OutboxMessageLease lease) : IOutboxRepository
    {
        private bool _claimed;

        public Guid? FailedEventId { get; private set; }
        public Guid? ProcessedEventId { get; private set; }
        public TimeSpan? RetryDelay { get; private set; }

        public Task AddAsync(
            Guid eventId,
            CEntityScope scope,
            Guid? appId,
            Guid? tenantId,
            string type,
            string payload,
            DateTimeOffset occurredAt,
            DateTimeOffset nextAttemptAt,
            CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<OutboxMessageLease?> ClaimNextAsync(
            DateTimeOffset now,
            TimeSpan leaseDuration,
            OutboxMessageFilter? filter = null,
            CancellationToken cancellationToken = default)
        {
            if (_claimed)
            {
                return Task.FromResult<OutboxMessageLease?>(null);
            }

            _claimed = true;
            return Task.FromResult<OutboxMessageLease?>(lease);
        }

        public Task<bool> RenewLeaseAsync(
            Guid messageId,
            Guid lockId,
            DateTimeOffset lockedUntil,
            CancellationToken cancellationToken = default) => Task.FromResult(true);

        public Task MarkProcessedAsync(
            OutboxMessageLease outboxMessageLease,
            DateTimeOffset processedAt,
            CancellationToken cancellationToken = default)
        {
            ProcessedEventId = outboxMessageLease.EventId;
            return Task.CompletedTask;
        }

        public Task<bool> RecordFailureAsync(
            OutboxMessageLease outboxMessageLease,
            string error,
            DateTimeOffset failedAt,
            int maxAttempts,
            TimeSpan retryDelay,
            CancellationToken cancellationToken = default)
        {
            FailedEventId = outboxMessageLease.EventId;
            RetryDelay = retryDelay;
            return Task.FromResult(false);
        }

        public Task<bool> ReplayDeadLetterAsync(
            Guid eventId,
            DateTimeOffset nextAttemptAt,
            CancellationToken cancellationToken = default) => Task.FromResult(false);
    }
}
