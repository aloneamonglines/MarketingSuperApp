namespace MKS.Identity.Application.Contracts.MultiTenancy.DTOs
{
    /// <summary>
    /// Describes a tenant and its initial administrator.
    /// </summary>
    public sealed record CreateTenantDto(
        string Name,
        string Code,
        string AdminEmail,
        string? AdminPhoneNumber,
        string? AdminUsername);
}
