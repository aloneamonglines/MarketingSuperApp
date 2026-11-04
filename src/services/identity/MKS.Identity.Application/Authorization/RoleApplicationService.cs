using System.Linq.Expressions;
using MKS.Identity.Domain.Shared.Constants;
using MKS.Identity.Application.Contracts.Authorization.DTOs;
using MKS.Identity.Application.Contracts.Authorization.Interfaces;
using MKS.Identity.Application.Contracts.Common.Interfaces;
using MKS.Identity.Application.Contracts.Common.Repositories;
using MKS.Identity.Domain.Authorization.Entities;
using MKS.Identity.Domain.Authorization.Events;
using MKS.Identity.Domain.Authorization.Repositories;
using MKS.Identity.Domain.Authorization.Services;
using MKS.Identity.Domain.MultiTenancy.Repositories;
using MKS.Identity.Domain.Users.Entities;
using MKS.Identity.Domain.Users.Repositories;
using MKS.Shared.Application.Contracts.Common.DTOs;
using MKS.Shared.Application.Contracts.Common.Constants;
using MKS.Shared.Domain.Shared.Common;
using MKS.Shared.Application.Contracts.Common.Interfaces;
using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Identity.Application.Authorization
{
    /// <summary>
    /// Implements role catalog and assignment workflows.
    /// </summary>
    public sealed class RoleApplicationService(
        IRoleRepository roleRepository,
        IAuthorizationRepository authorizationRepository,
        ITenantMembershipRepository membershipRepository,
        IUserRepository userRepository,
        IIdentityEntityRepository<RoleEntity> roleEntities,
        IIdentityEntityRepository<UserEntity> userEntities,
        IIdentityUnitOfWork unitOfWork,
        ICurrentContext currentContext,
        ITenantScopeAccessor tenantScope,
        PermissionEvaluator permissionEvaluator,
        TimeProvider timeProvider,
        IIntegrationEventPublisher integrationEventPublisher) : IRoleApplicationService
    {
        /// <inheritdoc />
        public async Task<Guid> CreateAsync(
            CreateRoleDto input,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            if (currentContext.UserId is not { } actorId ||
                currentContext.AppId is not { } appId ||
                actorId == Guid.Empty ||
                appId == Guid.Empty ||
                currentContext.TenantId == Guid.Empty)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }

            if (string.IsNullOrWhiteSpace(input.Name) ||
                string.IsNullOrWhiteSpace(input.Code) ||
                input.FunctionCodes is null ||
                input.PermissionCodes is null)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }

            var tenantId = currentContext.TenantId;
            using var scope = tenantId is { } scopedTenantId ? tenantScope.Enter(scopedTenantId) : null;
            Guid roleId = Guid.Empty;
            await unitOfWork.ExecuteAsync(async token =>
            {
                var isAppAdmin = await authorizationRepository.IsAppAdminAsync(actorId, appId, token);
                if (!isAppAdmin && tenantId is null)
                {
                    throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
                }

                if (await roleRepository.CodeExistsAsync(
                        appId,
                        tenantId,
                        input.Code.Trim().ToLowerInvariant(),
                        token))
                {
                    throw new DomainException(IdentityDomainErrorCodes.IdentifierAlreadyExists);
                }

                if (input.FunctionCodes.Count == 0 && input.PermissionCodes.Count == 0 ||
                    !await roleRepository.CatalogEntriesExistAsync(
                        appId,
                        input.FunctionCodes,
                        input.PermissionCodes,
                        token))
                {
                    throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
                }

                var effectivePermissions = input.PermissionCodes.ToHashSet(StringComparer.Ordinal);
                if (input.FunctionCodes.Count > 0)
                {
                    var functionPermissions = await roleRepository.GetFunctionPermissionsAsync(
                        appId,
                        input.FunctionCodes,
                        token);
                    effectivePermissions.UnionWith(functionPermissions);
                }

                if (!isAppAdmin)
                {
                    foreach (var permissionCode in effectivePermissions)
                    {
                        if (!await permissionEvaluator.HasPermissionAsync(
                                actorId,
                                appId,
                                tenantId,
                                permissionCode,
                                token))
                        {
                            throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
                        }
                    }
                }

                var createdAt = timeProvider.GetUtcNow();
                var role = new RoleEntity
                {
                    AppId = appId,
                    TenantId = tenantId,
                    Name = input.Name.Trim(),
                    Code = input.Code.Trim().ToLowerInvariant(),
                    MasterStatus = CMasterStatus.Active,
                    CreatorId = actorId,
                    CreatedAt = createdAt
                };
                var functionLinks = input.FunctionCodes.Distinct(StringComparer.Ordinal)
                    .Select(code => new RoleFunctionEntity
                    {
                        AppId = appId,
                        RoleId = role.Id,
                        FunctionCode = code,
                        CreatorId = actorId,
                        CreatedAt = createdAt
                    })
                    .ToArray();
                var permissionLinks = input.PermissionCodes.Distinct(StringComparer.Ordinal)
                    .Select(code => new RolePermissionEntity
                    {
                        AppId = appId,
                        RoleId = role.Id,
                        PermissionCode = code,
                        CreatorId = actorId,
                        CreatedAt = createdAt
                    })
                    .ToArray();
                await roleRepository.AddAsync(role, functionLinks, permissionLinks, token);
                roleId = role.Id;
                await EnqueueMembershipChangedAsync(actorId, appId, tenantId, role.Id, token);
            }, cancellationToken);
            return roleId;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<Guid>> CreateManyAsync(
            IReadOnlyCollection<CreateRoleDto> inputs,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(inputs);
            if (inputs.Count is < 1 or > PagingConstants.MaxMaxResultCount ||
                inputs.Any(input => input is null) ||
                inputs.Any(input =>
                    string.IsNullOrWhiteSpace(input.Name) ||
                    string.IsNullOrWhiteSpace(input.Code) ||
                    input.FunctionCodes is null ||
                    input.PermissionCodes is null))
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }

            var normalizedCodes = inputs
                .Select(input => input.Code.Trim().ToLowerInvariant())
                .ToArray();
            if (normalizedCodes.Distinct(StringComparer.Ordinal).Count() != normalizedCodes.Length)
            {
                throw new DomainException(IdentityDomainErrorCodes.IdentifierAlreadyExists);
            }

            var (actorId, appId, tenantId) = RequireContext();
            using var scope = tenantId is { } scopedTenantId ? tenantScope.Enter(scopedTenantId) : null;
            var roleIds = new List<Guid>(inputs.Count);
            await unitOfWork.ExecuteAsync(async token =>
            {
                var isAppAdmin = await authorizationRepository.IsAppAdminAsync(actorId, appId, token);
                if (!isAppAdmin && tenantId is null)
                {
                    throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
                }

                foreach (var code in normalizedCodes)
                {
                    if (await roleRepository.CodeExistsAsync(appId, tenantId, code, token))
                    {
                        throw new DomainException(IdentityDomainErrorCodes.IdentifierAlreadyExists);
                    }
                }

                var createdAt = timeProvider.GetUtcNow();
                foreach (var input in inputs)
                {
                    if (input.FunctionCodes.Count == 0 && input.PermissionCodes.Count == 0 ||
                        !await roleRepository.CatalogEntriesExistAsync(
                            appId,
                            input.FunctionCodes,
                            input.PermissionCodes,
                            token))
                    {
                        throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
                    }

                    var effectivePermissions = input.PermissionCodes.ToHashSet(StringComparer.Ordinal);
                    if (input.FunctionCodes.Count > 0)
                    {
                        var functionPermissions = await roleRepository.GetFunctionPermissionsAsync(
                            appId,
                            input.FunctionCodes,
                            token);
                        effectivePermissions.UnionWith(functionPermissions);
                    }

                    if (!isAppAdmin)
                    {
                        foreach (var permissionCode in effectivePermissions)
                        {
                            if (!await permissionEvaluator.HasPermissionAsync(
                                    actorId,
                                    appId,
                                    tenantId,
                                    permissionCode,
                                    token))
                            {
                                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
                            }
                        }
                    }

                    var role = new RoleEntity
                    {
                        AppId = appId,
                        TenantId = tenantId,
                        Name = input.Name.Trim(),
                        Code = input.Code.Trim().ToLowerInvariant(),
                        MasterStatus = CMasterStatus.Active,
                        CreatorId = actorId,
                        CreatedAt = createdAt
                    };
                    var functionLinks = input.FunctionCodes.Distinct(StringComparer.Ordinal)
                        .Select(code => new RoleFunctionEntity
                        {
                            AppId = appId,
                            RoleId = role.Id,
                            FunctionCode = code,
                            CreatorId = actorId,
                            CreatedAt = createdAt
                        })
                        .ToArray();
                    var permissionLinks = input.PermissionCodes.Distinct(StringComparer.Ordinal)
                        .Select(code => new RolePermissionEntity
                        {
                            AppId = appId,
                            RoleId = role.Id,
                            PermissionCode = code,
                            CreatorId = actorId,
                            CreatedAt = createdAt
                        })
                        .ToArray();
                    await roleRepository.AddAsync(role, functionLinks, permissionLinks, token);
                    roleIds.Add(role.Id);
                    await EnqueueMembershipChangedAsync(actorId, appId, tenantId, role.Id, token);
                }
            }, cancellationToken);

            return roleIds;
        }

        /// <inheritdoc />
        public async Task<RoleDto> GetAsync(Guid roleId, CancellationToken cancellationToken = default)
        {
            if (roleId == Guid.Empty)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }

            var (_, appId, tenantId) = RequireContext();
            var role = await roleEntities.GetAsync(roleId, cancellationToken)
                ?? throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            EnsureRoleVisible(role, appId, tenantId);
            return ToDto(role);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<RoleDto>> GetManyAsync(
            IReadOnlyCollection<Guid> roleIds,
            CancellationToken cancellationToken = default)
        {
            ValidateIds(roleIds);
            var (_, appId, tenantId) = RequireContext();
            var roles = await roleEntities.GetManyAsync(roleIds, cancellationToken);
            if (roles.Count != roleIds.Count ||
                roles.Any(role => !IsRoleVisible(role, appId, tenantId)))
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }

            var byId = roles.ToDictionary(role => role.Id);
            return roleIds.Select(id => ToDto(byId[id])).ToArray();
        }

        /// <inheritdoc />
        public async Task<PagedResultDto<RoleDto>> GetPagedAsync(
            RolePagedRequestDto request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            var (_, appId, tenantId) = RequireContext();
            var (sortField, descending) = ParseSorting(request.Sorting);
            var search = request.Filter?.Trim();
            Expression<Func<RoleEntity, bool>> filter = role =>
                role.AppId == appId &&
                (role.TenantId == null || role.TenantId == tenantId) &&
                (string.IsNullOrEmpty(search) ||
                    role.Name.Contains(search) ||
                    role.Code.Contains(search));
            Func<IQueryable<RoleEntity>, IOrderedQueryable<RoleEntity>> orderBy = query =>
                ApplyRoleSorting(query, sortField, descending);
            var page = await roleEntities.GetPagedAsync(request, filter, orderBy, cancellationToken);
            return new PagedResultDto<RoleDto>(page.TotalCount, page.Items.Select(ToDto).ToArray());
        }

        /// <inheritdoc />
        public async Task UpdateAsync(
            Guid roleId,
            UpdateRoleDto input,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            if (roleId == Guid.Empty)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }

            ValidateEditableFields(input.Name, input.MasterStatus);
            var (actorId, appId, tenantId) = RequireContext();
            using var scope = tenantId is { } scopedTenantId ? tenantScope.Enter(scopedTenantId) : null;
            await unitOfWork.ExecuteAsync(async token =>
            {
                var role = await roleEntities.GetAsync(roleId, token)
                    ?? throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
                var isAppAdmin = await authorizationRepository.IsAppAdminAsync(actorId, appId, token);
                await EnsureCanManageAsync(role, actorId, appId, tenantId, isAppAdmin, token);
                var newStatus = (CMasterStatus)input.MasterStatus;
                var authorizationChanged = role.MasterStatus != newStatus;
                role.Name = input.Name.Trim();
                role.MasterStatus = newStatus;
                role.ModifierId = actorId;
                role.ModifiedAt = timeProvider.GetUtcNow();
                await roleEntities.UpdateAsync(role, token);
                if (authorizationChanged)
                {
                    await InvalidateRoleAssignmentsAsync([role], token);
                }
            }, cancellationToken);
        }

        /// <inheritdoc />
        public async Task UpdateManyAsync(
            IReadOnlyCollection<UpdateRoleItemDto> inputs,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(inputs);
            if (inputs.Any(item => item is null))
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }

            ValidateIds(inputs.Select(item => item.Id).ToArray());
            foreach (var item in inputs)
            {
                ValidateEditableFields(item.Name, item.MasterStatus);
            }

            var (actorId, appId, tenantId) = RequireContext();
            using var scope = tenantId is { } scopedTenantId ? tenantScope.Enter(scopedTenantId) : null;
            await unitOfWork.ExecuteAsync(async token =>
            {
                var roles = await roleEntities.GetManyAsync(inputs.Select(item => item.Id).ToArray(), token);
                if (roles.Count != inputs.Count)
                {
                    throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
                }

                var rolesById = roles.ToDictionary(role => role.Id);
                var isAppAdmin = await authorizationRepository.IsAppAdminAsync(actorId, appId, token);
                var changedAuthorizationRoles = new List<RoleEntity>();
                foreach (var input in inputs)
                {
                    var role = rolesById[input.Id];
                    await EnsureCanManageAsync(role, actorId, appId, tenantId, isAppAdmin, token);
                    var newStatus = (CMasterStatus)input.MasterStatus;
                    if (role.MasterStatus != newStatus)
                    {
                        changedAuthorizationRoles.Add(role);
                    }

                    role.Name = input.Name.Trim();
                    role.MasterStatus = newStatus;
                    role.ModifierId = actorId;
                    role.ModifiedAt = timeProvider.GetUtcNow();
                }

                await roleEntities.BulkUpdateAsync(roles, token);
                if (changedAuthorizationRoles.Count > 0)
                {
                    await InvalidateRoleAssignmentsAsync(changedAuthorizationRoles, token);
                }
            }, cancellationToken);
        }

        /// <inheritdoc />
        public async Task DeleteAsync(Guid roleId, CancellationToken cancellationToken = default)
        {
            if (roleId == Guid.Empty)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }

            var (actorId, appId, tenantId) = RequireContext();
            using var scope = tenantId is { } scopedTenantId ? tenantScope.Enter(scopedTenantId) : null;
            await unitOfWork.ExecuteAsync(async token =>
            {
                var role = await roleEntities.GetAsync(roleId, token)
                    ?? throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
                var isAppAdmin = await authorizationRepository.IsAppAdminAsync(actorId, appId, token);
                await EnsureCanManageAsync(role, actorId, appId, tenantId, isAppAdmin, token);
                MarkDeleted(role, actorId, timeProvider.GetUtcNow());
                await roleEntities.UpdateAsync(role, token);
                await InvalidateRoleAssignmentsAsync([role], token);
            }, cancellationToken);
        }

        /// <inheritdoc />
        public async Task DeleteManyAsync(
            IReadOnlyCollection<Guid> roleIds,
            CancellationToken cancellationToken = default)
        {
            ValidateIds(roleIds);
            var (actorId, appId, tenantId) = RequireContext();
            using var scope = tenantId is { } scopedTenantId ? tenantScope.Enter(scopedTenantId) : null;
            await unitOfWork.ExecuteAsync(async token =>
            {
                var roles = await roleEntities.GetManyAsync(roleIds, token);
                if (roles.Count != roleIds.Count)
                {
                    throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
                }

                var isAppAdmin = await authorizationRepository.IsAppAdminAsync(actorId, appId, token);
                foreach (var role in roles)
                {
                    await EnsureCanManageAsync(role, actorId, appId, tenantId, isAppAdmin, token);
                    MarkDeleted(role, actorId, timeProvider.GetUtcNow());
                }

                await roleEntities.BulkUpdateAsync(roles, token);
                await InvalidateRoleAssignmentsAsync(roles, token);
            }, cancellationToken);
        }

        /// <inheritdoc />
        public async Task AssignAsync(
            Guid userId,
            Guid roleId,
            CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty ||
                roleId == Guid.Empty ||
                currentContext.UserId is not { } actorId ||
                currentContext.AppId is not { } appId ||
                currentContext.TenantId is not { } tenantId)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }

            using var scope = tenantScope.Enter(tenantId);
            await unitOfWork.ExecuteAsync(async token =>
            {
                var role = await roleRepository.GetAsync(roleId, token)
                    ?? throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
                var membership = await membershipRepository.FindAsync(userId, tenantId, token)
                    ?? throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
                var isAppAdmin = await authorizationRepository.IsAppAdminAsync(actorId, appId, token);
                RoleAssignmentValidator.Validate(
                    role.AppId,
                    role.TenantId,
                    membership.AppId,
                    membership.TenantId,
                    appId,
                    tenantId,
                    isAppAdmin);

                var permissions = await roleRepository.GetEffectivePermissionsAsync(roleId, token);
                if (!isAppAdmin)
                {
                    foreach (var permission in permissions)
                    {
                        if (!await permissionEvaluator.HasPermissionAsync(
                                actorId,
                                appId,
                                tenantId,
                                permission,
                                token))
                        {
                            throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
                        }
                    }
                }

                var targetUser = await userRepository.GetAsync(userId, token)
                    ?? throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
                targetUser.IncrementMembershipVersion();
                var credential = await userRepository.GetCredentialAsync(userId, token);
                await roleRepository.AddAssignmentAsync(new RoleAssignmentEntity
                {
                    AppId = appId,
                    RoleId = role.Id,
                    TenantId = tenantId,
                    UserId = userId,
                    IsActive = true
                }, token);
                if (credential is not null)
                {
                    await userRepository.UpdateAsync(targetUser, credential, token);
                }

                await EnqueueMembershipChangedAsync(userId, appId, tenantId, roleId, token);
            }, cancellationToken);
        }

        private (Guid ActorId, Guid AppId, Guid? TenantId) RequireContext()
        {
            if (currentContext.UserId is not { } actorId ||
                currentContext.AppId is not { } appId ||
                actorId == Guid.Empty ||
                appId == Guid.Empty ||
                currentContext.TenantId == Guid.Empty)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }

            return (actorId, appId, currentContext.TenantId);
        }

        private static bool IsRoleVisible(RoleEntity role, Guid appId, Guid? tenantId) =>
            role.AppId == appId &&
            (role.TenantId is null || tenantId is { } scopedTenantId && role.TenantId == scopedTenantId);

        private static void EnsureRoleVisible(RoleEntity role, Guid appId, Guid? tenantId)
        {
            if (!IsRoleVisible(role, appId, tenantId))
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }
        }

        private async Task EnsureCanManageAsync(
            RoleEntity role,
            Guid actorId,
            Guid appId,
            Guid? tenantId,
            bool isAppAdmin,
            CancellationToken cancellationToken)
        {
            EnsureRoleVisible(role, appId, tenantId);
            if (isAppAdmin)
            {
                return;
            }

            if (role.TenantId is not { } roleTenantId || tenantId != roleTenantId)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }

            var permissions = await roleRepository.GetEffectivePermissionsAsync(role.Id, cancellationToken);
            foreach (var permission in permissions)
            {
                if (!await permissionEvaluator.HasPermissionAsync(
                        actorId,
                        appId,
                        roleTenantId,
                        permission,
                        cancellationToken))
                {
                    throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
                }
            }
        }

        private async Task InvalidateRoleAssignmentsAsync(
            IReadOnlyCollection<RoleEntity> roles,
            CancellationToken cancellationToken)
        {
            var affectedAssignments = new HashSet<(Guid UserId, Guid TenantId, Guid RoleId)>();
            foreach (var role in roles)
            {
                var assignments = await roleRepository.GetActiveAssignmentsAsync(
                    role.AppId,
                    role.Id,
                    cancellationToken);
                foreach (var assignment in assignments)
                {
                    if (role.TenantId is { } roleTenantId && assignment.TenantId != roleTenantId)
                    {
                        continue;
                    }

                    affectedAssignments.Add((assignment.UserId, assignment.TenantId, role.Id));
                }
            }

            var now = timeProvider.GetUtcNow();
            foreach (var userId in affectedAssignments.Select(item => item.UserId).Distinct())
            {
                var user = await userEntities.GetAsync(userId, cancellationToken);
                if (user is null)
                {
                    continue;
                }

                user.IncrementMembershipVersion();
                user.ModifierId = currentContext.UserId;
                user.ModifiedAt = now;
                await userEntities.UpdateAsync(user, cancellationToken);
            }

            foreach (var assignment in affectedAssignments)
            {
                await integrationEventPublisher.PublishAsync(new AuthorizationChangedIntegrationEvent(
                    Guid.CreateVersion7(),
                    assignment.UserId,
                    roles.First(role => role.Id == assignment.RoleId).AppId,
                    assignment.TenantId,
                    assignment.RoleId,
                    now), cancellationToken);
            }
        }

        private static void ValidateEditableFields(string name, int masterStatus)
        {
            if (string.IsNullOrWhiteSpace(name) ||
                !Enum.IsDefined((CMasterStatus)masterStatus) ||
                (CMasterStatus)masterStatus == CMasterStatus.None)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }
        }

        private static void ValidateIds(IReadOnlyCollection<Guid> ids)
        {
            ArgumentNullException.ThrowIfNull(ids);
            if (ids.Count == 0 ||
                ids.Count > PagingConstants.MaxMaxResultCount ||
                ids.Any(id => id == Guid.Empty) ||
                ids.Distinct().Count() != ids.Count)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }
        }

        private static (string Field, bool Descending) ParseSorting(string? sorting)
        {
            if (string.IsNullOrWhiteSpace(sorting))
            {
                return (nameof(RoleEntity.Name), false);
            }

            var value = sorting.Trim();
            var descending = value.StartsWith("-", StringComparison.Ordinal);
            if (descending)
            {
                value = value[1..].Trim();
            }
            else
            {
                var parts = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 2 ||
                    parts.Length == 2 && !parts[1].Equals("asc", StringComparison.OrdinalIgnoreCase) &&
                    !parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase))
                {
                    throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
                }

                value = parts[0];
                descending = parts.Length == 2 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);
            }

            var field = value.ToLowerInvariant() switch
            {
                "id" => nameof(RoleEntity.Id),
                "name" => nameof(RoleEntity.Name),
                "code" => nameof(RoleEntity.Code),
                "masterstatus" or "status" => nameof(RoleEntity.MasterStatus),
                "createdat" => nameof(RoleEntity.CreatedAt),
                _ => throw new DomainException(IdentityDomainErrorCodes.InvalidInput)
            };
            return (field, descending);
        }

        private static IOrderedQueryable<RoleEntity> ApplyRoleSorting(
            IQueryable<RoleEntity> query,
            string field,
            bool descending) =>
            (field, descending) switch
            {
                (nameof(RoleEntity.Id), false) => query.OrderBy(role => role.Id),
                (nameof(RoleEntity.Id), true) => query.OrderByDescending(role => role.Id),
                (nameof(RoleEntity.Name), false) => query.OrderBy(role => role.Name).ThenBy(role => role.Id),
                (nameof(RoleEntity.Name), true) => query.OrderByDescending(role => role.Name).ThenBy(role => role.Id),
                (nameof(RoleEntity.Code), false) => query.OrderBy(role => role.Code).ThenBy(role => role.Id),
                (nameof(RoleEntity.Code), true) => query.OrderByDescending(role => role.Code).ThenBy(role => role.Id),
                (nameof(RoleEntity.MasterStatus), false) =>
                    query.OrderBy(role => role.MasterStatus).ThenBy(role => role.Id),
                (nameof(RoleEntity.MasterStatus), true) =>
                    query.OrderByDescending(role => role.MasterStatus).ThenBy(role => role.Id),
                (nameof(RoleEntity.CreatedAt), false) => query.OrderBy(role => role.CreatedAt).ThenBy(role => role.Id),
                _ => query.OrderByDescending(role => role.CreatedAt).ThenBy(role => role.Id)
            };

        private static RoleDto ToDto(RoleEntity role) =>
            new(role.Id, role.AppId, role.TenantId, role.Name, role.Code, (int)role.MasterStatus);

        private static void MarkDeleted(RoleEntity role, Guid actorId, DateTimeOffset now)
        {
            role.IsDeleted = true;
            role.DeleterId = actorId;
            role.DeletedAt = now;
            role.ModifierId = actorId;
            role.ModifiedAt = now;
        }

        private async Task EnqueueMembershipChangedAsync(
            Guid userId,
            Guid appId,
            Guid? tenantId,
            Guid roleId,
            CancellationToken cancellationToken)
        {
            if (tenantId is not { } scopedTenantId)
            {
                return;
            }

            await integrationEventPublisher.PublishAsync(new AuthorizationChangedIntegrationEvent(
                Guid.CreateVersion7(),
                userId,
                appId,
                scopedTenantId,
                roleId,
                timeProvider.GetUtcNow()), cancellationToken);
        }
    }
}
