using MKS.Shared.Domain.Shared.Enums;
using MKS.Shared.Domain.Shared.Constants;

namespace MKS.Shared.Domain.Shared.Common.Events
{
    /// <summary>Resolves and validates the isolation scope carried by an integration event.</summary>
    public static class IntegrationEventScope
    {
        public static CEntityScope Resolve(IIntegrationEvent integrationEvent)
        {
            ArgumentNullException.ThrowIfNull(integrationEvent);
            if (integrationEvent.TenantId is { } tenantId)
            {
                if (tenantId == Guid.Empty ||
                    integrationEvent.AppId is not { } tenantAppId ||
                    tenantAppId == Guid.Empty)
                {
                    throw new ArgumentException(
                        IntegrationEventConstants.InvalidScopeIdentifiers,
                        nameof(integrationEvent));
                }

                return CEntityScope.Tenant;
            }

            if (integrationEvent.AppId is { } appId)
            {
                if (appId == Guid.Empty)
                {
                    throw new ArgumentException(
                        IntegrationEventConstants.InvalidScopeIdentifiers,
                        nameof(integrationEvent));
                }

                return CEntityScope.App;
            }

            return CEntityScope.Global;
        }
    }
}
