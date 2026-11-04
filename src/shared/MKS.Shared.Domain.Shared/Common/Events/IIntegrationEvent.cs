namespace MKS.Shared.Domain.Shared.Common.Events
{
    /// <summary>
    /// Represents a versioned event published between bounded contexts.
    /// </summary>
    public interface IIntegrationEvent
    {
        /// <summary>Gets the unique event identifier.</summary>
        Guid EventId { get; }
        /// <summary>Gets the application identifier associated with the event, when applicable.</summary>
        Guid? AppId { get; }
        /// <summary>Gets the tenant identifier associated with the event, when applicable.</summary>
        Guid? TenantId { get; }
        /// <summary>Gets the UTC time when the event occurred.</summary>
        DateTimeOffset OccurredAt { get; }
        /// <summary>Gets the event schema version.</summary>
        int SchemaVersion { get; }
    }
}
