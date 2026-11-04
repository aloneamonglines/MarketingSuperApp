namespace MKS.Shared.Infrastructure.Persistence
{
    /// <summary>Represents the lifecycle state of a queued integration event.</summary>
    public enum OutboxMessageStatus
    {
        Pending,
        Processing,
        Processed,
        DeadLettered
    }
}
