using MKS.Identity.Application.Contracts.Platform.DTOs;
using MKS.Shared.Application.Contracts.Common.Interfaces;

namespace MKS.Identity.Application.Contracts.Platform.Interfaces
{
    /// <summary>
    /// Provides authorized break-glass grant lifecycle operations.
    /// </summary>
    public interface IBreakGlassApplicationService : IApplicationService
    {
        /// <summary>Creates a read-only grant for an authorized platform administrator.</summary>
        Task<Guid> CreateAsync(
            CreateBreakGlassGrantDto input,
            CancellationToken cancellationToken = default);

        /// <summary>Gets one grant's non-sensitive lifecycle metadata.</summary>
        Task<BreakGlassGrantDto> GetAsync(Guid grantId, CancellationToken cancellationToken = default);

        /// <summary>Gets metadata for a bounded set of grants.</summary>
        Task<IReadOnlyList<BreakGlassGrantDto>> GetManyAsync(
            BreakGlassGrantIdsDto input,
            CancellationToken cancellationToken = default);

        /// <summary>Gets a tenant-filtered, allowlisted page of grants.</summary>
        Task<MKS.Shared.Application.Contracts.Common.DTOs.PagedResultDto<BreakGlassGrantDto>> GetPagedAsync(
            BreakGlassGrantPageRequestDto input,
            CancellationToken cancellationToken = default);

        /// <summary>Revokes a grant and records the operation.</summary>
        Task RevokeAsync(Guid grantId, CancellationToken cancellationToken = default);

        /// <summary>Atomically revokes a bounded set of grants and records each operation.</summary>
        Task RevokeManyAsync(
            BreakGlassGrantIdsDto input,
            CancellationToken cancellationToken = default);
    }
}
