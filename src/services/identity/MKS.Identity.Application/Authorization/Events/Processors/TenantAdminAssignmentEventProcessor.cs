using Microsoft.Extensions.Logging;
using MKS.Identity.Application.Contracts.Common.Interfaces;
using MKS.Identity.Application.Contracts.Common.Repositories;
using MKS.Identity.Domain.Authorization.Events;
using MKS.Identity.Domain.MultiTenancy.Entities;
using MKS.Identity.Domain.Users.Entities;
using MKS.Identity.Domain.Shared.Constants;
using MKS.Shared.Application.Contracts.Common.Interfaces;
using MKS.Shared.Domain.MultiTenancy.Repositories;
using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Identity.Application.Authorization.Events.Processors
{
    /// <summary>
    /// Applies tenant-admin authorization side effects after the assignment change has been persisted.
    /// </summary>
    public sealed class TenantAdminAssignmentEventProcessor(
        ICurrentContext currentContext,
        IIdentityEntityRepository<UserEntity> userEntities,
        ITenantRepository<TenantEntity> tenantRepository,
        ITenantScopeAccessor tenantScope,
        ISecurityAuditLogWriter securityAuditLogWriter,
        IIntegrationEventPublisher integrationEventPublisher,
        ILogger<TenantAdminAssignmentEventProcessor> logger) : ITenantAdminAssignmentEventProcessor
    {
        public async Task HandleAsync(
            TenantAdminAssignmentEntity assignment,
            CEntityChangeType changeType,
            DateTimeOffset occurredAt,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(assignment);
            if (changeType == CEntityChangeType.Create && !assignment.IsActive)
            {
                logger.LogDebug(
                    "Skipping inactive tenant administrator assignment {AssignmentId} created for tenant {TenantId} and user {UserId}.",
                    assignment.Id,
                    assignment.TenantId,
                    assignment.UserId);
                return;
            }

            logger.LogDebug(
                "Processing tenant administrator assignment {AssignmentId} change {ChangeType} for tenant {TenantId} and user {UserId}.",
                assignment.Id,
                changeType,
                assignment.TenantId,
                assignment.UserId);

            var tenant = await tenantRepository.GetAsync(assignment.TenantId, cancellationToken)
                ?? throw new InvalidOperationException(
                    $"Tenant '{assignment.TenantId}' for an administrator assignment was not found.");
            using var scope = tenantScope.Enter(tenant.Id);

            var user = await userEntities.GetAsync(assignment.UserId, cancellationToken)
                ?? throw new InvalidOperationException(
                    $"User '{assignment.UserId}' for a tenant administrator assignment was not found.");
            var actorId = currentContext.UserId ?? assignment.ModifierId ?? assignment.CreatorId;
            user.IncrementMembershipVersion();
            user.ModifierId = actorId;
            user.ModifiedAt = occurredAt;
            await userEntities.UpdatePropertiesAsync(
                user,
                [
                    nameof(UserEntity.MembershipVersion),
                    nameof(UserEntity.ModifierId),
                    nameof(UserEntity.ModifiedAt)
                ],
                cancellationToken);

            var action = assignment.IsActive
                ? IdentityConstants.AuditActions.TenantAdminAdded
                : IdentityConstants.AuditActions.TenantAdminRemoved;
            await securityAuditLogWriter.AppendAsync(
                actorId,
                currentContext.ImpersonatorId,
                currentContext.BreakGlassGrantId,
                action,
                nameof(TenantAdminAssignmentEntity),
                assignment.UserId.ToString(),
                $"TenantId={tenant.Id}",
                occurredAt,
                cancellationToken);

            await integrationEventPublisher.PublishAsync(
                new AuthorizationChangedIntegrationEvent(
                    Guid.CreateVersion7(),
                    assignment.UserId,
                    tenant.AppId,
                    tenant.Id,
                    null,
                    occurredAt),
                cancellationToken);

            logger.LogInformation(
                "Processed tenant administrator assignment {AssignmentId} change {ChangeType} for tenant {TenantId} and user {UserId}. Membership version is now {MembershipVersion}.",
                assignment.Id,
                changeType,
                tenant.Id,
                user.Id,
                user.MembershipVersion);
        }
    }

    public interface ITenantAdminAssignmentEventProcessor
    {
        Task HandleAsync(
            TenantAdminAssignmentEntity assignment,
            CEntityChangeType changeType,
            DateTimeOffset occurredAt,
            CancellationToken cancellationToken);
    }
}
