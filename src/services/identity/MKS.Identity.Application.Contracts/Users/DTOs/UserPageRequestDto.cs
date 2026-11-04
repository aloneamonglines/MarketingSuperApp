using MKS.Shared.Application.Contracts.Common.Interfaces;
using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Identity.Application.Contracts.Users.DTOs
{
    /// <summary>Describes a bounded, tenant-scoped user profile search.</summary>
    public sealed record UserPageRequestDto(
        int SkipCount = 0,
        int MaxResultCount = 10,
        string? Sorting = null,
        string? Filter = null,
        CMasterStatus? Status = null) : IPagedAndSortedResultRequest;
}
