namespace MKS.Shared.Application.Contracts.Common.Constants
{
    /// <summary>
    /// Defines shared limits for API paging and query parameters.
    /// </summary>
    public static class PagingConstants
    {
        /// <summary>The default number of records returned by a page request.</summary>
        public const int DefaultMaxResultCount = 10;

        /// <summary>The maximum number of records returned by a page request.</summary>
        public const int MaxMaxResultCount = 1000;

        /// <summary>The maximum length of a sorting expression.</summary>
        public const int MaxSortingLength = 256;

        /// <summary>The maximum length of a filter expression.</summary>
        public const int MaxFilterLength = 256;
    }
}
