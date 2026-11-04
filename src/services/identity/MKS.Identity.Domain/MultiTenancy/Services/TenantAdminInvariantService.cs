using MKS.Identity.Domain.Shared.Constants;
using MKS.Identity.Domain.MultiTenancy.Repositories;
using MKS.Shared.Domain.Shared.Common;

namespace MKS.Identity.Domain.MultiTenancy.Services
{
    /// <summary>
    /// Prevents removal of the final active administrator from a tenant.
    /// </summary>
    public sealed class TenantAdminInvariantService(ITenantAdminAssignmentRepository repository)
    {
        /// <summary>Runs a removal under a tenant lock after validating the remaining count.</summary>
        public Task RemoveAsync(
            Guid tenantId,
            Guid userId,
            Func<CancellationToken, Task> authorize,
            Func<CancellationToken, Task> removal,
            CancellationToken cancellationToken = default)
        {
            return repository.ExecuteForTenantAsync(tenantId, async token =>
            {
                await authorize(token);
                var assignment = await repository.FindAsync(tenantId, userId, token);
                if (assignment is null || !assignment.IsActive)
                {
                    return;
                }

                if (await repository.CountActiveAsync(tenantId, token) <= 1)
                {
                    throw new DomainException(IdentityDomainErrorCodes.LastAdministrator);
                }

                await removal(token);
            }, cancellationToken);
        }
    }
}
