using System.Security.Claims;
using Microsoft.Extensions.Localization;
using MKS.Identity.Application.Contracts.Users.DTOs;
using MKS.Identity.Application.Contracts.Users.Interfaces;
using MKS.Identity.Domain.Shared.Constants;
using MKS.Identity.Domain.Shared.Localization;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server;

namespace MKS.Identity.API.Infrastructure
{
    /// <summary>
    /// Handles the OpenIddict password grant through the Identity application workflow.
    /// </summary>
    public sealed class PasswordGrantHandler(
        IAuthenticationFlowService authenticationFlow,
        IStringLocalizer<IdentityResource> localizer)
        : IOpenIddictServerHandler<OpenIddictServerEvents.HandleTokenRequestContext>
    {
        /// <inheritdoc />
        public async ValueTask HandleAsync(OpenIddictServerEvents.HandleTokenRequestContext context)
        {
            var isPasswordGrant = context.Request.IsPasswordGrantType();
            var isBreakGlassGrant = string.Equals(
                context.Request.GrantType,
                IdentityConstants.BreakGlassGrantType,
                StringComparison.Ordinal);
            if (!isPasswordGrant && !isBreakGlassGrant)
            {
                return;
            }

            var request = context.Request;
            if (isBreakGlassGrant && request.GetParameter("app_id") is not null)
            {
                context.Reject(
                    error: OpenIddictConstants.Errors.InvalidRequest,
                    description: localizer[IdentityProtocolMessages.AppScopeNotAllowedForBreakGlass].Value);
                return;
            }

            var appParameter = request.GetParameter("app_id").ToString();
            Guid? appId = null;
            if (!string.IsNullOrWhiteSpace(appParameter))
            {
                if (!Guid.TryParse(appParameter, out var parsedAppId) || parsedAppId == Guid.Empty)
                {
                    context.Reject(
                        error: OpenIddictConstants.Errors.InvalidRequest,
                        description: localizer[IdentityProtocolMessages.InvalidAppContext].Value);
                    return;
                }

                appId = parsedAppId;
            }

            if (request.Username is null ||
                request.Password is null)
            {
                context.Reject(
                    error: OpenIddictConstants.Errors.InvalidRequest,
                    description: localizer[IdentityProtocolMessages.IncompleteSignInRequest].Value);
                return;
            }

            Guid? tenantId = null;
            var tenantParameter = request.GetParameter("tenant_id").ToString();
            if (!string.IsNullOrWhiteSpace(tenantParameter))
            {
                if (!Guid.TryParse(tenantParameter, out var parsedTenantId) || parsedTenantId == Guid.Empty)
                {
                    context.Reject(
                        error: OpenIddictConstants.Errors.InvalidRequest,
                        description: localizer[IdentityProtocolMessages.InvalidTenantContext].Value);
                    return;
                }

                tenantId = parsedTenantId;
            }
            if (isBreakGlassGrant && tenantId is null)
            {
                context.Reject(
                    error: OpenIddictConstants.Errors.InvalidRequest,
                    description: localizer[IdentityProtocolMessages.TenantRequiredForBreakGlass].Value);
                return;
            }

            Guid? grantId = null;
            var grantParameter = request.GetParameter("grant_id").ToString();
            if (isBreakGlassGrant)
            {
                if (!Guid.TryParse(grantParameter, out var parsedGrantId) || parsedGrantId == Guid.Empty)
                {
                    context.Reject(
                        error: OpenIddictConstants.Errors.InvalidRequest,
                        description: localizer[IdentityProtocolMessages.InvalidBreakGlassGrant].Value);
                    return;
                }

                grantId = parsedGrantId;
            }
            else if (!string.IsNullOrWhiteSpace(grantParameter))
            {
                context.Reject(
                    error: OpenIddictConstants.Errors.InvalidRequest,
                    description: localizer[IdentityProtocolMessages.BreakGlassGrantOnlyForCustomGrant].Value);
                return;
            }

            try
            {
                var input = new AuthenticateUserDto(
                    request.Username,
                    request.Password,
                    appId,
                    tenantId,
                    request.GetParameter("device_name").ToString());
                var authenticatedUser = grantId is { } activeGrantId
                    ? await authenticationFlow.AuthenticateBreakGlassAsync(input, activeGrantId)
                    : await authenticationFlow.AuthenticateAsync(input);
                var identity = new ClaimsIdentity(
                    TokenValidationParameters.DefaultAuthenticationType,
                    OpenIddictConstants.Claims.Name,
                    OpenIddictConstants.Claims.Role);
                identity.AddClaim(new Claim(OpenIddictConstants.Claims.Subject, authenticatedUser.UserId.ToString()));
                identity.AddClaim(new Claim(IdentityConstants.Claims.SessionId, authenticatedUser.SessionId.ToString()));
                if (authenticatedUser.AppId is { } authenticatedAppId)
                {
                    identity.AddClaim(new Claim(IdentityConstants.Claims.AppId, authenticatedAppId.ToString()));
                }
                if (authenticatedUser.TenantId is { } activeTenantId)
                {
                    identity.AddClaim(new Claim(IdentityConstants.Claims.TenantId, activeTenantId.ToString()));
                }
                if (authenticatedUser.BreakGlassGrantId is { } issuedGrantId)
                {
                    identity.AddClaim(new Claim(IdentityConstants.Claims.BreakGlassGrantId, issuedGrantId.ToString()));
                }

                identity.AddClaim(new Claim(
                    IdentityConstants.Claims.MustChangePassword,
                    authenticatedUser.MustChangePassword ? "true" : "false"));
                identity.AddClaim(new Claim(
                    IdentityConstants.Claims.MembershipVersion,
                    authenticatedUser.MembershipVersion.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                identity.AddClaim(new Claim(IdentityConstants.Claims.RefreshGeneration, "0"));

                var principal = new ClaimsPrincipal(identity);
                principal.SetScopes(request.GetScopes());
                principal.SetResources(IdentityConstants.Resources.Identity, IdentityConstants.Resources.Gateway);
                foreach (var claim in principal.Claims)
                {
                    claim.SetDestinations(OpenIddictConstants.Destinations.AccessToken);
                }

                context.SignIn(principal);
            }
            catch (MKS.Shared.Domain.Shared.Common.DomainException)
            {
                context.Reject(
                    error: OpenIddictConstants.Errors.InvalidGrant,
                    description: localizer[IdentityProtocolMessages.InvalidCredentialsOrAccessContext].Value);
            }
        }
    }
}
