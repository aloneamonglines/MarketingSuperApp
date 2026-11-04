using MKS.Identity.Application.Contracts.Authorization.Interfaces;
using MKS.Identity.Application.Contracts.Authorization.DTOs;
using MKS.Shared.Application.Contracts.Common.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MKS.Identity.API.Controllers
{
    /// <summary>
    /// Manages app and tenant administrator assignments.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/identity")]
    public sealed class AdminAssignmentsController(IAdminAssignmentApplicationService admins)
        : ControllerBase
    {
        /// <summary>Gets a sorted page of app-admin assignments.</summary>
        [HttpGet("apps/{appId:guid}/admins")]
        [ProducesResponseType<PagedResultDto<AdminAssignmentDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAppAdminsPaged(
            Guid appId,
            [FromQuery] AdminAssignmentPagedRequestDto request,
            CancellationToken cancellationToken) =>
            Ok(await admins.GetAppAdminsPagedAsync(appId, request, cancellationToken));

        /// <summary>Gets one app-admin assignment.</summary>
        [HttpGet("apps/{appId:guid}/admins/{userId:guid}")]
        [ProducesResponseType<AdminAssignmentDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAppAdmin(
            Guid appId,
            Guid userId,
            CancellationToken cancellationToken) =>
            Ok(await admins.GetAppAdminAsync(appId, userId, cancellationToken));

        /// <summary>Gets multiple app-admin assignments.</summary>
        [HttpPost("apps/{appId:guid}/admins/many")]
        [ProducesResponseType<IReadOnlyList<AdminAssignmentDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAppAdmins(
            Guid appId,
            AdminAssignmentIdsDto input,
            CancellationToken cancellationToken) =>
            Ok(await admins.GetAppAdminsAsync(appId, input.UserIds, cancellationToken));

        /// <summary>Atomically adds or reactivates app-admin assignments.</summary>
        [HttpPost("apps/{appId:guid}/admins/bulk")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> AddAppAdmins(
            Guid appId,
            AdminAssignmentIdsDto input,
            CancellationToken cancellationToken)
        {
            await admins.AddAppAdminsAsync(appId, input.UserIds, cancellationToken);
            return NoContent();
        }

        /// <summary>Atomically removes app-admin assignments while preserving the final active administrator.</summary>
        [HttpPost("apps/{appId:guid}/admins/bulk-remove")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> RemoveAppAdmins(
            Guid appId,
            AdminAssignmentIdsDto input,
            CancellationToken cancellationToken)
        {
            await admins.RemoveAppAdminsAsync(appId, input.UserIds, cancellationToken);
            return NoContent();
        }

        /// <summary>Adds an AppAdmin to the selected application.</summary>
        [HttpPost("apps/{appId:guid}/admins/{userId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> AddAppAdmin(
            Guid appId,
            Guid userId,
            CancellationToken cancellationToken)
        {
            await admins.AddAppAdminAsync(appId, userId, cancellationToken);
            return NoContent();
        }

        /// <summary>Removes an AppAdmin without removing the final active assignment.</summary>
        [HttpDelete("apps/{appId:guid}/admins/{userId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> RemoveAppAdmin(
            Guid appId,
            Guid userId,
            CancellationToken cancellationToken)
        {
            await admins.RemoveAppAdminAsync(appId, userId, cancellationToken);
            return NoContent();
        }

        /// <summary>Gets a sorted page of tenant-admin assignments.</summary>
        [HttpGet("tenants/{tenantId:guid}/admins")]
        [ProducesResponseType<PagedResultDto<AdminAssignmentDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTenantAdminsPaged(
            Guid tenantId,
            [FromQuery] AdminAssignmentPagedRequestDto request,
            CancellationToken cancellationToken) =>
            Ok(await admins.GetTenantAdminsPagedAsync(tenantId, request, cancellationToken));

        /// <summary>Gets one tenant-admin assignment.</summary>
        [HttpGet("tenants/{tenantId:guid}/admins/{userId:guid}")]
        [ProducesResponseType<AdminAssignmentDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTenantAdmin(
            Guid tenantId,
            Guid userId,
            CancellationToken cancellationToken) =>
            Ok(await admins.GetTenantAdminAsync(tenantId, userId, cancellationToken));

        /// <summary>Gets multiple tenant-admin assignments.</summary>
        [HttpPost("tenants/{tenantId:guid}/admins/many")]
        [ProducesResponseType<IReadOnlyList<AdminAssignmentDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTenantAdmins(
            Guid tenantId,
            AdminAssignmentIdsDto input,
            CancellationToken cancellationToken) =>
            Ok(await admins.GetTenantAdminsAsync(tenantId, input.UserIds, cancellationToken));

        /// <summary>Atomically adds or reactivates tenant-admin assignments.</summary>
        [HttpPost("tenants/{tenantId:guid}/admins/bulk")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> AddTenantAdmins(
            Guid tenantId,
            AdminAssignmentIdsDto input,
            CancellationToken cancellationToken)
        {
            await admins.AddTenantAdminsAsync(tenantId, input.UserIds, cancellationToken);
            return NoContent();
        }

        /// <summary>Atomically removes tenant-admin assignments while preserving the final active administrator.</summary>
        [HttpPost("tenants/{tenantId:guid}/admins/bulk-remove")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> RemoveTenantAdmins(
            Guid tenantId,
            AdminAssignmentIdsDto input,
            CancellationToken cancellationToken)
        {
            await admins.RemoveTenantAdminsAsync(tenantId, input.UserIds, cancellationToken);
            return NoContent();
        }

        /// <summary>Adds an active tenant member as a TenantAdmin.</summary>
        [HttpPost("tenants/{tenantId:guid}/admins/{userId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> AddTenantAdmin(
            Guid tenantId,
            Guid userId,
            CancellationToken cancellationToken)
        {
            await admins.AddTenantAdminAsync(tenantId, userId, cancellationToken);
            return NoContent();
        }

        /// <summary>Removes a TenantAdmin without removing the final active assignment.</summary>
        [HttpDelete("tenants/{tenantId:guid}/admins/{userId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> RemoveTenantAdmin(
            Guid tenantId,
            Guid userId,
            CancellationToken cancellationToken)
        {
            await admins.RemoveTenantAdminAsync(tenantId, userId, cancellationToken);
            return NoContent();
        }
    }
}
