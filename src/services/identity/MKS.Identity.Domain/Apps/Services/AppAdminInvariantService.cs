using MKS.Identity.Domain.Shared.Constants;
using MKS.Identity.Domain.Apps.Repositories;
using MKS.Shared.Domain.Shared.Common;

namespace MKS.Identity.Domain.Apps.Services
{
    /// <summary>
    /// Prevents removal of the final active administrator from an application.
    /// </summary>
    public sealed class AppAdminInvariantService(IAppAdminAssignmentRepository repository)
    {
        /// <summary>Runs a removal under an application lock after validating the remaining count.</summary>
        public Task RemoveAsync(
            Guid appId,
            Guid userId,
            Func<CancellationToken, Task> authorize,
            Func<CancellationToken, Task> removal,
            CancellationToken cancellationToken = default)
        {
            return repository.ExecuteForAppAsync(appId, async token =>
            {
                await authorize(token);
                var assignment = await repository.FindAsync(appId, userId, token);
                if (assignment is null || !assignment.IsActive)
                {
                    return;
                }

                if (await repository.CountActiveAsync(appId, token) <= 1)
                {
                    throw new DomainException(IdentityDomainErrorCodes.LastAdministrator);
                }

                await removal(token);
            }, cancellationToken);
        }
    }
}
