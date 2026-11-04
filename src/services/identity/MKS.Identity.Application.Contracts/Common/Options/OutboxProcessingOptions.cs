namespace MKS.Identity.Application.Contracts.Common.Options
{
    /// <summary>Controls durable outbox polling, leasing, retries, and dead-lettering.</summary>
    public sealed class OutboxProcessingOptions
    {
        public const string SectionName = "Messaging:Outbox";

        public int BatchSize { get; set; } = 50;
        public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(5);
        public TimeSpan LeaseDuration { get; set; } = TimeSpan.FromMinutes(1);
        public int MaxAttempts { get; set; } = 10;
        public TimeSpan InitialRetryDelay { get; set; } = TimeSpan.FromSeconds(1);
        public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromMinutes(15);
        public int MaxErrorLength { get; set; } = 4000;
    }
}
