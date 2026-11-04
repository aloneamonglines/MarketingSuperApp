using Microsoft.EntityFrameworkCore;
using MKS.Identity.Domain.Authorization.Entities;
using MKS.Identity.Domain.Authorization.Repositories;
using MKS.Identity.Domain.MultiTenancy.Entities;
using MKS.Identity.Domain.Shared.Enums;
using MKS.Identity.Infrastructure.Persistence;

namespace MKS.Identity.Infrastructure.Authorization
{
    /// <summary>
    /// Executes authorization lookups constrained to the requested application and tenant.
    /// </summary>
    public sealed class AuthorizationRepository(IdentityDbContext dbContext) : IAuthorizationRepository
    {
        /// <inheritdoc />
        public Task<bool> IsAppAdminAsync(Guid userId, Guid appId, CancellationToken cancellationToken = default) =>
            dbContext.AppAdminAssignments.AnyAsync(
                assignment => assignment.UserId == userId && assignment.AppId == appId && assignment.IsActive,
                cancellationToken);

        /// <inheritdoc />
        public Task<bool> IsTenantAdminAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default) =>
            dbContext.TenantAdminAssignments.AnyAsync(
                assignment => assignment.UserId == userId && assignment.TenantId == tenantId && assignment.IsActive,
                cancellationToken);

        /// <inheritdoc />
        public async Task<IReadOnlySet<string>> GetRolePermissionsAsync(
            Guid userId,
            Guid appId,
            Guid tenantId,
            CancellationToken cancellationToken = default)
        {
            var assignedRoleIds = dbContext.RoleAssignments
                .Where(assignment =>
                    assignment.UserId == userId &&
                    assignment.AppId == appId &&
                    assignment.TenantId == tenantId &&
                    assignment.IsActive)
                .Select(assignment => assignment.RoleId);

            var explicitPermissions =
                from link in dbContext.RolePermissions
                join permission in dbContext.AppPermissions
                    on new { link.AppId, Code = link.PermissionCode }
                    equals new { permission.AppId, permission.Code }
                where link.AppId == appId &&
                      assignedRoleIds.Contains(link.RoleId) &&
                      !permission.Deprecated
                select permission.Code;
            var functionPermissions =
                from roleFunction in dbContext.RoleFunctions
                join function in dbContext.AppFunctions
                    on new { roleFunction.AppId, roleFunction.FunctionCode }
                    equals new { function.AppId, FunctionCode = function.Code }
                join permission in dbContext.AppPermissions
                    on new { roleFunction.AppId, roleFunction.FunctionCode }
                    equals new { permission.AppId, FunctionCode = permission.FunctionCode! }
                where roleFunction.AppId == appId &&
                      assignedRoleIds.Contains(roleFunction.RoleId) &&
                      !function.Deprecated &&
                      !permission.Deprecated
                select permission.Code;

            var permissionCodes = await explicitPermissions
                .Union(functionPermissions)
                .Distinct()
                .ToListAsync(cancellationToken);
            return permissionCodes.ToHashSet(StringComparer.Ordinal);
        }

        /// <inheritdoc />
        public Task<TenantMembershipEntity?> GetMembershipAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default) =>
            dbContext.TenantMemberships.SingleOrDefaultAsync(
                membership => membership.UserId == userId &&
                              membership.TenantId == tenantId &&
                              membership.Status == CMembershipStatus.Active,
                cancellationToken);

        /// <inheritdoc />
        public async Task<IReadOnlySet<string>> GetPlatformPermissionsAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var roles = await dbContext.PlatformRoleAssignments
                .Where(assignment => assignment.UserId == userId && assignment.IsActive)
                .Select(assignment => assignment.RoleCode)
                .ToListAsync(cancellationToken);
            return roles.Select(role => $"platform:{role}:*").ToHashSet(StringComparer.Ordinal);
        }
    }
}
