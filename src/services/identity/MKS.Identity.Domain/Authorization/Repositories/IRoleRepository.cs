using MKS.Identity.Domain.Authorization.Entities;

namespace MKS.Identity.Domain.Authorization.Repositories
{
    /// <summary>
    /// Defines persistence operations for roles and assignments.
    /// </summary>
    public interface IRoleRepository
    {
        /// <summary>Adds a role.</summary>
        Task AddAsync(RoleEntity role, IReadOnlyCollection<RoleFunctionEntity> functions, IReadOnlyCollection<RolePermissionEntity> permissions, CancellationToken cancellationToken = default);
        /// <summary>Finds a role by identifier.</summary>
        Task<RoleEntity?> GetAsync(Guid roleId, CancellationToken cancellationToken = default);
        /// <summary>Checks whether a non-deleted role already uses a code in the exact scope.</summary>
        Task<bool> CodeExistsAsync(
            Guid appId,
            Guid? tenantId,
            string normalizedCode,
            CancellationToken cancellationToken = default);
        /// <summary>Checks that all requested catalog entries exist in the application.</summary>
        Task<bool> CatalogEntriesExistAsync(Guid appId, IReadOnlyCollection<string> functionCodes, IReadOnlyCollection<string> permissionCodes, CancellationToken cancellationToken = default);
        /// <summary>Gets permissions effectively granted by one role.</summary>
        Task<IReadOnlySet<string>> GetEffectivePermissionsAsync(Guid roleId, CancellationToken cancellationToken = default);
        /// <summary>Gets permissions represented by the requested function catalog entries.</summary>
        Task<IReadOnlySet<string>> GetFunctionPermissionsAsync(Guid appId, IReadOnlyCollection<string> functionCodes, CancellationToken cancellationToken = default);
        /// <summary>Gets active assignments for cache invalidation after a role change.</summary>
        Task<IReadOnlyList<RoleAssignmentEntity>> GetActiveAssignmentsAsync(
            Guid appId,
            Guid roleId,
            CancellationToken cancellationToken = default);
        /// <summary>Adds a role assignment.</summary>
        Task AddAssignmentAsync(RoleAssignmentEntity assignment, CancellationToken cancellationToken = default);
    }
}
