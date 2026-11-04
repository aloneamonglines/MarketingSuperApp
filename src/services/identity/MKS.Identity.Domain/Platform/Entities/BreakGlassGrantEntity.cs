using MKS.Identity.Domain.Shared.Constants;
using MKS.Identity.Domain.Shared.Enums;
using MKS.Shared.Domain.Common;
using MKS.Shared.Domain.Shared.Attributes;
using MKS.Shared.Domain.Shared.Common;
using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Identity.Domain.Platform.Entities
{
    /// <summary>
    /// Grants temporary, audited access to one tenant for a platform administrator.
    /// </summary>
    [EntityScope(CEntityScope.Global)]
    public sealed class BreakGlassGrantEntity : BaseEntity
    {
        private BreakGlassGrantEntity()
        {
        }

        /// <summary>Gets or sets the platform administrator who requested access.</summary>
        public Guid UserId { get; private set; }
        /// <summary>Gets or sets the target tenant identifier.</summary>
        public Guid TenantId { get; private set; }
        /// <summary>Gets or sets the justification.</summary>
        public string Reason { get; private set; } = string.Empty;
        /// <summary>Gets or sets when the grant expires.</summary>
        public DateTimeOffset ExpiresAt { get; private set; }
        /// <summary>Gets or sets whether access is read-only.</summary>
        public bool ReadOnly { get; private set; }
        /// <summary>Gets or sets the lifecycle state.</summary>
        public CBreakGlassStatus Status { get; private set; }
        /// <summary>Gets or sets when access was revoked.</summary>
        public DateTimeOffset? RevokedAt { get; private set; }

        /// <summary>Creates a bounded read-only grant with a required reason.</summary>
        public static BreakGlassGrantEntity Create(
            Guid userId,
            Guid tenantId,
            string reason,
            DateTimeOffset now,
            TimeSpan duration,
            TimeSpan maximumDuration)
        {
            if (userId == Guid.Empty || tenantId == Guid.Empty)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }

            if (string.IsNullOrWhiteSpace(reason) || reason.Trim().Length < 20)
            {
                throw new DomainException(IdentityDomainErrorCodes.BreakGlassReason);
            }

            if (duration <= TimeSpan.Zero || maximumDuration <= TimeSpan.Zero || duration > maximumDuration)
            {
                throw new DomainException(IdentityDomainErrorCodes.BreakGlassDuration);
            }

            return new BreakGlassGrantEntity
            {
                UserId = userId,
                TenantId = tenantId,
                Reason = reason.Trim(),
                ExpiresAt = now.Add(duration),
                ReadOnly = true,
                Status = CBreakGlassStatus.Active
            };
        }

        /// <summary>Revokes this grant immediately.</summary>
        public void Revoke(DateTimeOffset now)
        {
            if (Status != CBreakGlassStatus.Active)
            {
                return;
            }

            Status = CBreakGlassStatus.Revoked;
            RevokedAt = now;
        }

        /// <summary>Returns whether the grant is active at the specified time.</summary>
        public bool IsActiveAt(DateTimeOffset now) =>
            Status == CBreakGlassStatus.Active && now < ExpiresAt && RevokedAt is null;
    }
}
