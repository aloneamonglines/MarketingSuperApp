using System.Text.Json;
using MKS.Identity.Application.Common;
using MKS.Identity.Application.Contracts.Common.DTOs;
using MKS.Identity.Application.Contracts.Common.Interfaces;
using MKS.Identity.Application.Contracts.Common.Repositories;
using MKS.Shared.Domain.Shared.Common.Events;
using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Identity.Domain.Tests;

public sealed class IntegrationEventPublisherTests
{
    [Fact]
    public async Task PublishDelegatesSerializedEventToOutboxRepository()
    {
        var eventId = Guid.CreateVersion7();
        var occurredAt = new DateTimeOffset(2026, 11, 4, 3, 0, 0, TimeSpan.Zero);
        var eventTime = new FixedTimeProvider(occurredAt.AddMinutes(1));
        var repository = new RecordingOutboxRepository();
        var publisher = new IntegrationEventPublisher(repository, eventTime);
        var appId = Guid.CreateVersion7();
        var tenantId = Guid.CreateVersion7();
        var integrationEvent = new TestIntegrationEvent(eventId, appId, tenantId, occurredAt);

        await publisher.PublishAsync(integrationEvent);

        Assert.Equal(eventId, repository.EventId);
        Assert.Equal(CEntityScope.Tenant, repository.Scope);
        Assert.Equal(appId, repository.AppId);
        Assert.Equal(tenantId, repository.TenantId);
        Assert.Equal(typeof(TestIntegrationEvent).FullName, repository.Type);
        Assert.Equal(occurredAt, repository.OccurredAt);
        Assert.Equal(eventTime.GetUtcNow(), repository.NextAttemptAt);
        using var payload = JsonDocument.Parse(repository.Payload!);
        Assert.Equal(
            eventId,
            payload.RootElement.GetProperty(nameof(IIntegrationEvent.EventId)).GetGuid());
    }

    [Fact]
    public async Task PublishMarksGlobalEventsWithoutScopeIdentifiers()
    {
        var repository = new RecordingOutboxRepository();
        var publisher = new IntegrationEventPublisher(repository, TimeProvider.System);

        await publisher.PublishAsync(new TestIntegrationEvent(
            Guid.CreateVersion7(),
            null,
            null,
            TimeProvider.System.GetUtcNow()));

        Assert.Equal(CEntityScope.Global, repository.Scope);
        Assert.Null(repository.AppId);
        Assert.Null(repository.TenantId);
    }

    [Fact]
    public async Task PublishMarksAppScopedEventsWithoutTenantIdentifier()
    {
        var appId = Guid.CreateVersion7();
        var repository = new RecordingOutboxRepository();
        var publisher = new IntegrationEventPublisher(repository, TimeProvider.System);

        await publisher.PublishAsync(new TestIntegrationEvent(
            Guid.CreateVersion7(),
            appId,
            null,
            TimeProvider.System.GetUtcNow()));

        Assert.Equal(CEntityScope.App, repository.Scope);
        Assert.Equal(appId, repository.AppId);
        Assert.Null(repository.TenantId);
    }

    [Fact]
    public async Task PublishRejectsTenantScopedEventsWithoutAppIdentifier()
    {
        var repository = new RecordingOutboxRepository();
        var publisher = new IntegrationEventPublisher(repository, TimeProvider.System);

        await Assert.ThrowsAsync<ArgumentException>(() => publisher.PublishAsync(
            new TestIntegrationEvent(
                Guid.CreateVersion7(),
                null,
                Guid.CreateVersion7(),
                TimeProvider.System.GetUtcNow())));

        Assert.Null(repository.EventId);
    }

    [Fact]
    public async Task PublishRejectsEventsWithoutAnIdentifier()
    {
        var repository = new RecordingOutboxRepository();
        var publisher = new IntegrationEventPublisher(repository, TimeProvider.System);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            publisher.PublishAsync(new TestIntegrationEvent(
                Guid.Empty,
                Guid.CreateVersion7(),
                null,
                TimeProvider.System.GetUtcNow())));

        Assert.Null(repository.EventId);
    }

    private sealed record TestIntegrationEvent(
        Guid EventId,
        Guid? AppId,
        Guid? TenantId,
        DateTimeOffset OccurredAt,
        int SchemaVersion = 1) : IIntegrationEvent;

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class RecordingOutboxRepository : IOutboxRepository
    {
        public Guid? EventId { get; private set; }
        public CEntityScope? Scope { get; private set; }
        public Guid? AppId { get; private set; }
        public Guid? TenantId { get; private set; }
        public string? Type { get; private set; }
        public string? Payload { get; private set; }
        public DateTimeOffset OccurredAt { get; private set; }
        public DateTimeOffset NextAttemptAt { get; private set; }

        public Task AddAsync(
            Guid eventId,
            CEntityScope scope,
            Guid? appId,
            Guid? tenantId,
            string type,
            string payload,
            DateTimeOffset occurredAt,
            DateTimeOffset nextAttemptAt,
            CancellationToken cancellationToken = default)
        {
            EventId = eventId;
            Scope = scope;
            AppId = appId;
            TenantId = tenantId;
            Type = type;
            Payload = payload;
            OccurredAt = occurredAt;
            NextAttemptAt = nextAttemptAt;
            return Task.CompletedTask;
        }

        public Task<OutboxMessageLease?> ClaimNextAsync(
            DateTimeOffset now,
            TimeSpan leaseDuration,
            OutboxMessageFilter? filter = null,
            CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task<bool> RenewLeaseAsync(
            Guid messageId,
            Guid lockId,
            DateTimeOffset lockedUntil,
            CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task MarkProcessedAsync(
            OutboxMessageLease lease,
            DateTimeOffset processedAt,
            CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task<bool> RecordFailureAsync(
            OutboxMessageLease lease,
            string error,
            DateTimeOffset failedAt,
            int maxAttempts,
            TimeSpan retryDelay,
            CancellationToken cancellationToken = default) => throw new NotImplementedException();

        public Task<bool> ReplayDeadLetterAsync(
            Guid eventId,
            DateTimeOffset nextAttemptAt,
            CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }
}
