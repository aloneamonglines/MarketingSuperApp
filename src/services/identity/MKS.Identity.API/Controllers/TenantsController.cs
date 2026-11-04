using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MKS.Identity.Application.Contracts.MultiTenancy.DTOs;
using MKS.Identity.Application.Contracts.MultiTenancy.Interfaces;
using MKS.Shared.Application.Contracts.Common.DTOs;

namespace MKS.Identity.API.Controllers
{
    /// <summary>
    /// Exposes application-scoped tenant administration endpoints.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/identity/tenants")]
    public sealed class TenantsController(ITenantApplicationService tenants) : ControllerBase
    {
        /// <summary>Creates a tenant and its initial tenant administrator.</summary>
        [HttpPost]
        [ProducesResponseType<Guid>(StatusCodes.Status201Created)]
        public async Task<IActionResult> Create(
            [FromBody]
            CreateTenantDto input,
            CancellationToken cancellationToken)
        {
            var tenantId = await tenants.CreateAsync(input, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = tenantId }, new { id = tenantId });
        }

        /// <summary>Atomically creates tenants with their initial administrator memberships.</summary>
        [HttpPost("bulk")]
        [ProducesResponseType<IReadOnlyList<Guid>>(StatusCodes.Status201Created)]
        public async Task<IActionResult> CreateMany(
            [FromBody] IReadOnlyCollection<CreateTenantDto> inputs,
            CancellationToken cancellationToken) =>
            StatusCode(StatusCodes.Status201Created, await tenants.CreateManyAsync(inputs, cancellationToken));

        /// <summary>Gets a tenant in the authenticated application.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType<TenantDto>(StatusCodes.Status200OK)]
        public async Task<ActionResult<TenantDto>> Get(Guid id, CancellationToken cancellationToken) =>
            Ok(await tenants.GetAsync(id, cancellationToken));

        /// <summary>Gets multiple tenants in the authenticated application.</summary>
        [HttpPost("bulk-read")]
        [ProducesResponseType<IReadOnlyList<TenantDto>>(StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<TenantDto>>> GetMany(
            [FromBody] IReadOnlyCollection<Guid> ids,
            CancellationToken cancellationToken) =>
            Ok(await tenants.GetManyAsync(ids, cancellationToken));

        /// <summary>Gets a filtered, sorted page of tenants in the authenticated application.</summary>
        [HttpGet]
        [ProducesResponseType<PagedResultDto<TenantDto>>(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResultDto<TenantDto>>> GetPaged(
            [FromQuery] PagedAndSortedResultRequestDto request,
            CancellationToken cancellationToken) =>
            Ok(await tenants.GetPagedAsync(request, cancellationToken));

        /// <summary>Updates a tenant in the authenticated application.</summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType<TenantDto>(StatusCodes.Status200OK)]
        public async Task<ActionResult<TenantDto>> Update(
            Guid id,
            [FromBody] UpdateTenantDto input,
            CancellationToken cancellationToken) =>
            Ok(await tenants.UpdateAsync(id, input, cancellationToken));

        /// <summary>Updates multiple tenants atomically.</summary>
        [HttpPut("bulk")]
        [ProducesResponseType<IReadOnlyList<TenantDto>>(StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<TenantDto>>> UpdateMany(
            [FromBody] IReadOnlyCollection<UpdateTenantItemDto> inputs,
            CancellationToken cancellationToken) =>
            Ok(await tenants.UpdateManyAsync(inputs, cancellationToken));

        /// <summary>Soft-deletes a tenant.</summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await tenants.DeleteAsync(id, cancellationToken);
            return NoContent();
        }

        /// <summary>Soft-deletes multiple tenants atomically.</summary>
        [HttpPost("bulk-delete")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteMany(
            [FromBody] IReadOnlyCollection<Guid> ids,
            CancellationToken cancellationToken)
        {
            await tenants.DeleteManyAsync(ids, cancellationToken);
            return NoContent();
        }
    }
}
