using MKS.Identity.Domain.Authorization.Entities;
using MKS.Identity.Domain.MultiTenancy.Entities;

namespace MKS.Identity.Domain.Authorization.Repositories
{
    /// <summary>
    /// Defines queries used to evaluate a user's application and tenant permissions.
    /// </summary>
    public interface IAuthorizationRepository
    {
        /// <summary>Checks whether the user is an active administrator for the application.</summary>
        Task<bool> IsAppAdminAsync(Guid userId, Guid appId, CancellationToken cancellationToken = default);
        /// <summary>Checks whether the user is an active administrator for the tenant.</summary>
        Task<bool> IsTenantAdminAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);
        /// <summary>Gets active permissions assigned through roles in the tenant.</summary>
        Task<IReadOnlySet<string>> GetRolePermissionsAsync(
            Guid userId,
            Guid appId,
            Guid tenantId,
            CancellationToken cancellationToken = default);
        /// <summary>Gets a user's active membership in a tenant.</summary>
        Task<TenantMembershipEntity?> GetMembershipAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default);
        /// <summary>Gets platform permissions for a user.</summary>
        Task<IReadOnlySet<string>> GetPlatformPermissionsAsync(
            Guid userId,
            CancellationToken cancellationToken = default);
    }
}
