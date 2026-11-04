namespace MKS.Shared.Application.Contracts.Common.DTOs
{
    /// <summary>
    /// Represents a page of results and the total number of matching records.
    /// </summary>
    /// <typeparam name="T">The type of items in the page.</typeparam>
    public sealed class PagedResultDto<T>
    {
        /// <summary>Creates a page result.</summary>
        public PagedResultDto(long totalCount, IReadOnlyList<T> items)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(totalCount);
            ArgumentNullException.ThrowIfNull(items);
            TotalCount = totalCount;
            Items = items;
        }

        /// <summary>Gets the total number of matching records before paging.</summary>
        public long TotalCount { get; }

        /// <summary>Gets the requested page of records.</summary>
        public IReadOnlyList<T> Items { get; }
    }
}
