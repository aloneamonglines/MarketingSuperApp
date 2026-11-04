using MKS.Shared.Application.Contracts.Common.Constants;
using MKS.Shared.Application.Contracts.Common.Interfaces;

namespace MKS.Shared.Application.Contracts.Common.Extensions
{
    /// <summary>
    /// Provides bounded page operations for application queries.
    /// </summary>
    public static class PagedQueryExtensions
    {
        /// <summary>Applies validated, bounded offset paging to a query.</summary>
        public static IQueryable<T> PageBy<T>(
            this IQueryable<T> query,
            IPagedAndSortedResultRequest request)
        {
            ArgumentNullException.ThrowIfNull(query);
            ArgumentNullException.ThrowIfNull(request);

            var skipCount = Math.Max(0, request.SkipCount);
            var maxResultCount = Math.Clamp(
                request.MaxResultCount,
                1,
                PagingConstants.MaxMaxResultCount);
            return query.Skip(skipCount).Take(maxResultCount);
        }
    }
}
