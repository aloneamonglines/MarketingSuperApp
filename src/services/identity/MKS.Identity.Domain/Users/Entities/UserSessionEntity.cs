using MKS.Shared.Domain.Common;
using MKS.Shared.Domain.Shared.Attributes;
using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Identity.Domain.Users.Entities
{
    /// <summary>
    /// Represents a revocable sign-in session for one user device.
    /// </summary>
    [EntityScope(CEntityScope.Global)]
    public sealed class UserSessionEntity : FullAuditEntity
    {
        /// <summary>Gets or sets the authenticated user identifier.</summary>
        public Guid UserId { get; set; }
        /// <summary>Gets or sets the device label.</summary>
        public string? DeviceName { get; set; }
        /// <summary>Gets or sets the application context selected at sign-in.</summary>
        public Guid? AppId { get; set; }
        /// <summary>Gets or sets the tenant context selected at sign-in.</summary>
        public Guid? TenantId { get; set; }
        /// <summary>Gets or sets the break-glass grant used to create this session.</summary>
        public Guid? BreakGlassGrantId { get; set; }
        /// <summary>Gets or sets the generation of the only valid refresh token for this session.</summary>
        public long RefreshTokenGeneration { get; set; }
        /// <summary>Gets or sets when the session was last used.</summary>
        public DateTimeOffset LastSeenAt { get; set; }
        /// <summary>Gets or sets when the session was revoked.</summary>
        public DateTimeOffset? RevokedAt { get; set; }

        /// <summary>Revokes this session.</summary>
        public void Revoke(DateTimeOffset now) => RevokedAt ??= now;
    }
}
