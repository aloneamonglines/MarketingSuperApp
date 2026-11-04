using System.Linq.Expressions;
using MKS.Identity.Domain.Shared.Constants;
using Microsoft.Extensions.Options;
using MKS.Identity.Application.Contracts.Common.Interfaces;
using MKS.Identity.Application.Contracts.Common.Repositories;
using MKS.Identity.Application.Contracts.Common.Options;
using MKS.Identity.Application.Contracts.MultiTenancy.DTOs;
using MKS.Identity.Application.Contracts.MultiTenancy.Interfaces;
using MKS.Identity.Domain.Authorization.Repositories;
using MKS.Identity.Domain.MultiTenancy.Entities;
using MKS.Identity.Domain.MultiTenancy.Repositories;
using MKS.Identity.Domain.Shared.Enums;
using MKS.Identity.Domain.Users.Entities;
using MKS.Identity.Domain.Users.Events;
using MKS.Identity.Domain.Users.Repositories;
using MKS.Shared.Domain.MultiTenancy.Repositories;
using MKS.Shared.Domain.Shared.Common;
using MKS.Shared.Application.Contracts.Common.Interfaces;
using MKS.Shared.Application.Contracts.Common.Constants;
using MKS.Shared.Application.Contracts.Common.DTOs;
using MKS.Shared.Domain.Shared.Enums;

namespace MKS.Identity.Application.MultiTenancy
{
    /// <summary>
    /// Provides application-scoped tenant workflows.
    /// </summary>
    public sealed class TenantApplicationService(
        ITenantRepository<TenantEntity> tenantRepository,
        IAuthorizationRepository authorizationRepository,
        IUserRepository userRepository,
        ITenantMembershipRepository membershipRepository,
        ITenantAdminAssignmentRepository tenantAdminRepository,
        IIdentityUnitOfWork unitOfWork,
        IPasswordService passwordService,
        IIntegrationEventPublisher integrationEventPublisher,
        ITenantScopeAccessor tenantScope,
        ICurrentContext currentContext,
        IOptions<IdentityWorkflowOptions> options,
        TimeProvider timeProvider,
        IIdentityEntityRepository<TenantEntity> entityRepository,
        IIdentityEntityRepository<TenantMembershipEntity> membershipEntities,
        IIdentityEntityRepository<TenantAdminAssignmentEntity> tenantAdminEntities) : ITenantApplicationService
    {
        /// <inheritdoc />
        public async Task<Guid> CreateAsync(
            CreateTenantDto input,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            var appId = await EnsureAppAdminAsync(cancellationToken);

            var normalizedCode = input.Code.Trim().ToLowerInvariant();
            if (await tenantRepository.CodeExistsAsync(appId, normalizedCode, cancellationToken))
            {
                throw new DomainException(IdentityDomainErrorCodes.IdentifierAlreadyExists);
            }

            var tenant = TenantEntity.Create(appId, input.Name, normalizedCode);
            var initialAdmin = UserEntity.Create(input.AdminEmail, input.AdminPhoneNumber, input.AdminUsername);
            var identifiers = initialAdmin.Identifiers.Select(identifier => identifier.NormalizedValue).ToArray();
            var existingUser = await userRepository.FindByAnyIdentifierAsync(identifiers, cancellationToken);
            using var tenantContext = tenantScope.Enter(tenant.Id);

            await unitOfWork.ExecuteAsync(async token =>
            {
                await tenantRepository.AddAsync(tenant, token);
                UserEntity user;
                if (existingUser is not null)
                {
                    user = existingUser;
                }
                else
                {
                    user = initialAdmin;
                    if (string.IsNullOrWhiteSpace(options.Value.DefaultPassword))
                    {
                        throw new InvalidOperationException(
                            $"{IdentityConstants.ConfigurationKeys.DefaultPassword} must be configured.");
                    }

                    await userRepository.AddAsync(
                        user,
                        UserCredentialEntity.Create(
                            user.Id,
                            passwordService.Hash(user.Id, options.Value.DefaultPassword),
                            mustChangePassword: true),
                        token);
                    await integrationEventPublisher.PublishAsync(
                        new UserCreatedIntegrationEvent(
                            Guid.CreateVersion7(),
                            user.Id,
                            appId,
                            tenant.Id,
                            timeProvider.GetUtcNow()),
                        token);
                }

                await membershipRepository.AddAsync(new TenantMembershipEntity
                {
                    TenantId = tenant.Id,
                    AppId = appId,
                    UserId = user.Id,
                    Status = CMembershipStatus.Active
                }, token);
                await tenantAdminRepository.AddAsync(new TenantAdminAssignmentEntity
                {
                    TenantId = tenant.Id,
                    UserId = user.Id,
                    IsActive = true
                }, token);
            }, cancellationToken);

            return tenant.Id;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<Guid>> CreateManyAsync(
            IReadOnlyCollection<CreateTenantDto> inputs,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(inputs);
            if (inputs.Count is < 1 or > PagingConstants.MaxMaxResultCount ||
                inputs.Any(input => input is null) ||
                inputs.Any(input =>
                    string.IsNullOrWhiteSpace(input.Name) ||
                    string.IsNullOrWhiteSpace(input.Code) ||
                    string.IsNullOrWhiteSpace(input.AdminEmail) &&
                    string.IsNullOrWhiteSpace(input.AdminPhoneNumber) &&
                    string.IsNullOrWhiteSpace(input.AdminUsername)))
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }

            var appId = await EnsureAppAdminAsync(cancellationToken);
            var normalizedCodes = inputs.Select(input => input.Code.Trim().ToLowerInvariant()).ToArray();
            if (normalizedCodes.Distinct(StringComparer.Ordinal).Count() != normalizedCodes.Length)
            {
                throw new DomainException(IdentityDomainErrorCodes.IdentifierAlreadyExists);
            }

            if (await tenantRepository.AnyCodeExistsAsync(appId, normalizedCodes, cancellationToken))
            {
                throw new DomainException(IdentityDomainErrorCodes.IdentifierAlreadyExists);
            }

            var tenants = inputs
                .Select((input, index) => TenantEntity.Create(appId, input.Name, normalizedCodes[index]))
                .ToArray();
            var adminCandidates = inputs
                .Select(input => UserEntity.Create(input.AdminEmail, input.AdminPhoneNumber, input.AdminUsername))
                .ToArray();
            var createdAt = timeProvider.GetUtcNow();
            var actorId = currentContext.UserId
                ?? throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            foreach (var tenant in tenants)
            {
                tenant.CreatorId = actorId;
                tenant.CreatedAt = createdAt;
            }

            var memberships = new List<TenantMembershipEntity>(tenants.Length);
            var assignments = new List<TenantAdminAssignmentEntity>(tenants.Length);
            var roleUserByIdentifier = new Dictionary<string, UserEntity>(StringComparer.Ordinal);
            await unitOfWork.ExecuteAsync(async token =>
            {
                await entityRepository.BulkInsertAsync(tenants, token);
                for (var index = 0; index < tenants.Length; index++)
                {
                    var tenant = tenants[index];
                    var candidate = adminCandidates[index];
                    using var scope = tenantScope.Enter(tenant.Id);
                    var identifiers = candidate.Identifiers
                        .Select(identifier => identifier.NormalizedValue)
                        .ToArray();
                    var knownUsers = identifiers
                        .Select(identifier => roleUserByIdentifier.GetValueOrDefault(identifier))
                        .OfType<UserEntity>()
                        .DistinctBy(knownUser => knownUser.Id)
                        .ToArray();
                    if (knownUsers.Length > 1)
                    {
                        throw new DomainException(IdentityDomainErrorCodes.IdentifierAlreadyExists);
                    }

                    var user = await userRepository.FindByAnyIdentifierAsync(identifiers, token);
                    if (knownUsers.Length == 1 &&
                        user is not null &&
                        knownUsers[0].Id != user.Id)
                    {
                        throw new DomainException(IdentityDomainErrorCodes.IdentifierAlreadyExists);
                    }

                    user ??= knownUsers.FirstOrDefault();
                    if (user is null)
                    {
                        user = candidate;
                        if (string.IsNullOrWhiteSpace(options.Value.DefaultPassword))
                        {
                            throw new InvalidOperationException(
                                $"{IdentityConstants.ConfigurationKeys.DefaultPassword} must be configured.");
                        }

                        await userRepository.AddAsync(
                            user,
                            UserCredentialEntity.Create(
                                user.Id,
                                passwordService.Hash(user.Id, options.Value.DefaultPassword),
                                mustChangePassword: true),
                            token);
                        await integrationEventPublisher.PublishAsync(new UserCreatedIntegrationEvent(
                            Guid.CreateVersion7(),
                            user.Id,
                            appId,
                            tenant.Id,
                            timeProvider.GetUtcNow()), token);
                    }

                    foreach (var identifier in identifiers)
                    {
                        if (roleUserByIdentifier.TryGetValue(identifier, out var knownUser) &&
                            knownUser.Id != user.Id)
                        {
                            throw new DomainException(IdentityDomainErrorCodes.IdentifierAlreadyExists);
                        }

                        roleUserByIdentifier[identifier] = user;
                    }

                    if (await membershipRepository.FindAsync(user.Id, tenant.Id, token) is not null)
                    {
                        throw new DomainException(IdentityDomainErrorCodes.IdentifierAlreadyExists);
                    }

                    memberships.Add(new TenantMembershipEntity
                    {
                        TenantId = tenant.Id,
                        AppId = appId,
                        UserId = user.Id,
                        Status = CMembershipStatus.Active,
                        CreatorId = actorId,
                        CreatedAt = createdAt
                    });
                    assignments.Add(new TenantAdminAssignmentEntity
                    {
                        TenantId = tenant.Id,
                        UserId = user.Id,
                        IsActive = true,
                        CreatorId = actorId,
                        CreatedAt = createdAt
                    });
                }

                await membershipEntities.BulkInsertAsync(memberships, token);
                await tenantAdminEntities.BulkInsertAsync(assignments, token);
            }, cancellationToken);

            return tenants.Select(tenant => tenant.Id).ToArray();
        }

        /// <inheritdoc />
        public async Task<TenantDto> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var appId = await EnsureAppAdminAsync(cancellationToken);
            if (id == Guid.Empty)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }

            var tenant = await entityRepository.GetAsync(id, cancellationToken);
            if (tenant is null || tenant.AppId != appId)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }

            return ToDto(tenant);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<TenantDto>> GetManyAsync(
            IReadOnlyCollection<Guid> ids,
            CancellationToken cancellationToken = default)
        {
            ValidateIds(ids);
            var appId = await EnsureAppAdminAsync(cancellationToken);
            var tenants = await entityRepository.GetManyAsync(ids, cancellationToken);
            if (tenants.Count != ids.Count || tenants.Any(tenant => tenant.AppId != appId))
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }

            var byId = tenants.ToDictionary(tenant => tenant.Id);
            return ids.Select(id => ToDto(byId[id])).ToArray();
        }

        /// <inheritdoc />
        public async Task<PagedResultDto<TenantDto>> GetPagedAsync(
            PagedAndSortedResultRequestDto request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);
            var appId = await EnsureAppAdminAsync(cancellationToken);
            Func<IQueryable<TenantEntity>, IOrderedQueryable<TenantEntity>> orderBy =
                request.Sorting?.Trim().ToLowerInvariant() switch
                {
                    "name" => query => query.OrderBy(tenant => tenant.Name).ThenBy(tenant => tenant.Id),
                    "name desc" => query => query.OrderByDescending(tenant => tenant.Name).ThenBy(tenant => tenant.Id),
                    "code" => query => query.OrderBy(tenant => tenant.Code).ThenBy(tenant => tenant.Id),
                    "code desc" => query => query.OrderByDescending(tenant => tenant.Code).ThenBy(tenant => tenant.Id),
                    "masterstatus" => query => query.OrderBy(tenant => tenant.MasterStatus).ThenBy(tenant => tenant.Id),
                    "masterstatus desc" => query => query.OrderByDescending(tenant => tenant.MasterStatus)
                        .ThenBy(tenant => tenant.Id),
                    "createdat" => query => query.OrderBy(tenant => tenant.CreatedAt).ThenBy(tenant => tenant.Id),
                    "createdat desc" => query => query.OrderByDescending(tenant => tenant.CreatedAt)
                        .ThenBy(tenant => tenant.Id),
                    _ => query => query.OrderBy(tenant => tenant.Id)
                };
            var search = request.Filter?.Trim();
            Expression<Func<TenantEntity, bool>> filter = search is { Length: > 0 }
                ? tenant => tenant.AppId == appId &&
                            (tenant.Name.Contains(search) || tenant.Code.Contains(search))
                : tenant => tenant.AppId == appId;

            var page = await entityRepository.GetPagedAsync(request, filter, orderBy, cancellationToken);
            return new PagedResultDto<TenantDto>(page.TotalCount, page.Items.Select(ToDto).ToArray());
        }

        /// <inheritdoc />
        public async Task<TenantDto> UpdateAsync(
            Guid id,
            UpdateTenantDto input,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            var appId = await EnsureAppAdminAsync(cancellationToken);
            if (id == Guid.Empty)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }

            var tenant = await entityRepository.GetAsync(id, cancellationToken);
            if (tenant is null || tenant.AppId != appId)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }

            ApplyUpdate(tenant, input.Name, input.MasterStatus);
            tenant.ModifierId = currentContext.UserId;
            tenant.ModifiedAt = timeProvider.GetUtcNow();
            await unitOfWork.ExecuteAsync(token => entityRepository.UpdateAsync(tenant, token), cancellationToken);
            return ToDto(tenant);
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<TenantDto>> UpdateManyAsync(
            IReadOnlyCollection<UpdateTenantItemDto> inputs,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(inputs);
            ValidateIds(inputs.Select(input => input?.Id ?? Guid.Empty).ToArray());
            var appId = await EnsureAppAdminAsync(cancellationToken);
            var ids = inputs.Select(input => input.Id).ToArray();
            var tenants = await entityRepository.GetManyAsync(ids, cancellationToken);
            if (tenants.Count != ids.Length || tenants.Any(tenant => tenant.AppId != appId))
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }

            var byId = tenants.ToDictionary(tenant => tenant.Id);
            foreach (var input in inputs)
            {
                ValidateUpdate(input.Name, input.MasterStatus);
            }

            var modifiedAt = timeProvider.GetUtcNow();
            foreach (var input in inputs)
            {
                var tenant = byId[input.Id];
                ApplyUpdate(tenant, input.Name, input.MasterStatus);
                tenant.ModifierId = currentContext.UserId;
                tenant.ModifiedAt = modifiedAt;
            }

            await unitOfWork.ExecuteAsync(
                token => entityRepository.BulkUpdateAsync(tenants, token),
                cancellationToken);
            return ids.Select(id => ToDto(byId[id])).ToArray();
        }

        /// <inheritdoc />
        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
            DeleteManyAsync([id], cancellationToken);

        /// <inheritdoc />
        public async Task DeleteManyAsync(
            IReadOnlyCollection<Guid> ids,
            CancellationToken cancellationToken = default)
        {
            ValidateIds(ids);
            var appId = await EnsureAppAdminAsync(cancellationToken);
            var tenants = await entityRepository.GetManyAsync(ids, cancellationToken);
            if (tenants.Count != ids.Count || tenants.Any(tenant => tenant.AppId != appId))
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }

            await unitOfWork.ExecuteAsync(
                token => entityRepository.BulkSoftDeleteAsync(
                    ids,
                    currentContext.UserId,
                    timeProvider.GetUtcNow(),
                    token),
                cancellationToken);
        }

        private async Task<Guid> EnsureAppAdminAsync(CancellationToken cancellationToken)
        {
            if (currentContext.AppId is not { } appId ||
                currentContext.UserId is not { } actorId ||
                !await authorizationRepository.IsAppAdminAsync(actorId, appId, cancellationToken))
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }

            return appId;
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

        private static void ApplyUpdate(TenantEntity tenant, string name, int masterStatus)
        {
            ValidateUpdate(name, masterStatus);
            tenant.Name = name.Trim();
            tenant.MasterStatus = (CMasterStatus)masterStatus;
        }

        private static void ValidateUpdate(string name, int masterStatus)
        {
            if (string.IsNullOrWhiteSpace(name) ||
                !Enum.IsDefined((CMasterStatus)masterStatus) ||
                masterStatus == (int)CMasterStatus.None)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidInput);
            }
        }

        private static TenantDto ToDto(TenantEntity tenant) => new()
        {
            Id = tenant.Id,
            AppId = tenant.AppId,
            Name = tenant.Name,
            Code = tenant.Code,
            MasterStatus = (int)tenant.MasterStatus
        };
    }
}
