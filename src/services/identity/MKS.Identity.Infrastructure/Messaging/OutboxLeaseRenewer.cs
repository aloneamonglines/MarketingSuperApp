using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MKS.Identity.Application.Contracts.Common.Interfaces;
using MKS.Identity.Application.Contracts.Common.Options;
using MKS.Identity.Application.Contracts.Common.Repositories;

namespace MKS.Identity.Infrastructure.Messaging
{
    /// <summary>Keeps long-running delivery claims alive without sharing a DbContext.</summary>
    public sealed class OutboxLeaseRenewer(
        IServiceScopeFactory scopeFactory,
        IOptions<OutboxProcessingOptions> options,
        TimeProvider timeProvider) : IOutboxLeaseRenewer
    {
        private readonly OutboxProcessingOptions _options = options.Value;

        public async Task RenewUntilCancelledAsync(
            Guid messageId,
            Guid lockId,
            CancellationTokenSource deliveryCancellation)
        {
            var interval = TimeSpan.FromTicks(Math.Max(1, _options.LeaseDuration.Ticks / 3));
            var cancellationToken = deliveryCancellation.Token;
            try
            {
                while (true)
                {
                    await Task.Delay(interval, cancellationToken);
                    await using var scope = scopeFactory.CreateAsyncScope();
                    var repository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
                    var renewed = await repository.RenewLeaseAsync(
                        messageId,
                        lockId,
                        timeProvider.GetUtcNow().Add(_options.LeaseDuration),
                        cancellationToken);
                    if (!renewed)
                    {
                        deliveryCancellation.Cancel();
                        return;
                    }
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
            }
        }
    }
}
