namespace MKS.Shared.Application.Contracts.Common.Interfaces
{
    /// <summary>
    /// Describes a bounded page request with optional sorting and filtering.
    /// </summary>
    public interface IPagedAndSortedResultRequest
    {
        /// <summary>Gets the number of records to skip.</summary>
        int SkipCount { get; }

        /// <summary>Gets the maximum number of records to return.</summary>
        int MaxResultCount { get; }

        /// <summary>Gets an optional sort expression understood by the application service.</summary>
        string? Sorting { get; }

        /// <summary>Gets an optional application-defined filter expression.</summary>
        string? Filter { get; }
    }
}
