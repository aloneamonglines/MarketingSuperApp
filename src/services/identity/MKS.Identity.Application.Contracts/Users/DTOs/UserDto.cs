using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Identity.Application.Contracts.Users.DTOs
{
    /// <summary>
    /// A tenant-authorized user profile. Credentials and authentication secrets are never included.
    /// </summary>
    public sealed record UserDto(
        Guid Id,
        string? FirstName,
        string? LastName,
        string? Thumbnail,
        CMasterStatus MasterStatus);
}
