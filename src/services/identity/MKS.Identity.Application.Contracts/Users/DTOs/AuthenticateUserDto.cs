namespace MKS.Identity.Application.Contracts.Users.DTOs
{
    /// <summary>
    /// Describes a credential sign-in request and the requested application context.
    /// </summary>
    public sealed record AuthenticateUserDto(
        string Identifier,
        string Password,
        Guid? AppId,
        Guid? TenantId,
        string? DeviceName);
}
