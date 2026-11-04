namespace MKS.Identity.Application.Contracts.Users.DTOs
{
    /// <summary>Reports the number of tenant-scoped users changed by a bulk operation.</summary>
    public sealed record UserBulkResultDto(int AffectedCount);
}
