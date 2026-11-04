namespace MKS.Identity.Application.Contracts.Authorization.DTOs
{
    /// <summary>Contains user identifiers for a many-item administrator operation.</summary>
    public sealed record AdminAssignmentIdsDto(IReadOnlyCollection<Guid> UserIds);
}
