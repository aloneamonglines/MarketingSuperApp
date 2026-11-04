using MKS.Identity.Domain.Shared.Constants;
using MKS.Identity.Domain.Authorization.Repositories;

namespace MKS.Identity.Domain.Authorization.Services
{
    /// <summary>
    /// Evaluates effective permissions with strict platform, app, and tenant boundaries.
    /// </summary>
    public sealed class PermissionEvaluator(IAuthorizationRepository repository)
    {
        /// <summary>Checks the effective permission without allowing platform roles to imply tenant access.</summary>
        public async Task<bool> HasPermissionAsync(
            Guid userId,
            Guid appId,
            Guid? tenantId,
            string permissionCode,
            CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(permissionCode);
            var platformPermissions = await repository.GetPlatformPermissionsAsync(userId, cancellationToken);
            if (tenantId is null &&
                (platformPermissions.Contains(permissionCode) ||
                 platformPermissions.Contains(IdentityConstants.Permissions.PlatformSuperAdminWildcard) &&
                 permissionCode.StartsWith(IdentityConstants.PermissionPrefixes.Platform, StringComparison.Ordinal)))
            {
                return true;
            }

            if (await repository.IsAppAdminAsync(userId, appId, cancellationToken))
            {
                return true;
            }

            if (tenantId is not { } targetTenantId)
            {
                return false;
            }

            var membership = await repository.GetMembershipAsync(userId, targetTenantId, cancellationToken);
            if (membership is null || membership.AppId != appId)
            {
                return false;
            }

            if (await repository.IsTenantAdminAsync(userId, targetTenantId, cancellationToken) &&
                permissionCode.StartsWith(IdentityConstants.PermissionPrefixes.TenantAdmin, StringComparison.Ordinal))
            {
                return true;
            }

            var permissions = await repository.GetRolePermissionsAsync(
                userId,
                appId,
                targetTenantId,
                cancellationToken);
            return permissions.Contains(permissionCode);
        }
    }
}
