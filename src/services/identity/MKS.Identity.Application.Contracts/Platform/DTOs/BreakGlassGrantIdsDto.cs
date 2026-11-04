namespace MKS.Identity.Application.Contracts.Platform.DTOs
{
    /// <summary>Identifies grants for a bounded read or revoke operation.</summary>
    public sealed record BreakGlassGrantIdsDto(IReadOnlyList<Guid> Ids, Guid? TenantId = null);
}
