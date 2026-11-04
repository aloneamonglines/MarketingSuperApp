using MKS.Shared.Application.Contracts.Common.DTOs;

namespace MKS.Identity.Application.Contracts.Authorization.DTOs
{
    /// <summary>
    /// Requests a page of administrator assignments in the selected scope.
    /// Sorting accepts Id, UserId, IsActive, CreatedAt, or ModifiedAt with optional asc/desc;
    /// Filter, when supplied, must be an exact user identifier.
    /// </summary>
    public sealed class AdminAssignmentPagedRequestDto : PagedAndSortedResultRequestDto
    {
        /// <summary>Optionally limits results to active or inactive assignments.</summary>
        public bool? IsActive { get; set; }
    }
}
