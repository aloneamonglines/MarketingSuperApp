using Microsoft.Extensions.Logging;
using MKS.Identity.Domain.MultiTenancy.Entities;
using MKS.Identity.Application.Authorization.Events.Processors;
using MKS.Shared.Application.Contracts.Common.Events;
using MKS.Shared.Domain.Shared.Common.Events;
using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Identity.Application.Authorization.Events.Handlers
{
    /// <summary>Handles changes to a tenant administrator assignment.</summary>
    public sealed class TenantAdminAssignmentUpdatedEventHandler(
        ITenantAdminAssignmentEventProcessor processor,
        ILogger<TenantAdminAssignmentUpdatedEventHandler> logger) : ILocalEventHandler<EntityUpdatedEvent>
    {
        public Task HandleAsync(
            EntityUpdatedEvent localEvent,
            CancellationToken cancellationToken = default)
        {
            if (localEvent.Entity is not TenantAdminAssignmentEntity assignment)
            {
                return Task.CompletedTask;
            }

            logger.LogDebug(
                "Handling entity-updated event for tenant administrator assignment {AssignmentId}.",
                assignment.Id);
            return processor.HandleAsync(
                assignment,
                CEntityChangeType.Update,
                localEvent.OccurredAt,
                cancellationToken);
        }
    }
}
