namespace MKS.Identity.Application.Contracts.Users.DTOs
{
    /// <summary>
    /// Describes verified account and context information for token issuance.
    /// </summary>
    public sealed record AuthenticatedUserDto(
        Guid UserId,
        Guid SessionId,
        Guid? AppId,
        Guid? TenantId,
        bool MustChangePassword,
        long MembershipVersion,
        Guid? BreakGlassGrantId = null);
}
