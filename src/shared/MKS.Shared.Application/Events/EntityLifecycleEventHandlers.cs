using System.Diagnostics;
using MKS.Shared.Application.Contracts.Common.Events;
using MKS.Shared.Domain.Shared.Common.Events;

namespace MKS.Shared.Application.Events
{
    /// <summary>Handles default entity-created events with standard diagnostic activity.</summary>
    public sealed class EntityCreatedEventHandler : ILocalEventHandler<EntityCreatedEvent>
    {
        public Task HandleAsync(EntityCreatedEvent localEvent, CancellationToken cancellationToken = default)
        {
            EntityLifecycleDiagnostics.Emit(localEvent, EntityLifecycleDiagnostics.CreatedActivityName);
            return Task.CompletedTask;
        }
    }

    /// <summary>Handles default entity-updated events with standard diagnostic activity.</summary>
    public sealed class EntityUpdatedEventHandler : ILocalEventHandler<EntityUpdatedEvent>
    {
        public Task HandleAsync(EntityUpdatedEvent localEvent, CancellationToken cancellationToken = default)
        {
            EntityLifecycleDiagnostics.Emit(localEvent, EntityLifecycleDiagnostics.UpdatedActivityName);
            return Task.CompletedTask;
        }
    }

    /// <summary>Handles default entity-deleted events with standard diagnostic activity.</summary>
    public sealed class EntityDeletedEventHandler : ILocalEventHandler<EntityDeletedEvent>
    {
        public Task HandleAsync(EntityDeletedEvent localEvent, CancellationToken cancellationToken = default)
        {
            EntityLifecycleDiagnostics.Emit(localEvent, EntityLifecycleDiagnostics.DeletedActivityName);
            return Task.CompletedTask;
        }
    }

    internal static class EntityLifecycleDiagnostics
    {
        private const string ActivitySourceName = "MKS.EntityLifecycle";
        private const string EntityTypeTag = "entity.type";
        private const string EntityIdTag = "entity.id";
        private const string OccurredAtTag = "event.occurred_at";
        internal const string CreatedActivityName = "entity.created";
        internal const string UpdatedActivityName = "entity.updated";
        internal const string DeletedActivityName = "entity.deleted";
        private static readonly ActivitySource ActivitySource = new(ActivitySourceName);

        internal static void Emit(EntityLifecycleEvent localEvent, string eventName)
        {
            using var activity = ActivitySource.StartActivity(eventName);
            activity?.SetTag(EntityTypeTag, localEvent.Entity.GetType().FullName);
            activity?.SetTag(EntityIdTag, localEvent.EntityId);
            activity?.SetTag(OccurredAtTag, localEvent.OccurredAt);
        }
    }
}
