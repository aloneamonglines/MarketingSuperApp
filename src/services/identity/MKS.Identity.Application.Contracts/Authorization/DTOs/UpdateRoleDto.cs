namespace MKS.Identity.Application.Contracts.Authorization.DTOs
{
    /// <summary>Contains the editable properties of a role.</summary>
    public sealed record UpdateRoleDto(string Name, int MasterStatus);
}
