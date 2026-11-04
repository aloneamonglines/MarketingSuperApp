namespace MKS.Identity.Application.Contracts.Authorization.DTOs
{
    /// <summary>Contains the editable properties and identifier of one role.</summary>
    public sealed record UpdateRoleItemDto(Guid Id, string Name, int MasterStatus);
}
