using System.ComponentModel.DataAnnotations;
using MKS.Shared.Application.Contracts.Common.Constants;

namespace MKS.Identity.Application.Contracts.Platform.DTOs
{
    /// <summary>Defines a bounded, tenant-filtered break-glass grant page request.</summary>
    public sealed class BreakGlassGrantPageRequestDto
    {
        /// <summary>Gets or sets the number of records to skip.</summary>
        [Range(0, int.MaxValue)]
        public int SkipCount { get; set; }

        /// <summary>Gets or sets the maximum number of records to return.</summary>
        [Range(1, PagingConstants.MaxMaxResultCount)]
        public int MaxResultCount { get; set; } = PagingConstants.DefaultMaxResultCount;

        /// <summary>Gets or sets the allowlisted sort field and optional direction.</summary>
        [StringLength(PagingConstants.MaxSortingLength)]
        public string? Sorting { get; set; }

        /// <summary>Gets or sets an optional target tenant filter.</summary>
        public Guid? TenantId { get; set; }
    }
}
