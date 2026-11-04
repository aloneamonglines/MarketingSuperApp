using Microsoft.Extensions.Localization;
using MKS.Identity.Domain.Shared.Localization;
using MKS.Identity.Domain.Platform.Services;
using MKS.Identity.Domain.Shared.Constants;
using MKS.Identity.Domain.Users.Repositories;
using MKS.Shared.Domain.Shared.Common;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using System.Security.Claims;

namespace MKS.Identity.API.Infrastructure
{
    /// <summary>
    /// Validates active sessions, rejects refresh-token replay, and enforces break-glass expiry.
    /// </summary>
    public sealed class RefreshTokenSessionHandler(
        IUserSessionRepository sessions,
        BreakGlassService breakGlassService,
        IStringLocalizer<IdentityResource> localizer)
        : IOpenIddictServerHandler<OpenIddictServerEvents.ProcessSignInContext>
    {
        /// <inheritdoc />
        public async ValueTask HandleAsync(OpenIddictServerEvents.ProcessSignInContext context)
        {
            if (!context.Transaction.Request!.IsRefreshTokenGrantType())
            {
                return;
            }

            var principal = context.Principal;
            var subject = principal?.GetClaim(OpenIddictConstants.Claims.Subject);
            var sessionValue = principal?.GetClaim(IdentityConstants.Claims.SessionId);
            if (!Guid.TryParse(subject, out var userId) ||
                !Guid.TryParse(sessionValue, out var sessionId) ||
                await sessions.GetActiveAsync(userId, sessionId, context.CancellationToken) is not { } session)
            {
                context.Reject(
                    error: OpenIddictConstants.Errors.InvalidGrant,
                    description: localizer[IdentityProtocolMessages.InactiveSignInSession].Value);
                return;
            }

            var refreshPrincipal = principal
                ?? throw new InvalidOperationException(
                    IdentityProtocolMessages.RefreshTokenPrincipalRequired);
            var appId = ReadGuidClaim(refreshPrincipal, IdentityConstants.Claims.AppId);
            var tenantId = ReadGuidClaim(refreshPrincipal, IdentityConstants.Claims.TenantId);
            var grantId = ReadGuidClaim(refreshPrincipal, IdentityConstants.Claims.BreakGlassGrantId);
            if (session.AppId != appId ||
                session.TenantId != tenantId ||
                session.BreakGlassGrantId != grantId)
            {
                context.Reject(
                    error: OpenIddictConstants.Errors.InvalidGrant,
                    description: localizer[IdentityProtocolMessages.RefreshTokenScopeMismatch].Value);
                return;
            }

            if (!long.TryParse(
                    refreshPrincipal.GetClaim(IdentityConstants.Claims.RefreshGeneration),
                    System.Globalization.NumberStyles.None,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var generation) ||
                !await sessions.TryAdvanceRefreshGenerationAsync(
                    userId,
                    sessionId,
                    generation,
                    context.CancellationToken))
            {
                context.Reject(
                    error: OpenIddictConstants.Errors.InvalidGrant,
                    description: localizer[IdentityProtocolMessages.RefreshTokenAlreadyUsed].Value);
                return;
            }

            var identity = refreshPrincipal.Identity as ClaimsIdentity
                ?? throw new InvalidOperationException(
                    IdentityProtocolMessages.RefreshTokenIdentityRequired);
            var previousGenerationClaim = identity.FindFirst(IdentityConstants.Claims.RefreshGeneration);
            if (previousGenerationClaim is not null)
            {
                identity.RemoveClaim(previousGenerationClaim);
            }

            var nextGenerationClaim = new Claim(
                IdentityConstants.Claims.RefreshGeneration,
                (generation + 1).ToString(System.Globalization.CultureInfo.InvariantCulture));
            nextGenerationClaim.SetDestinations(OpenIddictConstants.Destinations.AccessToken);
            identity.AddClaim(nextGenerationClaim);

            if (grantId is { } activeGrantId)
            {
                try
                {
                    var grant = await breakGlassService.RequireActiveAsync(
                        activeGrantId,
                        tenantId ?? Guid.Empty,
                        context.CancellationToken);
                    if (grant.UserId != userId || !grant.ReadOnly)
                    {
                        context.Reject(
                            error: OpenIddictConstants.Errors.InvalidGrant,
                            description: localizer[IdentityProtocolMessages.InactiveBreakGlassGrant].Value);
                    }
                }
                catch (DomainException)
                {
                    context.Reject(
                        error: OpenIddictConstants.Errors.InvalidGrant,
                        description: localizer[IdentityProtocolMessages.InactiveBreakGlassGrant].Value);
                }
            }
        }

        private static Guid? ReadGuidClaim(System.Security.Claims.ClaimsPrincipal principal, string claimType) =>
            Guid.TryParse(principal.GetClaim(claimType), out var value) && value != Guid.Empty
                ? value
                : null;
    }
}
