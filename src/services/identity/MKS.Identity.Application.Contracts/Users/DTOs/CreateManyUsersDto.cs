namespace MKS.Identity.Application.Contracts.Users.DTOs
{
    /// <summary>Contains tenant-scoped user provisioning requests.</summary>
    public sealed record CreateManyUsersDto(IReadOnlyCollection<CreateUserDto> Users);
}
