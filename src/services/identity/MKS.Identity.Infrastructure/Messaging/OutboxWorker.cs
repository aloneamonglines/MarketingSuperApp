using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MKS.Identity.Application.Contracts.Common.Interfaces;
using MKS.Identity.Application.Contracts.Common.Options;

namespace MKS.Identity.Infrastructure.Messaging
{
    /// <summary>Continuously drains the transactional outbox with bounded polling.</summary>
    public sealed class OutboxWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<OutboxProcessingOptions> options,
        ILogger<OutboxWorker> logger) : BackgroundService
    {
        private readonly OutboxProcessingOptions _options = options.Value;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await using var scope = scopeFactory.CreateAsyncScope();
                    var processor = scope.ServiceProvider.GetRequiredService<IOutboxProcessor>();
                    var processedCount = await processor.ProcessBatchAsync(stoppingToken);
                    if (processedCount > 0)
                    {
                        continue;
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "The outbox polling cycle failed.");
                }

                await Task.Delay(_options.PollInterval, stoppingToken);
            }
        }
    }
}
