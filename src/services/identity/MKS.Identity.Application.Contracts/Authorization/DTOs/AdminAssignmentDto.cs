namespace MKS.Identity.Application.Contracts.Authorization.DTOs
{
    /// <summary>Describes a scoped administrator assignment.</summary>
    public sealed record AdminAssignmentDto(
        Guid Id,
        Guid UserId,
        bool IsActive,
        DateTimeOffset CreatedAt,
        DateTimeOffset? ModifiedAt);
}
