namespace MKS.Identity.Application.Contracts.Authorization.DTOs
{
    /// <summary>
    /// Describes a role and its application catalog permissions. App and tenant
    /// scope are taken from the authenticated request context.
    /// </summary>
    public sealed record CreateRoleDto(
        string Name,
        string Code,
        IReadOnlyCollection<string> FunctionCodes,
        IReadOnlyCollection<string> PermissionCodes);
}
