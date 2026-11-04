namespace MKS.Shared.Domain.Shared.Common.Events
{
    /// <summary>Base data for an automatically generated entity lifecycle event.</summary>
    public abstract record EntityLifecycleEvent(
        object Entity,
        Guid EntityId,
        DateTimeOffset OccurredAt);

    /// <summary>Raised after an entity is inserted successfully.</summary>
    public sealed record EntityCreatedEvent(
        object Entity,
        Guid EntityId,
        DateTimeOffset OccurredAt)
        : EntityLifecycleEvent(Entity, EntityId, OccurredAt);

    /// <summary>Raised after an entity is updated successfully.</summary>
    public sealed record EntityUpdatedEvent(
        object Entity,
        Guid EntityId,
        DateTimeOffset OccurredAt)
        : EntityLifecycleEvent(Entity, EntityId, OccurredAt);

    /// <summary>Raised after an entity is deleted or soft-deleted successfully.</summary>
    public sealed record EntityDeletedEvent(
        object Entity,
        Guid EntityId,
        DateTimeOffset OccurredAt)
        : EntityLifecycleEvent(Entity, EntityId, OccurredAt);
}
