namespace MKS.Identity.Application.Contracts.Users.DTOs
{
    /// <summary>
    /// Returns an enumeration-safe result for a user creation or membership request.
    /// </summary>
    public sealed record UserCreatedDto(bool Accepted);
}
