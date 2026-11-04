using System.ComponentModel.DataAnnotations;
using MKS.Shared.Application.Contracts.Common.Constants;
using MKS.Shared.Application.Contracts.Common.Interfaces;

namespace MKS.Shared.Application.Contracts.Common.DTOs
{
    /// <summary>
    /// Provides standard paging, sorting, and free-text filtering parameters.
    /// Sorting and filter semantics are defined by each application query; sorting
    /// must be mapped through an explicit field whitelist.
    /// </summary>
    public class PagedAndSortedResultRequestDto : IPagedAndSortedResultRequest
    {
        /// <inheritdoc />
        [Range(0, int.MaxValue)]
        public int SkipCount { get; set; }

        /// <inheritdoc />
        [Range(1, PagingConstants.MaxMaxResultCount)]
        public int MaxResultCount { get; set; } = PagingConstants.DefaultMaxResultCount;

        /// <inheritdoc />
        [StringLength(PagingConstants.MaxSortingLength)]
        public string? Sorting { get; set; }

        /// <inheritdoc />
        [StringLength(PagingConstants.MaxFilterLength)]
        public string? Filter { get; set; }
    }
}
