namespace MKS.Identity.Application.Contracts.Users.DTOs
{
    /// <summary>Contains user IDs whose membership in the active tenant should be deactivated.</summary>
    public sealed record DeactivateUsersDto(IReadOnlyList<Guid> UserIds);
}
