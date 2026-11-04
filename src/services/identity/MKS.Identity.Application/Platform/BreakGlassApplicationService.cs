using MKS.Identity.Domain.Shared.Constants;
using MKS.Identity.Application.Contracts.Common.Interfaces;
using MKS.Identity.Application.Contracts.Platform.DTOs;
using MKS.Identity.Application.Contracts.Platform.Interfaces;
using MKS.Identity.Domain.Authorization.Services;
using MKS.Identity.Domain.Authorization.Repositories;
using MKS.Identity.Domain.Platform.Repositories;
using MKS.Identity.Domain.Platform.Services;
using MKS.Identity.Domain.Shared.Enums;
using MKS.Identity.Domain.MultiTenancy.Entities;
using MKS.Identity.Domain.Platform.Entities;
using MKS.Shared.Application.Contracts.Common.Interfaces;
using MKS.Shared.Application.Contracts.Common.DTOs;
using MKS.Shared.Application.Contracts.Common.Constants;
using MKS.Shared.Domain.MultiTenancy.Repositories;
using MKS.Shared.Domain.Shared.Common;
using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Identity.Application.Platform
{
    /// <summary>
    /// Authorizes and audits platform break-glass grant lifecycle operations.
    /// </summary>
    public sealed class BreakGlassApplicationService(
        ICurrentContext currentContext,
        PermissionEvaluator permissionEvaluator,
        IAuthorizationRepository authorizationRepository,
        ITenantRepository<TenantEntity> tenantRepository,
        IBreakGlassGrantRepository grantRepository,
        BreakGlassService breakGlassService,
        IIdentityUnitOfWork unitOfWork,
        ISecurityAuditLogWriter securityAuditLogWriter,
        TimeProvider timeProvider) : IBreakGlassApplicationService
    {
        /// <inheritdoc />
        public async Task<Guid> CreateAsync(
            CreateBreakGlassGrantDto input,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            var actorId = await RequirePlatformPermissionAsync(
                IdentityConstants.Permissions.PlatformBreakGlassCreate,
                cancellationToken);
            if (input.TenantId == Guid.Empty)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }

            var tenant = await tenantRepository.GetAsync(input.TenantId, cancellationToken)
                ?? throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            if (tenant.MasterStatus != CMasterStatus.Active)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }

            var duration = TimeSpan.FromMinutes(input.DurationMinutes);
            Guid grantId = Guid.Empty;
            await unitOfWork.ExecuteAsync(async token =>
            {
                var grant = await breakGlassService.GrantAsync(
                    actorId,
                    tenant.Id,
                    input.Reason,
                    duration,
                    token);
                grantId = grant.Id;
                await securityAuditLogWriter.AppendAsync(
                    actorId,
                    currentContext.ImpersonatorId,
                    grant.Id,
                    IdentityConstants.AuditActions.BreakGlassCreated,
                    nameof(BreakGlassGrantEntity),
                    grant.Id.ToString(),
                    $"TenantId={tenant.Id}; ExpiresAt={grant.ExpiresAt:O}; ReadOnly=true; Reason={grant.Reason}",
                    timeProvider.GetUtcNow(),
                    token);
            }, cancellationToken);

            return grantId;
        }

        /// <inheritdoc />
        public async Task<BreakGlassGrantDto> GetAsync(
            Guid grantId,
            CancellationToken cancellationToken = default)
        {
            await RequirePlatformPermissionAsync(
                IdentityConstants.Permissions.PlatformBreakGlassRevoke,
                cancellationToken);
            var grant = await grantRepository.GetAsync(grantId, cancellationToken)
                ?? throw new DomainException(IdentityDomainErrorCodes.BreakGlassExpired);
            await EnsureActiveTenantAsync(grant.TenantId, cancellationToken);
            return ToDto(grant);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<BreakGlassGrantDto>> GetManyAsync(
            BreakGlassGrantIdsDto input,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            var ids = ValidateIds(input.Ids);
            await RequirePlatformPermissionAsync(
                IdentityConstants.Permissions.PlatformBreakGlassRevoke,
                cancellationToken);
            if (input.TenantId is { } tenantId)
            {
                await EnsureActiveTenantAsync(tenantId, cancellationToken);
            }

            var grants = await grantRepository.GetManyAsync(ids, input.TenantId, cancellationToken);
            if (grants.Count != ids.Count)
            {
                throw new DomainException(IdentityDomainErrorCodes.BreakGlassExpired);
            }

            await EnsureActiveTenantsAsync(grants, cancellationToken);
            return grants.Select(ToDto).ToArray();
        }

        /// <inheritdoc />
        public async Task<PagedResultDto<BreakGlassGrantDto>> GetPagedAsync(
            BreakGlassGrantPageRequestDto input,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            if (input.SkipCount < 0 ||
                input.MaxResultCount is < 1 or > PagingConstants.MaxMaxResultCount)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }

            await RequirePlatformPermissionAsync(
                IdentityConstants.Permissions.PlatformBreakGlassRevoke,
                cancellationToken);
            if (input.TenantId is { } tenantId)
            {
                await EnsureActiveTenantAsync(tenantId, cancellationToken);
            }

            var (sortField, descending) = ParseSorting(input.Sorting);
            var (items, totalCount) = await grantRepository.GetPagedAsync(
                input.SkipCount,
                input.MaxResultCount,
                input.TenantId,
                sortField,
                descending,
                cancellationToken);
            await EnsureActiveTenantsAsync(items, cancellationToken);
            return new PagedResultDto<BreakGlassGrantDto>(
                totalCount,
                items.Select(ToDto).ToArray());
        }

        /// <inheritdoc />
        public async Task RevokeAsync(Guid grantId, CancellationToken cancellationToken = default)
        {
            var actorId = await RequirePlatformPermissionAsync(
                IdentityConstants.Permissions.PlatformBreakGlassRevoke,
                cancellationToken);
            var grant = await grantRepository.GetAsync(grantId, cancellationToken)
                ?? throw new DomainException(IdentityDomainErrorCodes.BreakGlassExpired);
            await unitOfWork.ExecuteAsync(async token =>
            {
                await breakGlassService.RevokeAsync(grantId, token);
                await securityAuditLogWriter.AppendAsync(
                    actorId,
                    currentContext.ImpersonatorId,
                    grant.Id,
                    IdentityConstants.AuditActions.BreakGlassRevoked,
                    nameof(BreakGlassGrantEntity),
                    grant.Id.ToString(),
                    $"TenantId={grant.TenantId}",
                    timeProvider.GetUtcNow(),
                    token);
            }, cancellationToken);
        }

        /// <inheritdoc />
        public async Task RevokeManyAsync(
            BreakGlassGrantIdsDto input,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            var ids = ValidateIds(input.Ids);
            var actorId = await RequirePlatformPermissionAsync(
                IdentityConstants.Permissions.PlatformBreakGlassRevoke,
                cancellationToken);
            if (input.TenantId is { } tenantId)
            {
                await EnsureActiveTenantAsync(tenantId, cancellationToken);
            }

            await unitOfWork.ExecuteAsync(async token =>
            {
                var grants = await grantRepository.GetManyAsync(ids, input.TenantId, token);
                if (grants.Count != ids.Count)
                {
                    throw new DomainException(IdentityDomainErrorCodes.BreakGlassExpired);
                }

                await EnsureActiveTenantsAsync(grants, token);
                foreach (var grant in grants)
                {
                    await breakGlassService.RevokeAsync(grant.Id, token);
                    await securityAuditLogWriter.AppendAsync(
                        actorId,
                        currentContext.ImpersonatorId,
                        grant.Id,
                        IdentityConstants.AuditActions.BreakGlassRevoked,
                        nameof(BreakGlassGrantEntity),
                        grant.Id.ToString(),
                        $"TenantId={grant.TenantId}",
                        timeProvider.GetUtcNow(),
                        token);
                }
            }, cancellationToken);
        }

        private async Task<Guid> RequirePlatformPermissionAsync(
            string permission,
            CancellationToken cancellationToken)
        {
            if (currentContext.TenantId is not null ||
                currentContext.UserId is not { } actorId)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }

            var platformPermissions = await authorizationRepository.GetPlatformPermissionsAsync(
                actorId,
                cancellationToken);
            var hasPlatformPermission = platformPermissions.Contains(permission) ||
                platformPermissions.Contains(IdentityConstants.Permissions.PlatformSuperAdminWildcard) &&
                permission.StartsWith(IdentityConstants.PermissionPrefixes.Platform, StringComparison.Ordinal);
            if (!hasPlatformPermission ||
                !await permissionEvaluator.HasPermissionAsync(
                    actorId,
                    currentContext.AppId ?? Guid.Empty,
                    null,
                    permission,
                    cancellationToken))
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }

            return actorId;
        }

        private async Task EnsureActiveTenantAsync(Guid tenantId, CancellationToken cancellationToken)
        {
            if (tenantId == Guid.Empty)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }

            var tenant = await tenantRepository.GetAsync(tenantId, cancellationToken);
            if (tenant is null || tenant.MasterStatus != CMasterStatus.Active)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }
        }

        private async Task EnsureActiveTenantsAsync(
            IReadOnlyCollection<BreakGlassGrantEntity> grants,
            CancellationToken cancellationToken)
        {
            foreach (var tenantId in grants.Select(grant => grant.TenantId).Distinct())
            {
                await EnsureActiveTenantAsync(tenantId, cancellationToken);
            }
        }

        private static IReadOnlyList<Guid> ValidateIds(IReadOnlyList<Guid> ids)
        {
            if (ids is null ||
                ids.Count is < 1 or > PagingConstants.MaxMaxResultCount ||
                ids.Any(id => id == Guid.Empty) ||
                ids.Distinct().Count() != ids.Count)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }

            return ids;
        }

        private static (BreakGlassGrantSortField SortField, bool Descending) ParseSorting(string? sorting)
        {
            var values = (sorting ?? "id")
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (values.Length is < 1 or > 2)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }

            var sortField = values[0].ToLowerInvariant() switch
            {
                "id" => BreakGlassGrantSortField.Id,
                "tenantid" => BreakGlassGrantSortField.TenantId,
                "expiresat" => BreakGlassGrantSortField.ExpiresAt,
                "status" => BreakGlassGrantSortField.Status,
                "revokedat" => BreakGlassGrantSortField.RevokedAt,
                _ => throw new DomainException(IdentityDomainErrorCodes.InvalidInput)
            };
            var descending = values.Length == 2 && values[1].Equals("desc", StringComparison.OrdinalIgnoreCase);
            if (values.Length == 2 &&
                !descending &&
                !values[1].Equals("asc", StringComparison.OrdinalIgnoreCase))
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }

            return (sortField, descending);
        }

        private BreakGlassGrantDto ToDto(BreakGlassGrantEntity grant)
        {
            var status = grant.Status == CBreakGlassStatus.Active &&
                         !grant.IsActiveAt(timeProvider.GetUtcNow())
                ? CBreakGlassStatus.Expired
                : grant.Status;
            return new BreakGlassGrantDto(
                grant.Id,
                grant.TenantId,
                grant.ExpiresAt,
                status,
                grant.RevokedAt);
        }
    }
}
