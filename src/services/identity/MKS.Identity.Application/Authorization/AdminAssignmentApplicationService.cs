using MKS.Identity.Domain.Shared.Constants;
using MKS.Identity.Application.Contracts.Authorization.DTOs;
using MKS.Identity.Application.Contracts.Authorization.Interfaces;
using MKS.Identity.Application.Contracts.Common.Repositories;
using MKS.Identity.Application.Contracts.Common.Interfaces;
using MKS.Identity.Domain.Apps.Entities;
using MKS.Identity.Domain.Apps.Repositories;
using MKS.Identity.Domain.Apps.Services;
using MKS.Identity.Domain.Authorization.Repositories;
using MKS.Identity.Domain.Authorization.Services;
using MKS.Identity.Domain.MultiTenancy.Entities;
using MKS.Identity.Domain.MultiTenancy.Repositories;
using MKS.Identity.Domain.MultiTenancy.Services;
using MKS.Identity.Domain.Shared.Enums;
using MKS.Identity.Domain.Users.Entities;
using MKS.Identity.Domain.Users.Repositories;
using MKS.Shared.Application.Contracts.Common.Interfaces;
using MKS.Shared.Application.Contracts.Common.Constants;
using MKS.Shared.Application.Contracts.Common.DTOs;
using MKS.Shared.Domain.Apps.Repositories;
using MKS.Shared.Domain.MultiTenancy.Repositories;
using MKS.Shared.Domain.Shared.Common;
using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Identity.Application.Authorization
{
    /// <summary>
    /// Assigns scoped administrators while preserving active-member and final-admin rules.
    /// </summary>
    public sealed class AdminAssignmentApplicationService(
        ICurrentContext currentContext,
        IAppAdminAssignmentRepository appAdminRepository,
        ITenantAdminAssignmentRepository tenantAdminRepository,
        IAuthorizationRepository authorizationRepository,
        IUserRepository userRepository,
        ITenantMembershipRepository membershipRepository,
        ITenantRepository<TenantEntity> tenantRepository,
        AppAdminInvariantService appAdminInvariant,
        TenantAdminInvariantService tenantAdminInvariant,
        PermissionEvaluator permissionEvaluator,
        ITenantScopeAccessor tenantScope,
        ISecurityAuditLogWriter securityAuditLogWriter,
        TimeProvider timeProvider,
        IIdentityEntityRepository<AppAdminAssignmentEntity> appAdminEntities,
        IIdentityEntityRepository<TenantAdminAssignmentEntity> tenantAdminEntities) : IAdminAssignmentApplicationService
    {
        /// <inheritdoc />
        public async Task<AdminAssignmentDto> GetAppAdminAsync(
            Guid appId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var assignments = await GetAppAdminsAsync(appId, [userId], cancellationToken);
            return assignments[0];
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<AdminAssignmentDto>> GetAppAdminsAsync(
            Guid appId,
            IReadOnlyCollection<Guid> userIds,
            CancellationToken cancellationToken = default)
        {
            ValidateIds(userIds);
            var actorId = RequireAppAdminContext(appId);
            await EnsureAppAdminAsync(actorId, appId, cancellationToken);
            var assignments = await FindAppAdminsAsync(appId, userIds, cancellationToken);
            if (assignments.Count != userIds.Count)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }

            var byUserId = assignments.ToDictionary(assignment => assignment.UserId);
            return userIds.Select(id => ToDto(byUserId[id])).ToArray();
        }

        /// <inheritdoc />
        public async Task<PagedResultDto<AdminAssignmentDto>> GetAppAdminsPagedAsync(
            Guid appId,
            AdminAssignmentPagedRequestDto request,
            CancellationToken cancellationToken = default)
        {
            ValidatePageRequest(request, out var sortField, out var descending, out var filterUserId);
            var actorId = RequireAppAdminContext(appId);
            await EnsureAppAdminAsync(actorId, appId, cancellationToken);
            var page = await appAdminEntities.GetPagedAsync(
                request,
                assignment => assignment.AppId == appId &&
                    (request.IsActive == null || assignment.IsActive == request.IsActive) &&
                    (filterUserId == null || assignment.UserId == filterUserId),
                query => ApplySorting(query, sortField, descending),
                cancellationToken);
            return new PagedResultDto<AdminAssignmentDto>(
                page.TotalCount,
                page.Items.Select(ToDto).ToArray());
        }

        /// <inheritdoc />
        public async Task AddAppAdminsAsync(
            Guid appId,
            IReadOnlyCollection<Guid> userIds,
            CancellationToken cancellationToken = default)
        {
            ValidateIds(userIds);
            var actorId = RequireAppAdminContext(appId);
            await appAdminRepository.ExecuteForAppAsync(appId, async token =>
            {
                await EnsureAppAdminAsync(actorId, appId, token);
                var assignments = (await FindAppAdminsAsync(appId, userIds, token))
                    .ToDictionary(assignment => assignment.UserId);
                foreach (var userId in userIds)
                {
                    var target = await userRepository.GetAsync(userId, token);
                    if (target is null || target.MasterStatus != CMasterStatus.Active)
                    {
                        throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
                    }

                    assignments.TryGetValue(userId, out var assignment);
                    if (assignment?.IsActive == true)
                    {
                        continue;
                    }

                    if (assignment is null)
                    {
                        await appAdminRepository.AddAsync(new AppAdminAssignmentEntity
                        {
                            AppId = appId,
                            UserId = userId,
                            IsActive = true
                        }, token);
                    }
                    else
                    {
                        assignment.IsActive = true;
                        await appAdminRepository.UpdateAsync(assignment, token);
                    }

                    await securityAuditLogWriter.AppendAsync(
                        actorId,
                        currentContext.ImpersonatorId,
                        null,
                        IdentityConstants.AuditActions.AppAdminAdded,
                        nameof(AppAdminAssignmentEntity),
                        userId.ToString(),
                        $"AppId={appId}",
                        timeProvider.GetUtcNow(),
                        token);
                }
            }, cancellationToken);
        }

        /// <inheritdoc />
        public async Task RemoveAppAdminsAsync(
            Guid appId,
            IReadOnlyCollection<Guid> userIds,
            CancellationToken cancellationToken = default)
        {
            ValidateIds(userIds);
            var actorId = RequireAppAdminContext(appId);
            await appAdminRepository.ExecuteForAppAsync(appId, async token =>
            {
                await EnsureAppAdminAsync(actorId, appId, token);
                var assignments = await FindAppAdminsAsync(appId, userIds, token);
                var activeAssignments = assignments.Where(assignment => assignment.IsActive).ToArray();
                if (activeAssignments.Length == 0)
                {
                    return;
                }

                if (await appAdminRepository.CountActiveAsync(appId, token) <= activeAssignments.Length)
                {
                    throw new DomainException(IdentityDomainErrorCodes.LastAdministrator);
                }

                foreach (var assignment in activeAssignments)
                {
                    assignment.IsActive = false;
                    await appAdminRepository.UpdateAsync(assignment, token);
                    await securityAuditLogWriter.AppendAsync(
                        actorId,
                        currentContext.ImpersonatorId,
                        null,
                        IdentityConstants.AuditActions.AppAdminRemoved,
                        nameof(AppAdminAssignmentEntity),
                        assignment.UserId.ToString(),
                        $"AppId={appId}",
                        timeProvider.GetUtcNow(),
                        token);
                }
            }, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<AdminAssignmentDto> GetTenantAdminAsync(
            Guid tenantId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var assignments = await GetTenantAdminsAsync(tenantId, [userId], cancellationToken);
            return assignments[0];
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<AdminAssignmentDto>> GetTenantAdminsAsync(
            Guid tenantId,
            IReadOnlyCollection<Guid> userIds,
            CancellationToken cancellationToken = default)
        {
            ValidateIds(userIds);
            var tenant = await RequireTenantContextAsync(tenantId, cancellationToken);
            using var scope = tenantScope.Enter(tenantId);
            await RequireTenantAdminPermissionAsync(tenant.AppId, tenantId, cancellationToken);
            var assignments = await FindTenantAdminsAsync(tenantId, userIds, cancellationToken);
            if (assignments.Count != userIds.Count)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }

            var byUserId = assignments.ToDictionary(assignment => assignment.UserId);
            return userIds.Select(id => ToDto(byUserId[id])).ToArray();
        }

        /// <inheritdoc />
        public async Task<PagedResultDto<AdminAssignmentDto>> GetTenantAdminsPagedAsync(
            Guid tenantId,
            AdminAssignmentPagedRequestDto request,
            CancellationToken cancellationToken = default)
        {
            ValidatePageRequest(request, out var sortField, out var descending, out var filterUserId);
            var tenant = await RequireTenantContextAsync(tenantId, cancellationToken);
            using var scope = tenantScope.Enter(tenantId);
            await RequireTenantAdminPermissionAsync(tenant.AppId, tenantId, cancellationToken);
            var page = await tenantAdminEntities.GetPagedAsync(
                request,
                assignment => assignment.TenantId == tenantId &&
                    (request.IsActive == null || assignment.IsActive == request.IsActive) &&
                    (filterUserId == null || assignment.UserId == filterUserId),
                query => ApplySorting(query, sortField, descending),
                cancellationToken);
            return new PagedResultDto<AdminAssignmentDto>(
                page.TotalCount,
                page.Items.Select(ToDto).ToArray());
        }

        /// <inheritdoc />
        public async Task AddTenantAdminsAsync(
            Guid tenantId,
            IReadOnlyCollection<Guid> userIds,
            CancellationToken cancellationToken = default)
        {
            ValidateIds(userIds);
            var tenant = await RequireTenantContextAsync(tenantId, cancellationToken);
            using var scope = tenantScope.Enter(tenantId);
            await tenantAdminRepository.ExecuteForTenantAsync(tenantId, async token =>
            {
                _ = await RequireTenantAdminPermissionAsync(tenant.AppId, tenantId, token);
                var assignments = (await FindTenantAdminsAsync(tenantId, userIds, token))
                    .ToDictionary(assignment => assignment.UserId);
                foreach (var userId in userIds)
                {
                    var target = await userRepository.GetAsync(userId, token);
                    var membership = await membershipRepository.FindAsync(userId, tenantId, token);
                    if (target is null ||
                        target.MasterStatus != CMasterStatus.Active ||
                        membership?.Status != CMembershipStatus.Active)
                    {
                        throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
                    }

                    assignments.TryGetValue(userId, out var assignment);
                    if (assignment?.IsActive == true)
                    {
                        continue;
                    }

                    if (assignment is null)
                    {
                        await tenantAdminRepository.AddAsync(new TenantAdminAssignmentEntity
                        {
                            TenantId = tenantId,
                            UserId = userId,
                            IsActive = true
                        }, token);
                    }
                    else
                    {
                        assignment.IsActive = true;
                        await tenantAdminRepository.UpdateAsync(assignment, token);
                    }
                }
            }, cancellationToken);
        }

        /// <inheritdoc />
        public async Task RemoveTenantAdminsAsync(
            Guid tenantId,
            IReadOnlyCollection<Guid> userIds,
            CancellationToken cancellationToken = default)
        {
            ValidateIds(userIds);
            var tenant = await RequireTenantContextAsync(tenantId, cancellationToken);
            using var scope = tenantScope.Enter(tenantId);
            await tenantAdminRepository.ExecuteForTenantAsync(tenantId, async token =>
            {
                _ = await RequireTenantAdminPermissionAsync(tenant.AppId, tenantId, token);
                var assignments = await FindTenantAdminsAsync(tenantId, userIds, token);
                var activeAssignments = assignments.Where(assignment => assignment.IsActive).ToArray();
                if (activeAssignments.Length == 0)
                {
                    return;
                }

                if (await tenantAdminRepository.CountActiveAsync(tenantId, token) <= activeAssignments.Length)
                {
                    throw new DomainException(IdentityDomainErrorCodes.LastAdministrator);
                }

                foreach (var assignment in activeAssignments)
                {
                    assignment.IsActive = false;
                    await tenantAdminRepository.UpdateAsync(assignment, token);
                }
            }, cancellationToken);
        }

        /// <inheritdoc />
        public async Task AddAppAdminAsync(
            Guid appId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var actorId = RequireAppAdminContext(appId);
            await appAdminRepository.ExecuteForAppAsync(appId, async token =>
            {
                var target = await userRepository.GetAsync(userId, token);
                if (target is null || target.MasterStatus != CMasterStatus.Active)
                {
                    throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
                }

                var assignment = await appAdminRepository.FindAsync(appId, userId, token);
                if (assignment?.IsActive == true)
                {
                    return;
                }

                if (assignment is null)
                {
                    await appAdminRepository.AddAsync(new AppAdminAssignmentEntity
                    {
                        AppId = appId,
                        UserId = userId,
                        IsActive = true
                    }, token);
                }
                else
                {
                    assignment.IsActive = true;
                    await appAdminRepository.UpdateAsync(assignment, token);
                }

                await securityAuditLogWriter.AppendAsync(
                    actorId,
                    currentContext.ImpersonatorId,
                    null,
                    IdentityConstants.AuditActions.AppAdminAdded,
                    nameof(AppAdminAssignmentEntity),
                    userId.ToString(),
                    $"AppId={appId}",
                    timeProvider.GetUtcNow(),
                    token);
            }, cancellationToken);
        }

        /// <inheritdoc />
        public async Task RemoveAppAdminAsync(
            Guid appId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var actorId = RequireAppAdminContext(appId);
            await appAdminInvariant.RemoveAsync(
                appId,
                userId,
                token => EnsureAppAdminAsync(actorId, appId, token),
                async token =>
                {
                    var assignment = await appAdminRepository.FindAsync(appId, userId, token)
                        ?? throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
                    assignment.IsActive = false;
                    await appAdminRepository.UpdateAsync(assignment, token);
                    await securityAuditLogWriter.AppendAsync(
                        actorId,
                        currentContext.ImpersonatorId,
                        null,
                        IdentityConstants.AuditActions.AppAdminRemoved,
                        nameof(AppAdminAssignmentEntity),
                        userId.ToString(),
                        $"AppId={appId}",
                        timeProvider.GetUtcNow(),
                        token);
                },
                cancellationToken);
        }

        /// <inheritdoc />
        public async Task AddTenantAdminAsync(
            Guid tenantId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var tenant = await RequireTenantContextAsync(tenantId, cancellationToken);
            using var scope = tenantScope.Enter(tenantId);
            await tenantAdminRepository.ExecuteForTenantAsync(tenantId, async token =>
            {
                _ = await RequireTenantAdminPermissionAsync(tenant.AppId, tenantId, token);
                var target = await userRepository.GetAsync(userId, token);
                var membership = await membershipRepository.FindAsync(userId, tenantId, token);
                if (target is null
                    || target.MasterStatus != CMasterStatus.Active
                    || membership?.Status != CMembershipStatus.Active)
                {
                    throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
                }

                var assignment = await tenantAdminRepository.FindAsync(tenantId, userId, token);
                if (assignment?.IsActive == true)
                {
                    return;
                }

                if (assignment is null)
                {
                    await tenantAdminRepository.AddAsync(new TenantAdminAssignmentEntity
                    {
                        TenantId = tenantId,
                        UserId = userId,
                        IsActive = true
                    }, token);
                }
                else
                {
                    assignment.IsActive = true;
                    await tenantAdminRepository.UpdateAsync(assignment, token);
                }
            }, cancellationToken);
        }

        /// <inheritdoc />
        public async Task RemoveTenantAdminAsync(
            Guid tenantId,
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var tenant = await RequireTenantContextAsync(tenantId, cancellationToken);
            using var scope = tenantScope.Enter(tenantId);
            await tenantAdminInvariant.RemoveAsync(
                tenantId,
                userId,
                async token =>
                {
                    _ = await RequireTenantAdminPermissionAsync(tenant.AppId, tenantId, token);
                },
                async token =>
                {
                    var assignment = await tenantAdminRepository.FindAsync(tenantId, userId, token)
                        ?? throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
                    assignment.IsActive = false;
                    await tenantAdminRepository.UpdateAsync(assignment, token);
                },
                cancellationToken);
        }

        private async Task<IReadOnlyList<AppAdminAssignmentEntity>> FindAppAdminsAsync(
            Guid appId,
            IReadOnlyCollection<Guid> userIds,
            CancellationToken cancellationToken)
        {
            var request = new PagedAndSortedResultRequestDto { MaxResultCount = userIds.Count };
            var page = await appAdminEntities.GetPagedAsync(
                request,
                assignment => assignment.AppId == appId && userIds.Contains(assignment.UserId),
                query => query.OrderBy(assignment => assignment.UserId),
                cancellationToken);
            return page.Items;
        }

        private async Task<IReadOnlyList<TenantAdminAssignmentEntity>> FindTenantAdminsAsync(
            Guid tenantId,
            IReadOnlyCollection<Guid> userIds,
            CancellationToken cancellationToken)
        {
            var request = new PagedAndSortedResultRequestDto { MaxResultCount = userIds.Count };
            var page = await tenantAdminEntities.GetPagedAsync(
                request,
                assignment => assignment.TenantId == tenantId && userIds.Contains(assignment.UserId),
                query => query.OrderBy(assignment => assignment.UserId),
                cancellationToken);
            return page.Items;
        }

        private static AdminAssignmentDto ToDto(AppAdminAssignmentEntity assignment) =>
            new(assignment.Id, assignment.UserId, assignment.IsActive, assignment.CreatedAt, assignment.ModifiedAt);

        private static AdminAssignmentDto ToDto(TenantAdminAssignmentEntity assignment) =>
            new(assignment.Id, assignment.UserId, assignment.IsActive, assignment.CreatedAt, assignment.ModifiedAt);

        private static void ValidateIds(IReadOnlyCollection<Guid> userIds)
        {
            if (userIds is null ||
                userIds.Count is 0 or > PagingConstants.MaxMaxResultCount ||
                userIds.Any(userId => userId == Guid.Empty) ||
                userIds.Distinct().Count() != userIds.Count)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }
        }

        private static void ValidatePageRequest(
            AdminAssignmentPagedRequestDto request,
            out string sortField,
            out bool descending,
            out Guid? filterUserId)
        {
            if (request is null ||
                request.SkipCount < 0 ||
                request.MaxResultCount is < 1 or > PagingConstants.MaxMaxResultCount)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }

            var sortParts = (request.Sorting ?? "CreatedAt desc")
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (sortParts.Length is < 1 or > 2)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }

            sortField = sortParts[0].ToLowerInvariant();
            if (sortField is not ("id" or "userid" or "isactive" or "createdat" or "modifiedat"))
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }

            descending = sortParts.Length == 2 && sortParts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);
            if (sortParts.Length == 2 &&
                !descending &&
                !sortParts[1].Equals("asc", StringComparison.OrdinalIgnoreCase))
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }

            filterUserId = null;
            if (!string.IsNullOrWhiteSpace(request.Filter))
            {
                if (!Guid.TryParse(request.Filter, out var parsedUserId) || parsedUserId == Guid.Empty)
                {
                    throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
                }

                filterUserId = parsedUserId;
            }
        }

        private static IOrderedQueryable<AppAdminAssignmentEntity> ApplySorting(
            IQueryable<AppAdminAssignmentEntity> query,
            string field,
            bool descending)
        {
            return field switch
            {
                "id" => descending ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id),
                "userid" => descending ? query.OrderByDescending(x => x.UserId).ThenBy(x => x.Id) :
                    query.OrderBy(x => x.UserId).ThenBy(x => x.Id),
                "isactive" => descending ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.Id) :
                    query.OrderBy(x => x.IsActive).ThenBy(x => x.Id),
                "createdat" => descending ? query.OrderByDescending(x => x.CreatedAt).ThenBy(x => x.Id) :
                    query.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id),
                _ => descending ? query.OrderByDescending(x => x.ModifiedAt).ThenBy(x => x.Id) :
                    query.OrderBy(x => x.ModifiedAt).ThenBy(x => x.Id)
            };
        }

        private static IOrderedQueryable<TenantAdminAssignmentEntity> ApplySorting(
            IQueryable<TenantAdminAssignmentEntity> query,
            string field,
            bool descending)
        {
            return field switch
            {
                "id" => descending ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id),
                "userid" => descending ? query.OrderByDescending(x => x.UserId).ThenBy(x => x.Id) :
                    query.OrderBy(x => x.UserId).ThenBy(x => x.Id),
                "isactive" => descending ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.Id) :
                    query.OrderBy(x => x.IsActive).ThenBy(x => x.Id),
                "createdat" => descending ? query.OrderByDescending(x => x.CreatedAt).ThenBy(x => x.Id) :
                    query.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id),
                _ => descending ? query.OrderByDescending(x => x.ModifiedAt).ThenBy(x => x.Id) :
                    query.OrderBy(x => x.ModifiedAt).ThenBy(x => x.Id)
            };
        }

        private Guid RequireAppAdminContext(Guid appId)
        {
            if (appId == Guid.Empty ||
                currentContext.AppId != appId ||
                currentContext.UserId is not { } actorId)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }

            return actorId;
        }

        private async Task EnsureAppAdminAsync(
            Guid userId,
            Guid appId,
            CancellationToken cancellationToken)
        {
            if (!await authorizationRepository.IsAppAdminAsync(userId, appId, cancellationToken))
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }
        }

        private async Task<TenantEntity> RequireTenantContextAsync(
            Guid tenantId,
            CancellationToken cancellationToken)
        {
            var tenant = await tenantRepository.GetAsync(tenantId, cancellationToken)
                ?? throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            if (tenant.MasterStatus != CMasterStatus.Active ||
                currentContext.AppId != tenant.AppId ||
                currentContext.TenantId is { } activeTenantId && activeTenantId != tenantId)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }

            return tenant;
        }

        private async Task<Guid> RequireTenantAdminPermissionAsync(
            Guid appId,
            Guid tenantId,
            CancellationToken cancellationToken)
        {
            if (currentContext.UserId is not { } actorId ||
                !await permissionEvaluator.HasPermissionAsync(
                    actorId,
                    appId,
                    tenantId,
                    IdentityConstants.Permissions.TenantAdminAdminsManage,
                    cancellationToken))
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }

            return actorId;
        }
    }
}
