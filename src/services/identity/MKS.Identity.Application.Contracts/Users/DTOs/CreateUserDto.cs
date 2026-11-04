namespace MKS.Identity.Application.Contracts.Users.DTOs
{
    /// <summary>
    /// Describes a user's identifiers and initial tenant membership.
    /// </summary>
    public sealed record CreateUserDto(
        Guid TenantId,
        string? Email,
        string? PhoneNumber,
        string? Username,
        string? FirstName,
        string? LastName);
}
