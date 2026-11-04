namespace MKS.Identity.Application.Contracts.Authorization.DTOs
{
    /// <summary>Describes a role without exposing its function or permission links.</summary>
    public sealed record RoleDto(
        Guid Id,
        Guid AppId,
        Guid? TenantId,
        string Name,
        string Code,
        int MasterStatus);
}
