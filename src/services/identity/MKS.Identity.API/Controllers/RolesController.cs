using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MKS.Identity.Application.Contracts.Authorization.DTOs;
using MKS.Identity.Application.Contracts.Authorization.Interfaces;
using MKS.Shared.Application.Contracts.Common.DTOs;

namespace MKS.Identity.API.Controllers
{
    /// <summary>
    /// Exposes application and tenant role administration endpoints.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/identity/roles")]
    public sealed class RolesController(IRoleApplicationService roles) : ControllerBase
    {
        /// <summary>Creates a role using only catalog-defined app permissions and functions.</summary>
        [HttpPost]
        [ProducesResponseType<Guid>(StatusCodes.Status201Created)]
        public async Task<IActionResult> Create(
            CreateRoleDto input,
            CancellationToken cancellationToken)
        {
            var roleId = await roles.CreateAsync(input, cancellationToken);
            return CreatedAtAction(nameof(Create), new { id = roleId }, new { id = roleId });
        }

        /// <summary>Atomically creates roles in the authenticated app and tenant scope.</summary>
        [HttpPost("bulk")]
        [ProducesResponseType<IReadOnlyList<Guid>>(StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateMany(
            IReadOnlyCollection<CreateRoleDto> inputs,
            CancellationToken cancellationToken) =>
            StatusCode(StatusCodes.Status201Created, await roles.CreateManyAsync(inputs, cancellationToken));

        /// <summary>Gets roles visible in the authenticated app and tenant scope.</summary>
        [HttpGet]
        [ProducesResponseType<PagedResultDto<RoleDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaged(
            [FromQuery] RolePagedRequestDto request,
            CancellationToken cancellationToken) =>
            Ok(await roles.GetPagedAsync(request, cancellationToken));

        /// <summary>Gets one role visible in the authenticated app and tenant scope.</summary>
        [HttpGet("{roleId:guid}")]
        [ProducesResponseType<RoleDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(Guid roleId, CancellationToken cancellationToken) =>
            Ok(await roles.GetAsync(roleId, cancellationToken));

        /// <summary>Gets multiple roles visible in the authenticated app and tenant scope.</summary>
        [HttpPost("many")]
        [ProducesResponseType<IReadOnlyList<RoleDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMany(
            RoleIdsDto input,
            CancellationToken cancellationToken) =>
            Ok(await roles.GetManyAsync(input.Ids, cancellationToken));

        /// <summary>Updates a role's name and status without changing its catalog links.</summary>
        [HttpPut("{roleId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Update(
            Guid roleId,
            UpdateRoleDto input,
            CancellationToken cancellationToken)
        {
            await roles.UpdateAsync(roleId, input, cancellationToken);
            return NoContent();
        }

        /// <summary>Atomically updates up to 1000 roles' names and statuses.</summary>
        [HttpPut("bulk")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateMany(
            IReadOnlyCollection<UpdateRoleItemDto> inputs,
            CancellationToken cancellationToken)
        {
            await roles.UpdateManyAsync(inputs, cancellationToken);
            return NoContent();
        }

        /// <summary>Soft-deletes one role.</summary>
        [HttpDelete("{roleId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(Guid roleId, CancellationToken cancellationToken)
        {
            await roles.DeleteAsync(roleId, cancellationToken);
            return NoContent();
        }

        /// <summary>Atomically soft-deletes up to 1000 roles.</summary>
        [HttpPost("bulk-delete")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteMany(
            RoleIdsDto input,
            CancellationToken cancellationToken)
        {
            await roles.DeleteManyAsync(input.Ids, cancellationToken);
            return NoContent();
        }

        /// <summary>Assigns an existing role to a tenant member.</summary>
        [HttpPost("{roleId:guid}/assignments/{userId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Assign(
            Guid roleId,
            Guid userId,
            CancellationToken cancellationToken)
        {
            await roles.AssignAsync(userId, roleId, cancellationToken);
            return NoContent();
        }
    }
}
