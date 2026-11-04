using MKS.Identity.Application.Contracts.MultiTenancy.DTOs;
using MKS.Shared.Application.Contracts.Common.DTOs;
using MKS.Shared.Application.Contracts.Common.Interfaces;

namespace MKS.Identity.Application.Contracts.MultiTenancy.Interfaces
{
    /// <summary>
    /// Provides application-scoped tenant workflows.
    /// </summary>
    public interface ITenantApplicationService : IApplicationService
    {
        /// <summary>Creates a tenant and its initial administrator atomically.</summary>
        Task<Guid> CreateAsync(CreateTenantDto input, CancellationToken cancellationToken = default);
        /// <summary>Atomically creates tenants with their initial administrator memberships.</summary>
        Task<IReadOnlyList<Guid>> CreateManyAsync(
            IReadOnlyCollection<CreateTenantDto> inputs,
            CancellationToken cancellationToken = default);

        /// <summary>Gets a tenant in the authenticated application.</summary>
        Task<TenantDto> GetAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>Gets multiple tenants in the authenticated application.</summary>
        Task<IReadOnlyList<TenantDto>> GetManyAsync(
            IReadOnlyCollection<Guid> ids,
            CancellationToken cancellationToken = default);

        /// <summary>Gets a filtered and sorted page of tenants in the authenticated application.</summary>
        Task<PagedResultDto<TenantDto>> GetPagedAsync(
            PagedAndSortedResultRequestDto request,
            CancellationToken cancellationToken = default);

        /// <summary>Updates a tenant in the authenticated application.</summary>
        Task<TenantDto> UpdateAsync(
            Guid id,
            UpdateTenantDto input,
            CancellationToken cancellationToken = default);

        /// <summary>Updates multiple tenants atomically.</summary>
        Task<IReadOnlyList<TenantDto>> UpdateManyAsync(
            IReadOnlyCollection<UpdateTenantItemDto> inputs,
            CancellationToken cancellationToken = default);

        /// <summary>Soft-deletes a tenant in the authenticated application.</summary>
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>Soft-deletes multiple tenants atomically.</summary>
        Task DeleteManyAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);
    }
}
