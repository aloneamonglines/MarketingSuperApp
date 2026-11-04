using MKS.Shared.Domain.Common;

namespace MKS.Shared.Domain.MultiTenancy.Repositories
{
    public interface ITenantRepository<TTenant> where TTenant : BaseEntity
    {
        Task<TTenant?> GetAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> CodeExistsAsync(Guid appId, string normalizedCode, CancellationToken cancellationToken = default);
        Task<bool> AnyCodeExistsAsync(
            Guid appId,
            IReadOnlyCollection<string> normalizedCodes,
            CancellationToken cancellationToken = default);
        Task AddAsync(TTenant tenant, CancellationToken cancellationToken = default);
    }
}