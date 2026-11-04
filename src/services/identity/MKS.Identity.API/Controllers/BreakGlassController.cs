using MKS.Identity.Application.Contracts.Platform.DTOs;
using MKS.Identity.Application.Contracts.Platform.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MKS.Shared.Application.Contracts.Common.DTOs;

namespace MKS.Identity.API.Controllers
{
    /// <summary>
    /// Manages temporary, read-only platform access to a tenant.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/identity/break-glass")]
    public sealed class BreakGlassController(IBreakGlassApplicationService breakGlass)
        : ControllerBase
    {
        /// <summary>Creates a read-only, time-bounded tenant access grant.</summary>
        [HttpPost]
        [ProducesResponseType<Guid>(StatusCodes.Status201Created)]
        public async Task<IActionResult> Create(
            [FromBody] CreateBreakGlassGrantDto request,
            CancellationToken cancellationToken)
        {
            var grantId = await breakGlass.CreateAsync(
                request,
                cancellationToken);
            return CreatedAtAction(nameof(Revoke), new { grantId }, new { grantId });
        }

        /// <summary>Gets non-sensitive lifecycle metadata for one grant.</summary>
        [HttpGet("{grantId:guid}")]
        [ProducesResponseType<BreakGlassGrantDto>(StatusCodes.Status200OK)]
        public async Task<ActionResult<BreakGlassGrantDto>> Get(
            Guid grantId,
            CancellationToken cancellationToken) =>
            Ok(await breakGlass.GetAsync(grantId, cancellationToken));

        /// <summary>Gets a tenant-filtered, allowlisted page of grant metadata.</summary>
        [HttpGet]
        [ProducesResponseType<PagedResultDto<BreakGlassGrantDto>>(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResultDto<BreakGlassGrantDto>>> GetPaged(
            [FromQuery] BreakGlassGrantPageRequestDto request,
            CancellationToken cancellationToken) =>
            Ok(await breakGlass.GetPagedAsync(request, cancellationToken));

        /// <summary>Gets non-sensitive metadata for multiple grants.</summary>
        [HttpPost("many")]
        [ProducesResponseType<IReadOnlyList<BreakGlassGrantDto>>(StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<BreakGlassGrantDto>>> GetMany(
            [FromBody] BreakGlassGrantIdsDto request,
            CancellationToken cancellationToken) =>
            Ok(await breakGlass.GetManyAsync(request, cancellationToken));

        /// <summary>Revokes a previously issued break-glass grant.</summary>
        [HttpDelete("{grantId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Revoke(Guid grantId, CancellationToken cancellationToken)
        {
            await breakGlass.RevokeAsync(grantId, cancellationToken);
            return NoContent();
        }

        /// <summary>Atomically revokes multiple grants and audits each revocation.</summary>
        [HttpPost("bulk-revoke")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> RevokeMany(
            [FromBody] BreakGlassGrantIdsDto request,
            CancellationToken cancellationToken)
        {
            await breakGlass.RevokeManyAsync(request, cancellationToken);
            return NoContent();
        }
    }

}
