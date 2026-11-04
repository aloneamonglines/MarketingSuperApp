using MKS.Identity.Domain.Shared.Enums;

namespace MKS.Identity.Application.Contracts.Platform.DTOs
{
    /// <summary>
    /// Exposes non-sensitive lifecycle metadata for a break-glass grant.
    /// </summary>
    public sealed record BreakGlassGrantDto(
        Guid Id,
        Guid TenantId,
        DateTimeOffset ExpiresAt,
        CBreakGlassStatus Status,
        DateTimeOffset? RevokedAt);
}
