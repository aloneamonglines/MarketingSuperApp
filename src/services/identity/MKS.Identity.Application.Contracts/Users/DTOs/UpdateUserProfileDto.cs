namespace MKS.Identity.Application.Contracts.Users.DTOs
{
    /// <summary>Describes editable profile fields; identifiers and credentials are not mutable here.</summary>
    public sealed record UpdateUserProfileDto(
        string? FirstName,
        string? LastName,
        string? Thumbnail);

    /// <summary>Describes a profile update for one user in a bulk request.</summary>
    public sealed record UpdateUserProfileItemDto(
        Guid UserId,
        string? FirstName,
        string? LastName,
        string? Thumbnail);

    /// <summary>Contains profile updates for no more than 1000 users.</summary>
    public sealed record BulkUpdateUserProfilesDto(
        IReadOnlyList<UpdateUserProfileItemDto> Users);
}
