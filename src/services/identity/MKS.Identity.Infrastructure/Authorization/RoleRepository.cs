using Microsoft.EntityFrameworkCore;
using MKS.Identity.Domain.Authorization.Entities;
using MKS.Identity.Domain.Authorization.Repositories;
using MKS.Identity.Infrastructure.Persistence;

namespace MKS.Identity.Infrastructure.Authorization
{
    /// <summary>
    /// Persists roles and validates application-owned permission catalogs.
    /// </summary>
    public sealed class RoleRepository(IdentityDbContext dbContext) : IRoleRepository
    {
        /// <inheritdoc />
        public async Task AddAsync(
            RoleEntity role,
            IReadOnlyCollection<RoleFunctionEntity> functions,
            IReadOnlyCollection<RolePermissionEntity> permissions,
            CancellationToken cancellationToken = default)
        {
            await dbContext.Roles.AddAsync(role, cancellationToken);
            await dbContext.RoleFunctions.AddRangeAsync(functions, cancellationToken);
            await dbContext.RolePermissions.AddRangeAsync(permissions, cancellationToken);
        }

        /// <inheritdoc />
        public Task<RoleEntity?> GetAsync(Guid roleId, CancellationToken cancellationToken = default) =>
            dbContext.Roles.SingleOrDefaultAsync(role => role.Id == roleId, cancellationToken);

        /// <inheritdoc />
        public Task<bool> CodeExistsAsync(
            Guid appId,
            Guid? tenantId,
            string normalizedCode,
            CancellationToken cancellationToken = default) =>
            dbContext.Roles
                .IgnoreQueryFilters()
                .AnyAsync(
                    role => role.AppId == appId &&
                            role.TenantId == tenantId &&
                            role.Code == normalizedCode &&
                            !role.IsDeleted,
                    cancellationToken);

        /// <inheritdoc />
        public async Task<bool> CatalogEntriesExistAsync(
            Guid appId,
            IReadOnlyCollection<string> functionCodes,
            IReadOnlyCollection<string> permissionCodes,
            CancellationToken cancellationToken = default)
        {
            var functionsExist = await dbContext.AppFunctions.CountAsync(
                function => function.AppId == appId && functionCodes.Contains(function.Code) && !function.Deprecated,
                cancellationToken);
            var permissionsExist = await dbContext.AppPermissions.CountAsync(
                permission => permission.AppId == appId && permissionCodes.Contains(permission.Code) && !permission.Deprecated,
                cancellationToken);
            return functionsExist == functionCodes.Distinct(StringComparer.Ordinal).Count() &&
                   permissionsExist == permissionCodes.Distinct(StringComparer.Ordinal).Count();
        }

        /// <inheritdoc />
        public async Task<IReadOnlySet<string>> GetEffectivePermissionsAsync(
            Guid roleId,
            CancellationToken cancellationToken = default)
        {
            var explicitPermissions =
                from link in dbContext.RolePermissions
                join permission in dbContext.AppPermissions
                    on new { link.AppId, Code = link.PermissionCode }
                    equals new { permission.AppId, permission.Code }
                where link.RoleId == roleId && !permission.Deprecated
                select permission.Code;
            var functionPermissions =
                from roleFunction in dbContext.RoleFunctions
                join function in dbContext.AppFunctions
                    on new { roleFunction.AppId, roleFunction.FunctionCode }
                    equals new { function.AppId, FunctionCode = function.Code }
                join permission in dbContext.AppPermissions
                    on new { roleFunction.AppId, roleFunction.FunctionCode }
                    equals new { permission.AppId, FunctionCode = permission.FunctionCode! }
                where roleFunction.RoleId == roleId && !function.Deprecated && !permission.Deprecated
                select permission.Code;
            var codes = await explicitPermissions.Union(functionPermissions).Distinct().ToListAsync(cancellationToken);
            return codes.ToHashSet(StringComparer.Ordinal);
        }

        /// <inheritdoc />
        public async Task<IReadOnlySet<string>> GetFunctionPermissionsAsync(
            Guid appId,
            IReadOnlyCollection<string> functionCodes,
            CancellationToken cancellationToken = default)
        {
            var codes = await dbContext.AppPermissions
                .Where(permission => permission.AppId == appId &&
                                     !permission.Deprecated &&
                                     permission.FunctionCode != null &&
                                     functionCodes.Contains(permission.FunctionCode))
                .Select(permission => permission.Code)
                .ToListAsync(cancellationToken);
            return codes.ToHashSet(StringComparer.Ordinal);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<RoleAssignmentEntity>> GetActiveAssignmentsAsync(
            Guid appId,
            Guid roleId,
            CancellationToken cancellationToken = default) =>
            await dbContext.RoleAssignments
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(assignment =>
                    assignment.AppId == appId &&
                    assignment.RoleId == roleId &&
                    assignment.IsActive &&
                    !assignment.IsDeleted)
                .ToListAsync(cancellationToken);

        /// <inheritdoc />
        public async Task AddAssignmentAsync(RoleAssignmentEntity assignment, CancellationToken cancellationToken = default) =>
            await dbContext.RoleAssignments.AddAsync(assignment, cancellationToken);
    }
}
