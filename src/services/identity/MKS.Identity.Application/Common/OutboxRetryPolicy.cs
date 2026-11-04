using MKS.Identity.Application.Contracts.Common.Options;
using MKS.Identity.Domain.Shared.Constants;

namespace MKS.Identity.Application.Common
{
    /// <summary>Calculates bounded exponential retry delays for failed deliveries.</summary>
    public static class OutboxRetryPolicy
    {
        public static TimeSpan GetDelay(int attempt, OutboxProcessingOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);
            if (attempt < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(attempt));
            }

            if (options.InitialRetryDelay <= TimeSpan.Zero ||
                options.MaxRetryDelay <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(options),
                    IdentityConstants.ValidationMessages.RetryDelaysMustBePositive);
            }

            var multiplier = Math.Pow(2, Math.Min(attempt - 1, 30));
            var delayTicks = Math.Min(
                options.InitialRetryDelay.Ticks * multiplier,
                options.MaxRetryDelay.Ticks);
            return TimeSpan.FromTicks((long)delayTicks);
        }
    }
}
