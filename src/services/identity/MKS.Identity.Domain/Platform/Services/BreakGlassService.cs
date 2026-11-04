using MKS.Identity.Domain.Shared.Constants;
using MKS.Identity.Domain.Platform.Entities;
using MKS.Identity.Domain.Platform.Repositories;
using MKS.Shared.Domain.Shared.Common;

namespace MKS.Identity.Domain.Platform.Services
{
    /// <summary>
    /// Applies bounds and lifecycle checks to break-glass access.
    /// </summary>
    public sealed class BreakGlassService(
        IBreakGlassGrantRepository repository,
        TimeProvider timeProvider,
        TimeSpan maximumDuration)
    {
        /// <summary>Creates a read-only grant whose duration cannot exceed the configured maximum.</summary>
        public async Task<BreakGlassGrantEntity> GrantAsync(
            Guid userId,
            Guid tenantId,
            string reason,
            TimeSpan duration,
            CancellationToken cancellationToken = default)
        {
            var grant = BreakGlassGrantEntity.Create(
                userId,
                tenantId,
                reason,
                timeProvider.GetUtcNow(),
                duration,
                maximumDuration);
            await repository.AddAsync(grant, cancellationToken);
            return grant;
        }

        /// <summary>Revokes a grant if it exists.</summary>
        public async Task RevokeAsync(Guid grantId, CancellationToken cancellationToken = default)
        {
            var grant = await repository.GetAsync(grantId, cancellationToken)
                ?? throw new DomainException(IdentityDomainErrorCodes.BreakGlassExpired);
            grant.Revoke(timeProvider.GetUtcNow());
            await repository.UpdateAsync(grant, cancellationToken);
        }

        /// <summary>Ensures a token grant remains active before accessing tenant data.</summary>
        public async Task<BreakGlassGrantEntity> RequireActiveAsync(
            Guid grantId,
            Guid tenantId,
            CancellationToken cancellationToken = default)
        {
            var grant = await repository.GetAsync(grantId, cancellationToken);
            if (grant is null || grant.TenantId != tenantId || !grant.IsActiveAt(timeProvider.GetUtcNow()))
            {
                throw new DomainException(IdentityDomainErrorCodes.BreakGlassExpired);
            }

            return grant;
        }
    }
}
