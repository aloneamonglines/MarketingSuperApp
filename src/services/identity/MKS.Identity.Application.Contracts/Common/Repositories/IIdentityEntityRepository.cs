using System.Linq.Expressions;
using MKS.Shared.Application.Contracts.Common.DTOs;
using MKS.Shared.Application.Contracts.Common.Interfaces;
using MKS.Shared.Domain.Common;

namespace MKS.Identity.Application.Contracts.Common.Repositories
{
    /// <summary>
    /// Provides persistence primitives for Identity aggregate services. Authorization,
    /// validation, audit metadata, and aggregate invariants remain the caller's responsibility.
    /// </summary>
    public interface IIdentityEntityRepository<TEntity>
        where TEntity : FullAuditEntity
    {
        Task<TEntity?> GetAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<TEntity>> GetManyAsync(
            IReadOnlyCollection<Guid> ids,
            CancellationToken cancellationToken = default);
        Task<PagedResultDto<TEntity>> GetPagedAsync(
            IPagedAndSortedResultRequest request,
            Expression<Func<TEntity, bool>>? filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>>? orderBy = null,
            CancellationToken cancellationToken = default);
        Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
        /// <summary>Persists only the named properties of an existing entity.</summary>
        Task UpdatePropertiesAsync(
            TEntity entity,
            IReadOnlyCollection<string> propertyNames,
            CancellationToken cancellationToken = default);
        Task SoftDeleteAsync(
            Guid id,
            Guid? deleterId,
            DateTimeOffset deletedAt,
            CancellationToken cancellationToken = default);
        Task BulkInsertAsync(
            IReadOnlyCollection<TEntity> entities,
            CancellationToken cancellationToken = default);
        Task BulkUpdateAsync(
            IReadOnlyCollection<TEntity> entities,
            CancellationToken cancellationToken = default);
        /// <summary>Bulk-persists only the named properties of existing entities.</summary>
        Task BulkUpdatePropertiesAsync(
            IReadOnlyCollection<TEntity> entities,
            IReadOnlyCollection<string> propertyNames,
            CancellationToken cancellationToken = default);
        Task BulkSoftDeleteAsync(
            IReadOnlyCollection<Guid> ids,
            Guid? deleterId,
            DateTimeOffset deletedAt,
            CancellationToken cancellationToken = default);
    }
}
