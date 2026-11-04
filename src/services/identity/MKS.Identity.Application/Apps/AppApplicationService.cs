using System.Linq.Expressions;
using MKS.Identity.Domain.Shared.Constants;
using MKS.Identity.Application.Contracts.Common.Interfaces;
using MKS.Identity.Application.Contracts.Authorization.Interfaces;
using MKS.Identity.Domain.Apps.Repositories;
using MKS.Identity.Domain.Authorization.Repositories;
using MKS.Identity.Domain.Authorization.Services;
using MKS.Identity.Domain.Apps.Entities;
using MKS.Identity.Domain.MultiTenancy.Entities;
using MKS.Shared.Domain.Shared.Enums;
using MKS.Shared.Application.Contracts.Apps.DTOs;
using MKS.Shared.Application.Contracts.Apps.Interfaces;
using MKS.Shared.Application.Contracts.Common.DTOs;
using MKS.Shared.Application.Contracts.Common.Constants;
using MKS.Shared.Application.Contracts.Common.Interfaces;
using MKS.Identity.Application.Contracts.Common.Repositories;
using MKS.Shared.Domain.Apps.Repositories;
using MKS.Shared.Domain.Shared.Common;

namespace MKS.Identity.Application.Apps
{
    /// <summary>
    /// Implements platform application catalog workflows.
    /// </summary>
    public sealed class AppApplicationService(
        IAppRepository<AppEntity> appRepository,
        IAuthorizationRepository authorizationRepository,
        IAppAdminAssignmentRepository appAdminRepository,
        IIdentityUnitOfWork unitOfWork,
        ICurrentContext currentContext,
        PermissionEvaluator permissionEvaluator,
        IAppPermissionCatalogSynchronizer catalogSynchronizer,
        IIdentityEntityRepository<AppEntity> entityRepository,
        IIdentityEntityRepository<AppAdminAssignmentEntity> appAdminEntityRepository,
        TimeProvider timeProvider) : IAppService
    {
        /// <inheritdoc />
        public async Task<AppDto> CreateAsync(
            CreateAppDto input,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            if (currentContext.UserId is not { } userId ||
                !await permissionEvaluator.HasPermissionAsync(
                    userId,
                    Guid.Empty,
                    null,
                    IdentityConstants.Permissions.PlatformAppsCreate,
                    cancellationToken))
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }

            var app = AppEntity.Create(input.Name, input.Code, input.Description, input.LogoUrl);
            if (await appRepository.CodeExistsAsync(app.Code, cancellationToken))
            {
                throw new DomainException(IdentityDomainErrorCodes.IdentifierAlreadyExists);
            }

            await unitOfWork.ExecuteAsync(async token =>
            {
                await appRepository.AddAsync(app, token);
                await appAdminRepository.AddAsync(new AppAdminAssignmentEntity
                {
                    AppId = app.Id,
                    UserId = userId,
                    IsActive = true
                }, token);
            }, cancellationToken);
            await catalogSynchronizer.SynchronizeAppAsync(app.Code, cancellationToken);

            return ToDto(app);
        }

        /// <inheritdoc />
        public async Task<AppDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var app = await appRepository.GetAsync(id, cancellationToken)
                ?? throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            await EnsureAppAdminAsync(id, cancellationToken);
            return ToDto(app);
        }

        /// <inheritdoc />
        public async Task<AppDto> UpdateAsync(
            Guid id,
            UpdateAppDto input,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            await EnsureAppAdminAsync(id, cancellationToken);
            var app = await appRepository.GetAsync(id, cancellationToken)
                ?? throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            if (string.IsNullOrWhiteSpace(input.Name) ||
                !Enum.IsDefined((CMasterStatus)input.MasterStatus) ||
                input.MasterStatus == (int)CMasterStatus.None)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }

            app.Name = input.Name.Trim();
            app.Description = input.Description?.Trim();
            app.LogoUrl = input.LogoUrl?.Trim();
            app.MasterStatus = (CMasterStatus)input.MasterStatus;
            app.ModifierId = currentContext.UserId;
            app.ModifiedAt = timeProvider.GetUtcNow();
            await unitOfWork.ExecuteAsync(
                token => appRepository.UpdateAsync(app, token),
                cancellationToken);
            return ToDto(app);
        }

        public async Task<IReadOnlyList<AppDto>> GetManyAsync(
            IReadOnlyCollection<Guid> ids,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(ids);
            if (ids.Count > PagingConstants.MaxMaxResultCount)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }
            if (ids.Any(id => id == Guid.Empty) || ids.Distinct().Count() != ids.Count)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }

            var apps = await entityRepository.GetManyAsync(ids, cancellationToken);
            if (apps.Count != ids.Count)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }

            if (currentContext.AppId is { } appId)
            {
                await EnsureAppAdminAsync(appId, cancellationToken);
                if (apps.Any(app => app.Id != appId))
                {
                    throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
                }
            }
            else if (currentContext.UserId is not { } userId ||
                     !await permissionEvaluator.HasPermissionAsync(
                         userId,
                         Guid.Empty,
                         null,
                         IdentityConstants.Permissions.PlatformAppsCreate,
                         cancellationToken))
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }

            return apps.Select(ToDto).ToArray();
        }

        public async Task<PagedResultDto<AppDto>> GetPagedAsync(
            PagedAndSortedResultRequestDto request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            var userId = currentContext.UserId
                ?? throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            Guid? scopedAppId = null;
            if (currentContext.AppId is { } appId)
            {
                await EnsureAppAdminAsync(appId, cancellationToken);
                scopedAppId = appId;
            }
            else if (!await permissionEvaluator.HasPermissionAsync(
                         userId,
                         Guid.Empty,
                         null,
                         IdentityConstants.Permissions.PlatformAppsCreate,
                         cancellationToken))
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }

            Func<IQueryable<AppEntity>, IOrderedQueryable<AppEntity>> orderBy =
                request.Sorting?.Trim().ToLowerInvariant() switch
                {
                    "name" => query => query.OrderBy(app => app.Name).ThenBy(app => app.Id),
                    "name desc" => query => query.OrderByDescending(app => app.Name).ThenBy(app => app.Id),
                    "code" => query => query.OrderBy(app => app.Code).ThenBy(app => app.Id),
                    "code desc" => query => query.OrderByDescending(app => app.Code).ThenBy(app => app.Id),
                    _ => query => query.OrderBy(app => app.Id)
                };
            var search = request.Filter?.Trim();
            Expression<Func<AppEntity, bool>>? filter = search is { Length: > 0 }
                ? app => (scopedAppId == null || app.Id == scopedAppId) &&
                         (app.Name.Contains(search) || app.Code.Contains(search))
                : scopedAppId is { } selectedAppId
                    ? app => app.Id == selectedAppId
                    : null;

            var page = await entityRepository.GetPagedAsync(request, filter, orderBy, cancellationToken);
            return new PagedResultDto<AppDto>(page.TotalCount, page.Items.Select(ToDto).ToArray());
        }

        public async Task<IReadOnlyList<AppDto>> CreateManyAsync(
            IReadOnlyCollection<CreateAppDto> inputs,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(inputs);
            if (inputs.Count == 0)
            {
                return [];
            }
            if (inputs.Count > PagingConstants.MaxMaxResultCount)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }

            var userId = currentContext.UserId;
            if (userId is null ||
                !await permissionEvaluator.HasPermissionAsync(
                    userId.Value,
                    Guid.Empty,
                    null,
                    IdentityConstants.Permissions.PlatformAppsCreate,
                    cancellationToken))
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }

            var apps = inputs.Select(input =>
            {
                ArgumentNullException.ThrowIfNull(input);
                return AppEntity.Create(input.Name, input.Code, input.Description, input.LogoUrl);
            }).ToArray();
            if (apps.Select(app => app.Code).Distinct(StringComparer.Ordinal).Count() != apps.Length)
            {
                throw new DomainException(IdentityDomainErrorCodes.IdentifierAlreadyExists);
            }

            if (await appRepository.AnyCodeExistsAsync(
                    apps.Select(app => app.Code).ToArray(),
                    cancellationToken))
            {
                throw new DomainException(IdentityDomainErrorCodes.IdentifierAlreadyExists);
            }

            var createdAt = timeProvider.GetUtcNow();
            foreach (var app in apps)
            {
                app.CreatorId = userId;
                app.CreatedAt = createdAt;
            }

            var assignments = apps.Select(app => new AppAdminAssignmentEntity
            {
                AppId = app.Id,
                UserId = userId.Value,
                IsActive = true,
                CreatorId = userId,
                CreatedAt = createdAt
            }).ToArray();

            await unitOfWork.ExecuteAsync(async token =>
            {
                await entityRepository.BulkInsertAsync(apps, token);
                await appAdminEntityRepository.BulkInsertAsync(assignments, token);
            }, cancellationToken);

            foreach (var app in apps)
            {
                await catalogSynchronizer.SynchronizeAppAsync(app.Code, cancellationToken);
            }

            return apps.Select(ToDto).ToArray();
        }

        public async Task<IReadOnlyList<AppDto>> UpdateManyAsync(
            IReadOnlyCollection<UpdateAppItemDto> inputs,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(inputs);
            if (inputs.Count == 0)
            {
                return [];
            }
            if (inputs.Count > PagingConstants.MaxMaxResultCount)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }

            if (inputs.Any(input => input is null))
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }

            var ids = inputs.Select(input => input.Id).ToArray();
            if (ids.Any(id => id == Guid.Empty) || ids.Distinct().Count() != ids.Length)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }

            var apps = (await entityRepository.GetManyAsync(ids, cancellationToken)).ToDictionary(app => app.Id);
            if (apps.Count != ids.Length)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }

            var modifiedAt = timeProvider.GetUtcNow();
            foreach (var input in inputs)
            {
                await EnsureAppAdminAsync(input.Id, cancellationToken);
                if (string.IsNullOrWhiteSpace(input.Name) ||
                    !Enum.IsDefined((CMasterStatus)input.MasterStatus) ||
                    input.MasterStatus == (int)CMasterStatus.None)
                {
                    throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
                }

                var app = apps[input.Id];
                app.Name = input.Name.Trim();
                app.Description = input.Description?.Trim();
                app.LogoUrl = input.LogoUrl?.Trim();
                app.MasterStatus = (CMasterStatus)input.MasterStatus;
                app.ModifierId = currentContext.UserId;
                app.ModifiedAt = modifiedAt;
            }

            await unitOfWork.ExecuteAsync(
                token => entityRepository.BulkUpdateAsync(apps.Values.ToArray(), token),
                cancellationToken);
            return ids.Select(id => ToDto(apps[id])).ToArray();
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
            await DeleteManyAsync([id], cancellationToken);

        public async Task DeleteManyAsync(
            IReadOnlyCollection<Guid> ids,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(ids);
            if (ids.Count == 0)
            {
                return;
            }
            if (ids.Count > PagingConstants.MaxMaxResultCount)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }

            if (ids.Any(id => id == Guid.Empty) || ids.Distinct().Count() != ids.Count)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }

            var distinctIds = ids.Distinct().ToArray();
            var apps = await entityRepository.GetManyAsync(distinctIds, cancellationToken);
            if (apps.Count != distinctIds.Length)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }

            foreach (var app in apps)
            {
                await EnsureAppAdminAsync(app.Id, cancellationToken);
            }

            await unitOfWork.ExecuteAsync(
                token => entityRepository.BulkSoftDeleteAsync(
                    distinctIds,
                    currentContext.UserId,
                    timeProvider.GetUtcNow(),
                    token),
                cancellationToken);
        }

        private async Task EnsureAppAdminAsync(Guid appId, CancellationToken cancellationToken)
        {
            if (currentContext.AppId != appId ||
                currentContext.UserId is not { } userId ||
                !await authorizationRepository.IsAppAdminAsync(userId, appId, cancellationToken))
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }
        }

        private static AppDto ToDto(AppEntity app) => new()
        {
            Id = app.Id,
            Name = app.Name,
            Code = app.Code,
            Description = app.Description,
            LogoUrl = app.LogoUrl,
            MasterStatus = (int)app.MasterStatus
        };
    }
}
