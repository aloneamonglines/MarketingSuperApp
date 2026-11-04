using MKS.Shared.Domain.Common;

namespace MKS.Shared.Domain.Apps.Repositories
{
    public interface IAppRepository<TApp> where TApp : BaseEntity
    {
        Task<TApp?> GetAsync(Guid id, CancellationToken cancellationToken = default);
        Task<bool> CodeExistsAsync(string normalizedCode, CancellationToken cancellationToken = default);
        Task<bool> AnyCodeExistsAsync(
            IReadOnlyCollection<string> normalizedCodes,
            CancellationToken cancellationToken = default);
        Task AddAsync(TApp app, CancellationToken cancellationToken = default);
        Task UpdateAsync(TApp app, CancellationToken cancellationToken = default);
    }
}