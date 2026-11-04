using MKS.Shared.Application.Contracts.Apps.DTOs;
using MKS.Shared.Application.Contracts.Common.DTOs;
using MKS.Shared.Application.Contracts.Common.Interfaces;

namespace MKS.Shared.Application.Contracts.Apps.Interfaces
{
    public interface IAppService : IApplicationService
    {
        Task<AppDto> CreateAsync(CreateAppDto input, CancellationToken cancellationToken = default);
        Task<AppDto> GetAsync(Guid id, CancellationToken cancellationToken = default);
        Task<AppDto> UpdateAsync(Guid id, UpdateAppDto input, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AppDto>> GetManyAsync(
            IReadOnlyCollection<Guid> ids,
            CancellationToken cancellationToken = default);
        Task<PagedResultDto<AppDto>> GetPagedAsync(
            PagedAndSortedResultRequestDto request,
            CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AppDto>> CreateManyAsync(
            IReadOnlyCollection<CreateAppDto> inputs,
            CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AppDto>> UpdateManyAsync(
            IReadOnlyCollection<UpdateAppItemDto> inputs,
            CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task DeleteManyAsync(
            IReadOnlyCollection<Guid> ids,
            CancellationToken cancellationToken = default);
    }
}