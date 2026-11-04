using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MKS.Identity.Application.Contracts.Users.DTOs;
using MKS.Identity.Application.Contracts.Users.Interfaces;

namespace MKS.Identity.API.Controllers
{
    /// <summary>
    /// Exposes tenant-scoped user profile, membership, provisioning, and password-management endpoints.
    /// </summary>
    [ApiController]
    [Route("api/identity/users")]
    public sealed class IdentityUsersController(IUserApplicationService users) : ControllerBase
    {
        /// <summary>Creates a user or adds an existing user to a tenant.</summary>
        [Authorize]
        [HttpPost]
        [ProducesResponseType<UserCreatedDto>(StatusCodes.Status202Accepted)]
        public async Task<IActionResult> Create(
            CreateUserDto input,
            CancellationToken cancellationToken)
        {
            var result = await users.CreateAsync(input, cancellationToken);
            return Accepted(result);
        }

        /// <summary>Atomically provisions users or memberships in the current tenant.</summary>
        [Authorize]
        [HttpPost("bulk")]
        [ProducesResponseType<UserBulkResultDto>(StatusCodes.Status202Accepted)]
        public async Task<IActionResult> CreateMany(
            CreateManyUsersDto input,
            CancellationToken cancellationToken) =>
            Accepted(await users.CreateManyAsync(input, cancellationToken));

        /// <summary>Gets one active member's safe profile in the current tenant.</summary>
        [Authorize]
        [HttpGet("{userId:guid}")]
        [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(Guid userId, CancellationToken cancellationToken) =>
            Ok(await users.GetAsync(userId, cancellationToken));

        /// <summary>Gets safe profiles for up to 1000 active members in the current tenant.</summary>
        [Authorize]
        [HttpPost("many")]
        [ProducesResponseType<IReadOnlyList<UserDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMany(
            GetManyUsersDto input,
            CancellationToken cancellationToken) =>
            Ok(await users.GetManyAsync(input, cancellationToken));

        /// <summary>Gets a filtered page of active member profiles in the current tenant.</summary>
        [Authorize]
        [HttpGet]
        [ProducesResponseType<MKS.Shared.Application.Contracts.Common.DTOs.PagedResultDto<UserDto>>(
            StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaged(
            [FromQuery] UserPageRequestDto input,
            CancellationToken cancellationToken) =>
            Ok(await users.GetPagedAsync(input, cancellationToken));

        /// <summary>Updates editable profile fields for one active member.</summary>
        [Authorize]
        [HttpPut("{userId:guid}/profile")]
        [ProducesResponseType<UserDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateProfile(
            Guid userId,
            UpdateUserProfileDto input,
            CancellationToken cancellationToken) =>
            Ok(await users.UpdateProfileAsync(userId, input, cancellationToken));

        /// <summary>Atomically updates profile fields for up to 1000 active members.</summary>
        [Authorize]
        [HttpPut("profiles")]
        [ProducesResponseType<UserBulkResultDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateProfiles(
            BulkUpdateUserProfilesDto input,
            CancellationToken cancellationToken) =>
            Ok(await users.UpdateProfilesAsync(input, cancellationToken));

        /// <summary>
        /// Deactivates only this user's membership in the current tenant. The global identity and any
        /// memberships in other tenants are retained and are not changed by this endpoint; the global identity
        /// cannot be deleted safely because this tenant-scoped operation cannot inspect all its memberships.
        /// </summary>
        [Authorize]
        [HttpDelete("{userId:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Deactivate(Guid userId, CancellationToken cancellationToken)
        {
            await users.DeactivateAsync(userId, cancellationToken);
            return NoContent();
        }

        /// <summary>Atomically deactivates up to 1000 memberships in the current tenant only.</summary>
        [Authorize]
        [HttpPost("deactivate")]
        [ProducesResponseType<UserBulkResultDto>(StatusCodes.Status200OK)]
        public async Task<IActionResult> DeactivateMany(
            DeactivateUsersDto input,
            CancellationToken cancellationToken) =>
            Ok(await users.DeactivateManyAsync(input, cancellationToken));

        /// <summary>Changes the authenticated user's password.</summary>
        [Authorize]
        [HttpPost("change-password")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ChangePassword(
            ChangePasswordDto input,
            CancellationToken cancellationToken)
        {
            await users.ChangePasswordAsync(input, cancellationToken);
            return NoContent();
        }
    }
}
