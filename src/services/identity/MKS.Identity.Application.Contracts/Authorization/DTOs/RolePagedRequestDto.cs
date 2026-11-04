using MKS.Shared.Application.Contracts.Common.DTOs;

namespace MKS.Identity.Application.Contracts.Authorization.DTOs
{
    /// <summary>Requests a page of roles in the authenticated app and tenant scope.</summary>
    public sealed class RolePagedRequestDto : PagedAndSortedResultRequestDto
    {
    }
}
