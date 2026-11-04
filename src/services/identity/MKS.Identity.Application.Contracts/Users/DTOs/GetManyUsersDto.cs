namespace MKS.Identity.Application.Contracts.Users.DTOs
{
    /// <summary>Contains IDs for a tenant-scoped profile lookup of up to 1000 users.</summary>
    public sealed record GetManyUsersDto(IReadOnlyList<Guid> UserIds);
}
