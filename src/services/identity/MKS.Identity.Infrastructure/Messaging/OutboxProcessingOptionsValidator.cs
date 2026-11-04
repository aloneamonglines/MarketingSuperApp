using MKS.Identity.Application.Contracts.Common.Options;
using MKS.Identity.Domain.Shared.Constants;

namespace MKS.Identity.Infrastructure.Messaging
{
    internal static class OutboxProcessingOptionsValidator
    {
        public static bool IsValid(OutboxProcessingOptions options) =>
            options.BatchSize > 0 &&
            options.PollInterval > TimeSpan.Zero &&
            options.LeaseDuration > TimeSpan.Zero &&
            options.MaxAttempts > 0 &&
            options.InitialRetryDelay > TimeSpan.Zero &&
            options.MaxRetryDelay >= options.InitialRetryDelay &&
            options.MaxErrorLength > 0;
    }
}
