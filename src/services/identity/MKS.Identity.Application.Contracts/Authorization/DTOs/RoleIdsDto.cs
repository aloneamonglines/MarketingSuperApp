namespace MKS.Identity.Application.Contracts.Authorization.DTOs
{
    /// <summary>Contains role identifiers for a many-item operation.</summary>
    public sealed record RoleIdsDto(IReadOnlyCollection<Guid> Ids);
}
