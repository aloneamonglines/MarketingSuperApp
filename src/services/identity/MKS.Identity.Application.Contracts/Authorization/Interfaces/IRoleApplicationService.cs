using MKS.Identity.Application.Contracts.Authorization.DTOs;
using MKS.Shared.Application.Contracts.Common.DTOs;
using MKS.Shared.Application.Contracts.Common.Interfaces;

namespace MKS.Identity.Application.Contracts.Authorization.Interfaces
{
    /// <summary>
    /// Provides role definition and assignment workflows.
    /// </summary>
    public interface IRoleApplicationService : IApplicationService
    {
        /// <summary>Creates an app-wide or tenant-specific role.</summary>
        Task<Guid> CreateAsync(CreateRoleDto input, CancellationToken cancellationToken = default);
        /// <summary>Atomically creates roles in the authenticated app and tenant scope.</summary>
        Task<IReadOnlyList<Guid>> CreateManyAsync(
            IReadOnlyCollection<CreateRoleDto> inputs,
            CancellationToken cancellationToken = default);
        /// <summary>Gets one role visible in the current app and tenant scope.</summary>
        Task<RoleDto> GetAsync(Guid roleId, CancellationToken cancellationToken = default);
        /// <summary>Gets multiple roles visible in the current app and tenant scope.</summary>
        Task<IReadOnlyList<RoleDto>> GetManyAsync(
            IReadOnlyCollection<Guid> roleIds,
            CancellationToken cancellationToken = default);
        /// <summary>Gets a page of roles visible in the current app and tenant scope.</summary>
        Task<PagedResultDto<RoleDto>> GetPagedAsync(
            RolePagedRequestDto request,
            CancellationToken cancellationToken = default);
        /// <summary>Updates the name and status of one role.</summary>
        Task UpdateAsync(Guid roleId, UpdateRoleDto input, CancellationToken cancellationToken = default);
        /// <summary>Updates the names and statuses of multiple roles atomically.</summary>
        Task UpdateManyAsync(
            IReadOnlyCollection<UpdateRoleItemDto> inputs,
            CancellationToken cancellationToken = default);
        /// <summary>Soft-deletes one role.</summary>
        Task DeleteAsync(Guid roleId, CancellationToken cancellationToken = default);
        /// <summary>Soft-deletes multiple roles atomically.</summary>
        Task DeleteManyAsync(
            IReadOnlyCollection<Guid> roleIds,
            CancellationToken cancellationToken = default);
        /// <summary>Assigns a role to a user in the current tenant.</summary>
        Task AssignAsync(Guid userId, Guid roleId, CancellationToken cancellationToken = default);
    }
}
