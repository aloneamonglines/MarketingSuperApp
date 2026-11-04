using MKS.Shared.Domain.Shared.Common.Events;

namespace MKS.Identity.Domain.Users.Events
{
    /// <summary>
    /// Announces creation of a global user without publishing credentials or contact details.
    /// </summary>
    public sealed record UserCreatedIntegrationEvent(
        Guid EventId,
        Guid UserId,
        Guid AppId,
        Guid TenantId,
        DateTimeOffset OccurredAt,
        int SchemaVersion = 1) : IIntegrationEvent
    {
        /// <inheritdoc />
        Guid? IIntegrationEvent.AppId => AppId;

        /// <inheritdoc />
        Guid? IIntegrationEvent.TenantId => TenantId;
    }
}
