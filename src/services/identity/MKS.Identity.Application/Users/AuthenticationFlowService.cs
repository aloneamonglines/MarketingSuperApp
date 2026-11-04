using MKS.Identity.Domain.Shared.Constants;
using MKS.Identity.Application.Contracts.Common.Interfaces;
using MKS.Identity.Application.Contracts.Users.DTOs;
using MKS.Identity.Application.Contracts.Users.Interfaces;
using MKS.Identity.Domain.Apps.Repositories;
using MKS.Identity.Domain.Authorization.Repositories;
using MKS.Identity.Domain.Platform.Entities;
using MKS.Identity.Domain.Platform.Services;
using MKS.Identity.Domain.MultiTenancy.Entities;
using MKS.Identity.Domain.MultiTenancy.Repositories;
using MKS.Identity.Domain.Shared.Enums;
using MKS.Identity.Domain.Users.Entities;
using MKS.Identity.Domain.Users.Repositories;
using MKS.Identity.Domain.Users.Services;
using MKS.Identity.Domain.Apps.Entities;
using MKS.Shared.Domain.Apps.Repositories;
using MKS.Shared.Domain.MultiTenancy.Repositories;
using MKS.Shared.Domain.Shared.Common;
using MKS.Shared.Domain.Shared.Enums;
using Microsoft.Extensions.Options;
using MKS.Identity.Application.Contracts.Common.Options;
using MKS.Shared.Application.Contracts.Common.Interfaces;

namespace MKS.Identity.Application.Users
{
    /// <summary>
    /// Verifies credentials and creates revocable application or tenant sessions.
    /// </summary>
    public sealed class AuthenticationFlowService(
        IUserRepository userRepository,
        IUserSessionRepository sessionRepository,
        IAppRepository<AppEntity> appRepository,
        ITenantRepository<TenantEntity> tenantRepository,
        ITenantMembershipRepository membershipRepository,
        IAuthorizationRepository authorizationRepository,
        IIdentityUnitOfWork unitOfWork,
        IPasswordService passwordService,
        ITenantScopeAccessor tenantScope,
        BreakGlassService breakGlassService,
        IOptions<IdentityWorkflowOptions> options,
        TimeProvider timeProvider) : IAuthenticationFlowService
    {
        /// <inheritdoc />
        public async Task<AuthenticatedUserDto> AuthenticateAsync(
            AuthenticateUserDto input,
            CancellationToken cancellationToken = default)
        {
            return await AuthenticateCoreAsync(input, null, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<AuthenticatedUserDto> AuthenticateBreakGlassAsync(
            AuthenticateUserDto input,
            Guid grantId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(input);
            if (grantId == Guid.Empty || input.AppId is not null || input.TenantId is null)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }

            return await AuthenticateCoreAsync(input, grantId, cancellationToken);
        }

        private async Task<AuthenticatedUserDto> AuthenticateCoreAsync(
            AuthenticateUserDto input,
            Guid? breakGlassGrantId,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(input);
            if (input.AppId == Guid.Empty ||
                breakGlassGrantId is null && input.TenantId is not null && input.AppId is null)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidCredentials);
            }

            var identifierType = GetIdentifierType(input.Identifier);
            var normalizedIdentifier = IdentifierNormalizer.Normalize(input.Identifier, identifierType);
            var login = await userRepository.FindForSignInAsync(normalizedIdentifier, cancellationToken);
            if (login is null ||
                login.Value.UserEntity.MasterStatus != CMasterStatus.Active)
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidCredentials);
            }

            var (user, _, credential) = login.Value;
            var now = timeProvider.GetUtcNow();
            if (credential.LockedUntil is { } lockedUntil && lockedUntil > now)
            {
                throw new DomainException(IdentityDomainErrorCodes.AccountLocked);
            }

            if (!passwordService.Verify(user.Id, credential.PasswordHash, input.Password))
            {
                credential.RecordFailedAttempt(
                    now,
                    options.Value.FailedSignInThreshold,
                    options.Value.LockoutDuration);
                await unitOfWork.ExecuteAsync(
                    token => userRepository.UpdateAsync(user, credential, token),
                    cancellationToken);
                throw new DomainException(
                    credential.LockedUntil is not null
                        ? IdentityDomainErrorCodes.AccountLocked
                        : IdentityDomainErrorCodes.InvalidCredentials);
            }

            var app = input.AppId is { } appId
                ? await appRepository.GetAsync(appId, cancellationToken)
                : null;
            if (input.AppId is not null && (app is null || app.MasterStatus != CMasterStatus.Active))
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }

            var effectiveTenantId = input.TenantId;
            if (breakGlassGrantId is { } grantId && effectiveTenantId is { } grantTenantId)
            {
                using var scope = tenantScope.Enter(grantTenantId);
                var grant = await breakGlassService.RequireActiveAsync(grantId, grantTenantId, cancellationToken);
                if (grant.UserId != user.Id || !await IsSuperAdminAsync(user.Id, cancellationToken))
                {
                    throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
                }

                var tenant = await tenantRepository.GetAsync(grantTenantId, cancellationToken);
                if (tenant is null || tenant.MasterStatus != CMasterStatus.Active)
                {
                    throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
                }

                app = await appRepository.GetAsync(tenant.AppId, cancellationToken);
                if (app is null || app.MasterStatus != CMasterStatus.Active)
                {
                    throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
                }
            }
            else if (input.TenantId is { } tenantId)
            {
                using var scope = tenantScope.Enter(tenantId);
                await unitOfWork.ExecuteAsync(async token =>
                {
                    var tenant = await tenantRepository.GetAsync(tenantId, token);
                    var membership = await membershipRepository.FindAsync(user.Id, tenantId, token);
                    var isAppAdmin = await authorizationRepository.IsAppAdminAsync(user.Id, app!.Id, token);
                    if (tenant is null ||
                        tenant.AppId != app!.Id ||
                        tenant.MasterStatus != CMasterStatus.Active ||
                        membership?.Status != CMembershipStatus.Active && !isAppAdmin)
                    {
                        throw new DomainException(IdentityDomainErrorCodes.InvalidCredentials);
                    }
                }, cancellationToken);
            }
            else if (app is not null &&
                     !await authorizationRepository.IsAppAdminAsync(user.Id, app.Id, cancellationToken) &&
                     !await IsSuperAdminAsync(user.Id, cancellationToken))
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }
            else if (app is null && !await IsSuperAdminAsync(user.Id, cancellationToken))
            {
                throw new DomainException(IdentityDomainErrorCodes.InvalidScope);
            }

            credential.RecordSuccessfulSignIn();
            var session = new UserSessionEntity
            {
                UserId = user.Id,
                AppId = app?.Id,
                TenantId = effectiveTenantId,
                BreakGlassGrantId = breakGlassGrantId,
                DeviceName = input.DeviceName?.Trim(),
                LastSeenAt = now
            };
            await unitOfWork.ExecuteAsync(async token =>
            {
                await userRepository.UpdateAsync(user, credential, token);
                await sessionRepository.AddAsync(session, token);
            }, cancellationToken);

            return new AuthenticatedUserDto(
                user.Id,
                session.Id,
                app?.Id,
                effectiveTenantId,
                credential.MustChangePassword,
                user.MembershipVersion,
                breakGlassGrantId);
        }

        private async Task<bool> IsSuperAdminAsync(Guid userId, CancellationToken cancellationToken)
        {
            var permissions = await authorizationRepository.GetPlatformPermissionsAsync(userId, cancellationToken);
            return permissions.Contains(IdentityConstants.Permissions.PlatformSuperAdminWildcard);
        }

        private static CIdentifierType GetIdentifierType(string identifier)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(identifier);
            if (identifier.Contains('@', StringComparison.Ordinal))
            {
                return CIdentifierType.Email;
            }

            if (identifier.TrimStart().StartsWith("+", StringComparison.Ordinal))
            {
                return CIdentifierType.Phone;
            }

            return CIdentifierType.Username;
        }
    }
}
