using MKS.Shared.Domain.Shared.Common.Events;

namespace MKS.Identity.Domain.Authorization.Events
{
    /// <summary>
    /// Signals that effective authorization changed for a tenant member.
    /// </summary>
    public sealed record AuthorizationChangedIntegrationEvent(
        Guid EventId,
        Guid UserId,
        Guid AppId,
        Guid TenantId,
        Guid? RoleId,
        DateTimeOffset OccurredAt,
        int SchemaVersion = 1) : IIntegrationEvent
    {
        /// <inheritdoc />
        Guid? IIntegrationEvent.AppId => AppId;

        /// <inheritdoc />
        Guid? IIntegrationEvent.TenantId => TenantId;
    }
}
