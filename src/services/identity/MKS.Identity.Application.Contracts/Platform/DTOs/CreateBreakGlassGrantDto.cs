namespace MKS.Identity.Application.Contracts.Platform.DTOs
{
    /// <summary>
    /// Describes a bounded request for temporary tenant access.
    /// </summary>
    public sealed record CreateBreakGlassGrantDto(Guid TenantId, string Reason, int DurationMinutes);
}
