using MKS.Shared.Application.Contracts.Apps.DTOs;
using MKS.Shared.Application.Contracts.Apps.Interfaces;
using MKS.Shared.Application.Contracts.Common.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MKS.Identity.API.Controllers
{
    /// <summary>
    /// Manages applications in the Identity platform.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/identity/apps")]
    public sealed class AppsController(IAppService apps) : ControllerBase
    {
        /// <summary>Creates an application and assigns its creator as the first AppAdmin.</summary>
        [HttpPost]
        [ProducesResponseType<AppDto>(StatusCodes.Status201Created)]
        public async Task<IActionResult> Create(
            [FromBody] CreateAppDto input,
            CancellationToken cancellationToken)
        {
            var app = await apps.CreateAsync(input, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = app.Id }, app);
        }

        /// <summary>Gets an application in the authenticated application context.</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType<AppDto>(StatusCodes.Status200OK)]
        public async Task<ActionResult<AppDto>> Get(Guid id, CancellationToken cancellationToken) =>
            Ok(await apps.GetAsync(id, cancellationToken));

        /// <summary>Updates an application in the authenticated application context.</summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType<AppDto>(StatusCodes.Status200OK)]
        public async Task<ActionResult<AppDto>> Update(
            Guid id,
            [FromBody] UpdateAppDto input,
            CancellationToken cancellationToken) =>
            Ok(await apps.UpdateAsync(id, input, cancellationToken));

        /// <summary>Gets multiple applications visible to the authenticated caller.</summary>
        [HttpPost("bulk-read")]
        [ProducesResponseType<IReadOnlyList<AppDto>>(StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<AppDto>>> GetMany(
            [FromBody] IReadOnlyCollection<Guid> ids,
            CancellationToken cancellationToken) =>
            Ok(await apps.GetManyAsync(ids, cancellationToken));

        /// <summary>Gets a filtered, sorted page of applications visible to the caller.</summary>
        [HttpGet]
        [ProducesResponseType<PagedResultDto<AppDto>>(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResultDto<AppDto>>> GetPaged(
            [FromQuery] PagedAndSortedResultRequestDto request,
            CancellationToken cancellationToken) =>
            Ok(await apps.GetPagedAsync(request, cancellationToken));

        /// <summary>Creates applications and their initial administrator assignments atomically.</summary>
        [HttpPost("bulk")]
        [ProducesResponseType<IReadOnlyList<AppDto>>(StatusCodes.Status201Created)]
        public async Task<ActionResult<IReadOnlyList<AppDto>>> CreateMany(
            [FromBody] IReadOnlyCollection<CreateAppDto> inputs,
            CancellationToken cancellationToken) =>
            StatusCode(
                StatusCodes.Status201Created,
                await apps.CreateManyAsync(inputs, cancellationToken));

        /// <summary>Updates multiple applications atomically.</summary>
        [HttpPut("bulk")]
        [ProducesResponseType<IReadOnlyList<AppDto>>(StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<AppDto>>> UpdateMany(
            [FromBody] IReadOnlyCollection<UpdateAppItemDto> inputs,
            CancellationToken cancellationToken) =>
            Ok(await apps.UpdateManyAsync(inputs, cancellationToken));

        /// <summary>Soft-deletes an application.</summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await apps.DeleteAsync(id, cancellationToken);
            return NoContent();
        }

        /// <summary>Soft-deletes multiple applications atomically.</summary>
        [HttpPost("bulk-delete")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteMany(
            [FromBody] IReadOnlyCollection<Guid> ids,
            CancellationToken cancellationToken)
        {
            await apps.DeleteManyAsync(ids, cancellationToken);
            return NoContent();
        }
    }
}
