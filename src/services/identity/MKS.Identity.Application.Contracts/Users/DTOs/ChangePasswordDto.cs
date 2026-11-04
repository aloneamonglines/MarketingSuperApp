namespace MKS.Identity.Application.Contracts.Users.DTOs
{
    /// <summary>
    /// Describes a password change request.
    /// </summary>
    public sealed record ChangePasswordDto(string CurrentPassword, string NewPassword);
}
