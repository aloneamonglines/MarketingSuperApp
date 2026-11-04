using MKS.Identity.Application.Contracts.Users.DTOs;
using MKS.Shared.Application.Contracts.Common.Interfaces;

namespace MKS.Identity.Application.Contracts.Users.Interfaces
{
    /// <summary>
    /// Provides tenant-scoped user profile and membership workflows.
    /// </summary>
    public interface IUserApplicationService : IApplicationService
    {
        /// <summary>Creates a user or adds an existing user to a tenant without changing credentials.</summary>
        Task<UserCreatedDto> CreateAsync(CreateUserDto input, CancellationToken cancellationToken = default);
        /// <summary>Atomically provisions users or memberships in the current tenant.</summary>
        Task<UserBulkResultDto> CreateManyAsync(
            CreateManyUsersDto input,
            CancellationToken cancellationToken = default);
        /// <summary>Gets one active member's safe profile in the current tenant.</summary>
        Task<UserDto> GetAsync(Guid userId, CancellationToken cancellationToken = default);
        /// <summary>Gets safe profiles for active members in the current tenant.</summary>
        Task<IReadOnlyList<UserDto>> GetManyAsync(
            GetManyUsersDto input,
            CancellationToken cancellationToken = default);
        /// <summary>Gets a filtered page of active member profiles in the current tenant.</summary>
        Task<MKS.Shared.Application.Contracts.Common.DTOs.PagedResultDto<UserDto>> GetPagedAsync(
            UserPageRequestDto input,
            CancellationToken cancellationToken = default);
        /// <summary>Updates editable profile fields for one active member.</summary>
        Task<UserDto> UpdateProfileAsync(
            Guid userId,
            UpdateUserProfileDto input,
            CancellationToken cancellationToken = default);
        /// <summary>Atomically updates profile fields for up to 1000 active members.</summary>
        Task<UserBulkResultDto> UpdateProfilesAsync(
            BulkUpdateUserProfilesDto input,
            CancellationToken cancellationToken = default);
        /// <summary>
        /// Deactivates one membership in the current tenant. The global identity is retained because it may
        /// belong to memberships in other tenants, which this tenant-scoped operation cannot safely inspect or delete.
        /// </summary>
        Task DeactivateAsync(Guid userId, CancellationToken cancellationToken = default);
        /// <summary>
        /// Atomically deactivates up to 1000 memberships in the current tenant; global identities and memberships
        /// outside the current tenant are never deleted or changed.
        /// </summary>
        Task<UserBulkResultDto> DeactivateManyAsync(
            DeactivateUsersDto input,
            CancellationToken cancellationToken = default);
        /// <summary>Changes the authenticated user's password.</summary>
        Task ChangePasswordAsync(ChangePasswordDto input, CancellationToken cancellationToken = default);
    }
}
