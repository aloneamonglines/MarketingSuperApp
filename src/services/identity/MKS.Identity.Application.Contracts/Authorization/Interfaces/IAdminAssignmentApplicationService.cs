using MKS.Identity.Application.Contracts.Authorization.DTOs;
using MKS.Shared.Application.Contracts.Common.DTOs;

namespace MKS.Identity.Application.Contracts.Authorization.Interfaces
{
    /// <summary>
    /// Provides scoped application and tenant administrator assignment operations.
    /// </summary>
    public interface IAdminAssignmentApplicationService
    {
        /// <summary>Gets an app-admin assignment after verifying the caller's app scope.</summary>
        Task<AdminAssignmentDto> GetAppAdminAsync(Guid appId, Guid userId, CancellationToken cancellationToken = default);
        /// <summary>Gets multiple app-admin assignments after verifying the caller's app scope.</summary>
        Task<IReadOnlyList<AdminAssignmentDto>> GetAppAdminsAsync(
            Guid appId,
            IReadOnlyCollection<Guid> userIds,
            CancellationToken cancellationToken = default);
        /// <summary>Gets a sorted page of app-admin assignments in the selected app.</summary>
        Task<PagedResultDto<AdminAssignmentDto>> GetAppAdminsPagedAsync(
            Guid appId,
            AdminAssignmentPagedRequestDto request,
            CancellationToken cancellationToken = default);
        /// <summary>Adds an active AppAdmin to an application.</summary>
        Task AddAppAdminAsync(Guid appId, Guid userId, CancellationToken cancellationToken = default);
        /// <summary>Atomically adds or reactivates app-admin assignments.</summary>
        Task AddAppAdminsAsync(Guid appId, IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken = default);
        /// <summary>Removes an AppAdmin unless it is the application's final active administrator.</summary>
        Task RemoveAppAdminAsync(Guid appId, Guid userId, CancellationToken cancellationToken = default);
        /// <summary>Atomically removes app-admin assignments without removing the final active administrator.</summary>
        Task RemoveAppAdminsAsync(Guid appId, IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken = default);
        /// <summary>Gets a tenant-admin assignment after verifying the caller's tenant scope.</summary>
        Task<AdminAssignmentDto> GetTenantAdminAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
        /// <summary>Gets multiple tenant-admin assignments after verifying the caller's tenant scope.</summary>
        Task<IReadOnlyList<AdminAssignmentDto>> GetTenantAdminsAsync(
            Guid tenantId,
            IReadOnlyCollection<Guid> userIds,
            CancellationToken cancellationToken = default);
        /// <summary>Gets a sorted page of tenant-admin assignments in the selected tenant.</summary>
        Task<PagedResultDto<AdminAssignmentDto>> GetTenantAdminsPagedAsync(
            Guid tenantId,
            AdminAssignmentPagedRequestDto request,
            CancellationToken cancellationToken = default);
        /// <summary>Adds an active member as a tenant administrator.</summary>
        Task AddTenantAdminAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
        /// <summary>Atomically adds or reactivates tenant-admin assignments.</summary>
        Task AddTenantAdminsAsync(Guid tenantId, IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken = default);
        /// <summary>Removes a TenantAdmin unless it is the tenant's final active administrator.</summary>
        Task RemoveTenantAdminAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
        /// <summary>Atomically removes tenant-admin assignments without removing the final active administrator.</summary>
        Task RemoveTenantAdminsAsync(Guid tenantId, IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken = default);
    }
}
