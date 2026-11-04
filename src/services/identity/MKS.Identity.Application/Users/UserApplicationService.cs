using MKS.Identity.Domain.Shared.Constants;
using Microsoft.Extensions.Options;
using MKS.Identity.Application.Contracts.Common.Interfaces;
using MKS.Identity.Application.Contracts.Common.Options;
using MKS.Identity.Application.Contracts.Common.Repositories;
using MKS.Identity.Application.Contracts.Users.DTOs;
using MKS.Identity.Application.Contracts.Users.Interfaces;
using MKS.Identity.Domain.Apps.Repositories;
using MKS.Identity.Domain.Authorization.Services;
using MKS.Identity.Domain.Authorization.Events;
using MKS.Identity.Domain.MultiTenancy.Entities;
using MKS.Identity.Domain.MultiTenancy.Repositories;
using MKS.Identity.Domain.Shared.Enums;
using MKS.Identity.Domain.Users.Entities;
using MKS.Identity.Domain.Users.Events;
using MKS.Identity.Domain.Users.Repositories;
using MKS.Identity.Domain.Users.Services;
using MKS.Shared.Application.Contracts.Common.Interfaces;
using MKS.Shared.Application.Contracts.Common.DTOs;
using MKS.Shared.Application.Contracts.Common.Constants;
using MKS.Shared.Domain.Common;
using MKS.Shared.Domain.MultiTenancy.Repositories;
using MKS.Shared.Domain.Shared.Common;
using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Identity.Application.Users
{
    /// <summary>
    /// Implements tenant-scoped user profile and membership operations, provisioning, and password management.
    /// </summary>
    public sealed class UserApplicationService(
        IUserRepository userRepository,
        IUserSessionRepository sessionRepository,
        ITenantRepository<TenantEntity> tenantRepository,
        ITenantMembershipRepository membershipRepository,
        PermissionEvaluator permissionEvaluator,
        IPasswordService passwordService,
        IIdentityUnitOfWork unitOfWork,
        IIntegrationEventPublisher integrationEventPublisher,
        ICurrentContext currentContext,
        ITenantScopeAccessor tenantScope,
        IOptions<IdentityWorkflowOptions> options,
        TimeProvider timeProvider,
        IIdentityEntityRepository<UserEntity> userEntityRepository,
        ISecurityAuditLogWriter securityAuditLogWriter) : IUserApplicationService
    {
        private static readonly string[] ProfileFields =
        [
            nameof(UserEntity.FirstName),
            nameof(UserEntity.LastName),
            nameof(UserEntity.Thumbnail),
            nameof(FullAuditEntity.ModifiedAt),
            nameof(FullAuditEntity.ModifierId)
        ];

        private static readonly string[] MembershipVersionFields =
        [
            nameof(UserEntity.MembershipVersion),
            nameof(FullAuditEntity.ModifiedAt),
            nameof(FullAuditEntity.ModifierId)
        ];

        /// <inheritdoc />
        public async Task<UserCreatedDto> CreateAsync(
            CreateUserDto input,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            await CreateManyAsync(new CreateManyUsersDto([input]), cancellationToken);
            return new UserCreatedDto(Accepted: true);
        }

        /// <inheritdoc />
        public async Task<UserBulkResultDto> CreateManyAsync(
            CreateManyUsersDto input,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            ArgumentNullException.ThrowIfNull(input.Users);
            var requests = input.Users.ToArray();
            if (requests.Length is < 1 or > PagingConstants.MaxMaxResultCount ||
                requests.Any(request => request is null))
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }

            var tenantId = requests[0].TenantId;
            if (tenantId == Guid.Empty ||
                currentContext.UserId is not { } actorId ||
                currentContext.AppId is not { } appId ||
                currentContext.TenantId != tenantId ||
                requests.Any(request => request.TenantId != tenantId))
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }

            var tenant = await tenantRepository.GetAsync(tenantId, cancellationToken)
                ?? throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            if (appId != tenant.AppId)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }

            var candidates = requests.Select(request => UserEntity.Create(
                    request.Email,
                    request.PhoneNumber,
                    request.Username,
                    request.FirstName,
                    request.LastName))
                .ToArray();
            var requestedIdentifiers = new HashSet<string>(StringComparer.Ordinal);
            foreach (var candidate in candidates)
            {
                foreach (var identifier in candidate.Identifiers)
                {
                    if (!requestedIdentifiers.Add(identifier.NormalizedValue))
                    {
                        throw new DomainException(IdentityDomainErrorCodes.IdentifierAlreadyExists);
                    }
                }
            }

            using var tenantContext = tenantScope.Enter(tenant.Id);
            await unitOfWork.ExecuteAsync(async token =>
            {
                if (!await permissionEvaluator.HasPermissionAsync(
                        actorId,
                        tenant.AppId,
                        tenant.Id,
                        IdentityConstants.Permissions.TenantAdminUsersCreate,
                        token))
                {
                    throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
                }

                var usedUserIds = new HashSet<Guid>();
                foreach (var (request, candidate) in requests.Zip(candidates))
                {
                    var normalizedIdentifiers = candidate.Identifiers
                        .Select(identifier => identifier.NormalizedValue)
                        .ToArray();
                    var user = await userRepository.FindByAnyIdentifierAsync(normalizedIdentifiers, token);
                    var isExistingUser = user is not null;
                    if (user is null)
                    {
                        user = candidate;
                        var defaultPassword = options.Value.DefaultPassword;
                        if (string.IsNullOrWhiteSpace(defaultPassword))
                        {
                            throw new InvalidOperationException(
                                $"{IdentityConstants.ConfigurationKeys.DefaultPassword} must be configured.");
                        }

                        await userRepository.AddAsync(
                            user,
                            UserCredentialEntity.Create(
                                user.Id,
                                passwordService.Hash(user.Id, defaultPassword),
                                mustChangePassword: true),
                            token);
                        await integrationEventPublisher.PublishAsync(new UserCreatedIntegrationEvent(
                            Guid.CreateVersion7(),
                            user.Id,
                            tenant.AppId,
                            tenant.Id,
                            timeProvider.GetUtcNow()), token);
                    }

                    if (!usedUserIds.Add(user.Id))
                    {
                        throw new DomainException(IdentityDomainErrorCodes.IdentifierAlreadyExists);
                    }

                    var existingMembership = await membershipRepository.FindAsync(user.Id, tenant.Id, token);
                    if (existingMembership is null)
                    {
                        await membershipRepository.AddAsync(new TenantMembershipEntity
                        {
                            UserId = user.Id,
                            AppId = tenant.AppId,
                            TenantId = tenant.Id,
                            Status = CMembershipStatus.Active
                        }, token);
                    }
                    else if (existingMembership.Status != CMembershipStatus.Active)
                    {
                        existingMembership.Status = CMembershipStatus.Active;
                        await membershipRepository.UpdateAsync(existingMembership, token);
                    }
                    else
                    {
                        continue;
                    }

                    user.IncrementMembershipVersion();
                    if (isExistingUser)
                    {
                        await userEntityRepository.UpdatePropertiesAsync(user, MembershipVersionFields, token);
                    }

                    await integrationEventPublisher.PublishAsync(new AuthorizationChangedIntegrationEvent(
                        Guid.CreateVersion7(),
                        user.Id,
                        tenant.AppId,
                        tenant.Id,
                        null,
                        timeProvider.GetUtcNow()), token);
                }
            }, cancellationToken);

            return new UserBulkResultDto(requests.Length);
        }

        /// <inheritdoc />
        public async Task<UserDto> GetAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }

            var scope = RequireTenantScope();
            using var tenantContext = tenantScope.Enter(scope.TenantId);
            await EnsurePermissionAsync(scope, IdentityConstants.Permissions.TenantAdminUsersRead, cancellationToken);
            var user = await userRepository.GetInTenantAsync(
                userId,
                scope.TenantId,
                scope.AppId,
                cancellationToken)
                ?? throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            return ToDto(user);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<UserDto>> GetManyAsync(
            GetManyUsersDto input,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            ArgumentNullException.ThrowIfNull(input.UserIds);
            var ids = input.UserIds.ToArray();
            ValidateUserIds(ids);
            var scope = RequireTenantScope();
            using var tenantContext = tenantScope.Enter(scope.TenantId);
            await EnsurePermissionAsync(scope, IdentityConstants.Permissions.TenantAdminUsersRead, cancellationToken);
            var users = await userRepository.GetManyInTenantAsync(ids, scope.TenantId, scope.AppId, cancellationToken);
            EnsureAllTargetsFound(ids, users.Select(user => user.Id));
            var byId = users.ToDictionary(user => user.Id);
            return ids.Select(id => ToDto(byId[id])).ToArray();
        }

        /// <inheritdoc />
        public async Task<PagedResultDto<UserDto>> GetPagedAsync(
            UserPageRequestDto input,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            if (input.SkipCount < 0 ||
                input.Sorting?.Length > PagingConstants.MaxSortingLength ||
                input.Filter?.Length > PagingConstants.MaxFilterLength ||
                input.Status is { } status && !Enum.IsDefined(status))
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }

            var (sortBy, descending) = ParseSorting(input.Sorting);
            var scope = RequireTenantScope();
            using var tenantContext = tenantScope.Enter(scope.TenantId);
            await EnsurePermissionAsync(scope, IdentityConstants.Permissions.TenantAdminUsersRead, cancellationToken);
            var page = await userRepository.GetPagedInTenantAsync(
                scope.TenantId,
                scope.AppId,
                input.SkipCount,
                Math.Clamp(input.MaxResultCount, 1, PagingConstants.MaxMaxResultCount),
                input.Filter,
                input.Status,
                sortBy,
                descending,
                cancellationToken);
            return new PagedResultDto<UserDto>(page.TotalCount, page.Items.Select(ToDto).ToArray());
        }

        /// <inheritdoc />
        public async Task<UserDto> UpdateProfileAsync(
            Guid userId,
            UpdateUserProfileDto input,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            ValidateUserIds([userId]);
            ValidateProfile(input.FirstName, input.LastName, input.Thumbnail);
            var scope = RequireTenantScope();
            using var tenantContext = tenantScope.Enter(scope.TenantId);
            UserEntity? updated = null;
            await unitOfWork.ExecuteAsync(async token =>
            {
                await EnsurePermissionAsync(scope, IdentityConstants.Permissions.TenantAdminUsersUpdate, token);
                updated = await userRepository.GetInTenantAsync(
                    userId,
                    scope.TenantId,
                    scope.AppId,
                    token) ?? throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
                ApplyProfile(updated, input.FirstName, input.LastName, input.Thumbnail);
                SetModifiedAudit(updated, scope.ActorId, timeProvider.GetUtcNow());
                await userEntityRepository.UpdatePropertiesAsync(updated, ProfileFields, token);
                await securityAuditLogWriter.AppendAsync(
                    scope.ActorId,
                    currentContext.ImpersonatorId,
                    currentContext.BreakGlassGrantId,
                    IdentityConstants.AuditActions.TenantUserProfileUpdated,
                    nameof(UserEntity),
                    userId.ToString(),
                    $"TenantId={scope.TenantId}",
                    timeProvider.GetUtcNow(),
                    token);
            }, cancellationToken);
            return ToDto(updated!);
        }

        /// <inheritdoc />
        public async Task<UserBulkResultDto> UpdateProfilesAsync(
            BulkUpdateUserProfilesDto input,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            ArgumentNullException.ThrowIfNull(input.Users);
            if (input.Users.Any(item => item is null))
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }

            var ids = input.Users.Select(item => item.UserId).ToArray();
            ValidateUserIds(ids);
            foreach (var item in input.Users)
            {
                ValidateProfile(item.FirstName, item.LastName, item.Thumbnail);
            }

            var scope = RequireTenantScope();
            using var tenantContext = tenantScope.Enter(scope.TenantId);
            await unitOfWork.ExecuteAsync(async token =>
            {
                await EnsurePermissionAsync(scope, IdentityConstants.Permissions.TenantAdminUsersUpdate, token);
                var users = await userRepository.GetManyInTenantAsync(ids, scope.TenantId, scope.AppId, token);
                EnsureAllTargetsFound(ids, users.Select(user => user.Id));
                var byId = users.ToDictionary(user => user.Id);
                var now = timeProvider.GetUtcNow();
                foreach (var item in input.Users)
                {
                    var user = byId[item.UserId];
                    ApplyProfile(user, item.FirstName, item.LastName, item.Thumbnail);
                    SetModifiedAudit(user, scope.ActorId, now);
                    await securityAuditLogWriter.AppendAsync(
                        scope.ActorId,
                        currentContext.ImpersonatorId,
                        currentContext.BreakGlassGrantId,
                        IdentityConstants.AuditActions.TenantUserProfileUpdated,
                        nameof(UserEntity),
                        item.UserId.ToString(),
                        $"TenantId={scope.TenantId}",
                        now,
                        token);
                }

                await userEntityRepository.BulkUpdatePropertiesAsync(users, ProfileFields, token);
            }, cancellationToken);
            return new UserBulkResultDto(ids.Length);
        }

        /// <inheritdoc />
        public async Task DeactivateAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            ValidateUserIds([userId]);
            var scope = RequireTenantScope();
            using var tenantContext = tenantScope.Enter(scope.TenantId);
            await unitOfWork.ExecuteAsync(async token =>
            {
                await EnsurePermissionAsync(scope, IdentityConstants.Permissions.TenantAdminUsersDelete, token);
                var user = await userRepository.GetInTenantAsync(
                    userId,
                    scope.TenantId,
                    scope.AppId,
                    token,
                    includeInactiveMembership: true) ?? throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
                await DeactivateMembershipAsync(user, scope, token);
            }, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<UserBulkResultDto> DeactivateManyAsync(
            DeactivateUsersDto input,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            ArgumentNullException.ThrowIfNull(input.UserIds);
            var ids = input.UserIds.ToArray();
            ValidateUserIds(ids);
            var scope = RequireTenantScope();
            using var tenantContext = tenantScope.Enter(scope.TenantId);
            await unitOfWork.ExecuteAsync(async token =>
            {
                await EnsurePermissionAsync(scope, IdentityConstants.Permissions.TenantAdminUsersDelete, token);
                var users = await userRepository.GetManyInTenantAsync(
                    ids,
                    scope.TenantId,
                    scope.AppId,
                    token,
                    includeInactiveMembership: true);
                EnsureAllTargetsFound(ids, users.Select(user => user.Id));
                var memberships = new List<TenantMembershipEntity>(users.Count);
                foreach (var user in users)
                {
                    var membership = await membershipRepository.FindAsync(user.Id, scope.TenantId, token);
                    if (membership is null ||
                        membership.AppId != scope.AppId ||
                        membership.Status is not (CMembershipStatus.Active or CMembershipStatus.Suspended))
                    {
                        throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
                    }

                    memberships.Add(membership);
                }

                var now = timeProvider.GetUtcNow();
                foreach (var membership in memberships)
                {
                    membership.Status = CMembershipStatus.Removed;
                    SetModifiedAudit(membership, scope.ActorId, now);
                    await membershipRepository.UpdateAsync(membership, token);
                }

                foreach (var user in users)
                {
                    user.IncrementMembershipVersion();
                    SetModifiedAudit(user, scope.ActorId, now);
                    await integrationEventPublisher.PublishAsync(new AuthorizationChangedIntegrationEvent(
                        Guid.CreateVersion7(),
                        user.Id,
                        scope.AppId,
                        scope.TenantId,
                        null,
                        now), token);
                    await securityAuditLogWriter.AppendAsync(
                        scope.ActorId,
                        currentContext.ImpersonatorId,
                        currentContext.BreakGlassGrantId,
                        IdentityConstants.AuditActions.TenantUserMembershipDeactivated,
                        nameof(TenantMembershipEntity),
                        user.Id.ToString(),
                        $"TenantId={scope.TenantId}",
                        now,
                        token);
                }

                await userEntityRepository.BulkUpdatePropertiesAsync(users, MembershipVersionFields, token);
            }, cancellationToken);
            return new UserBulkResultDto(ids.Length);
        }

        /// <inheritdoc />
        public async Task ChangePasswordAsync(
            ChangePasswordDto input,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            if (currentContext.UserId is not { } userId)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidCredentials);
            }

            if (!passwordService.MeetsPolicy(input.NewPassword))
            {
                throw new DomainException(IdentityDomainErrorCodes.PasswordPolicy);
            }

            var credential = await userRepository.GetCredentialAsync(userId, cancellationToken)
                ?? throw new DomainException(IdentityDomainErrorCodes.InvalidCredentials);
            if (!passwordService.Verify(userId, credential.PasswordHash, input.CurrentPassword))
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidCredentials);
            }

            credential.ChangePassword(passwordService.Hash(userId, input.NewPassword), timeProvider.GetUtcNow());
            var user = await userRepository.GetAsync(userId, cancellationToken)
                ?? throw new DomainException(IdentityDomainErrorCodes.InvalidCredentials);
            await unitOfWork.ExecuteAsync(async token =>
            {
                await userRepository.UpdateAsync(user, credential, token);
                await sessionRepository.RevokeAllForUserAsync(userId, timeProvider.GetUtcNow(), token);
            }, cancellationToken);
        }

        private async Task DeactivateMembershipAsync(UserEntity user, TenantScope scope, CancellationToken cancellationToken)
        {
            var membership = await membershipRepository.FindAsync(user.Id, scope.TenantId, cancellationToken);
            if (membership is null ||
                membership.AppId != scope.AppId ||
                membership.Status is not (CMembershipStatus.Active or CMembershipStatus.Suspended))
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }

            var now = timeProvider.GetUtcNow();
            membership.Status = CMembershipStatus.Removed;
            SetModifiedAudit(membership, scope.ActorId, now);
            await membershipRepository.UpdateAsync(membership, cancellationToken);
            user.IncrementMembershipVersion();
            SetModifiedAudit(user, scope.ActorId, now);
            await userEntityRepository.UpdatePropertiesAsync(user, MembershipVersionFields, cancellationToken);
            await integrationEventPublisher.PublishAsync(new AuthorizationChangedIntegrationEvent(
                Guid.CreateVersion7(),
                user.Id,
                scope.AppId,
                scope.TenantId,
                null,
                now), cancellationToken);
            await securityAuditLogWriter.AppendAsync(
                scope.ActorId,
                currentContext.ImpersonatorId,
                currentContext.BreakGlassGrantId,
                IdentityConstants.AuditActions.TenantUserMembershipDeactivated,
                nameof(TenantMembershipEntity),
                user.Id.ToString(),
                $"TenantId={scope.TenantId}",
                now,
                cancellationToken);
        }

        private TenantScope RequireTenantScope()
        {
            if (currentContext.UserId is not { } actorId ||
                currentContext.AppId is not { } appId ||
                currentContext.TenantId is not { } tenantId)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }

            return new TenantScope(actorId, appId, tenantId);
        }

        private async Task EnsurePermissionAsync(
            TenantScope scope,
            string permission,
            CancellationToken cancellationToken)
        {
            if (!await permissionEvaluator.HasPermissionAsync(
                    scope.ActorId,
                    scope.AppId,
                    scope.TenantId,
                    permission,
                    cancellationToken))
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }
        }

        private static void ValidateUserIds(IReadOnlyCollection<Guid> ids)
        {
            if (ids.Count is < 1 or > 1000 ||
                ids.Any(id => id == Guid.Empty) ||
                ids.Distinct().Count() != ids.Count)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }
        }

        private static void EnsureAllTargetsFound(
            IReadOnlyCollection<Guid> requestedIds,
            IEnumerable<Guid> foundIds)
        {
            if (!requestedIds.ToHashSet().SetEquals(foundIds))
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }
        }

        private static void ValidateProfile(string? firstName, string? lastName, string? thumbnail)
        {
            if (firstName?.Length > 100 || lastName?.Length > 100 || thumbnail?.Length > 2048)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }
        }

        private static void ApplyProfile(UserEntity user, string? firstName, string? lastName, string? thumbnail)
        {
            user.FirstName = firstName?.Trim();
            user.LastName = lastName?.Trim();
            user.Thumbnail = thumbnail?.Trim();
        }

        private static void SetModifiedAudit(FullAuditEntity entity, Guid actorId, DateTimeOffset now)
        {
            entity.ModifiedAt = now;
            entity.ModifierId = actorId;
        }

        private static UserDto ToDto(UserEntity user) =>
            new(user.Id, user.FirstName, user.LastName, user.Thumbnail, user.MasterStatus);

        private static (string SortBy, bool Descending) ParseSorting(string? sorting)
        {
            if (string.IsNullOrWhiteSpace(sorting))
            {
                return ("createdAt", true);
            }

            var parts = sorting.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var rawSortBy = parts[0];
            var descending = rawSortBy.StartsWith("-", StringComparison.Ordinal);
            var sortBy = descending ? rawSortBy[1..] : rawSortBy;
            if (parts.Length > 2 ||
                parts.Length == 2 && descending ||
                parts.Length == 2 && parts[1] is not ("asc" or "desc") ||
                sortBy is not ("firstName" or "lastName" or "createdAt" or "modifiedAt" or "id"))
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }

            if (parts.Length == 2)
            {
                descending = parts[1] == "desc";
            }

            return (sortBy, descending);
        }

        private sealed record TenantScope(Guid ActorId, Guid AppId, Guid TenantId);

    }
}
