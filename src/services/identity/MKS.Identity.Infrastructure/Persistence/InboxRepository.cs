using System.Runtime.ExceptionServices;
using Microsoft.EntityFrameworkCore;
using MKS.Identity.Application.Contracts.Common.Repositories;
using MKS.Shared.Domain.Shared.Common.Events;
using MKS.Shared.Domain.Shared.Enums;
using MKS.Shared.Infrastructure.Persistence;

namespace MKS.Identity.Infrastructure.Persistence
{
    /// <summary>Persists inbox idempotency and consumer effects atomically.</summary>
    public sealed class InboxRepository(
        IdentityDbContext dbContext,
        TimeProvider timeProvider) : IInboxRepository
    {
        public async Task<bool> ProcessAsync(
            IIntegrationEvent integrationEvent,
            string consumer,
            Func<CancellationToken, Task> handler,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(integrationEvent);
            ArgumentException.ThrowIfNullOrWhiteSpace(consumer);
            ArgumentNullException.ThrowIfNull(handler);
            var scope = IntegrationEventScope.Resolve(integrationEvent);

            Exception? failure = null;
            await using (var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken))
            {
                var inboxMessage = await dbContext.InboxMessages.SingleOrDefaultAsync(
                    message => message.EventId == integrationEvent.EventId && message.Consumer == consumer,
                    cancellationToken);
                if (inboxMessage?.ProcessedAt is not null)
                {
                    await transaction.CommitAsync(cancellationToken);
                    return false;
                }

                if (inboxMessage is null)
                {
                    inboxMessage = new InboxMessageEntity
                    {
                        EventId = integrationEvent.EventId,
                        Scope = scope,
                        AppId = integrationEvent.AppId,
                        TenantId = integrationEvent.TenantId,
                        Consumer = consumer,
                        Type = integrationEvent.GetType().FullName ?? integrationEvent.GetType().Name
                    };
                    dbContext.InboxMessages.Add(inboxMessage);
                }

                inboxMessage.AttemptCount++;
                inboxMessage.ProcessedAt = timeProvider.GetUtcNow();
                inboxMessage.LastError = null;
                try
                {
                    await handler(cancellationToken);
                    await IdentityDbContextSaveChanges.SaveChangesWithLocalEventsAsync(dbContext, cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                    return true;
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    dbContext.ChangeTracker.Clear();
                    failure = exception;
                }
            }

            await RecordFailureAsync(integrationEvent, scope, consumer, failure!, cancellationToken);
            ExceptionDispatchInfo.Capture(failure!).Throw();
            return false;
        }

        private async Task RecordFailureAsync(
            IIntegrationEvent integrationEvent,
            CEntityScope scope,
            string consumer,
            Exception exception,
            CancellationToken cancellationToken)
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            var inboxMessage = await dbContext.InboxMessages.SingleOrDefaultAsync(
                message => message.EventId == integrationEvent.EventId && message.Consumer == consumer,
                cancellationToken);
            if (inboxMessage is null)
            {
                inboxMessage = new InboxMessageEntity
                {
                    EventId = integrationEvent.EventId,
                    Scope = scope,
                    AppId = integrationEvent.AppId,
                    TenantId = integrationEvent.TenantId,
                    Consumer = consumer,
                    Type = integrationEvent.GetType().FullName ?? integrationEvent.GetType().Name,
                    AttemptCount = 1
                };
                dbContext.InboxMessages.Add(inboxMessage);
            }
            else
            {
                inboxMessage.AttemptCount++;
            }

            var error = exception.ToString();
            inboxMessage.LastError = error.Length <= 4000 ? error : error[..4000];
            inboxMessage.ProcessedAt = null;
            await IdentityDbContextSaveChanges.SaveChangesWithLocalEventsAsync(dbContext, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
    }
}
