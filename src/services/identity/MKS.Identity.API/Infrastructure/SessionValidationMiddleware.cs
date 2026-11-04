using MKS.Identity.Domain.Users.Repositories;
using MKS.Identity.Application.Contracts.Common.Interfaces;
using MKS.Identity.Domain.Platform.Services;
using MKS.Identity.Domain.Shared.Constants;
using MKS.Shared.Application.Contracts.Common.Interfaces;
using MKS.Shared.Domain.Shared.Common;

namespace MKS.Identity.API.Infrastructure
{
    /// <summary>
    /// Rejects access tokens whose server-side session has been revoked.
    /// </summary>
    public sealed class SessionValidationMiddleware(RequestDelegate next)
    {
        /// <summary>Validates an authenticated session before dispatching the request.</summary>
        public async Task InvokeAsync(
            HttpContext context,
            IUserSessionRepository sessions,
            BreakGlassService breakGlassService,
            ISecurityAuditLogWriter securityAuditLogWriter,
            IIdentityUnitOfWork unitOfWork,
            TimeProvider timeProvider)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userIdValue = context.User.FindFirst(IdentityConstants.Claims.Subject)?.Value;
                var sessionIdValue = context.User.FindFirst(IdentityConstants.Claims.SessionId)?.Value;
                var tenantIdValue = context.User.FindFirst(IdentityConstants.Claims.TenantId)?.Value;
                if (!Guid.TryParse(userIdValue, out var userId) ||
                    !Guid.TryParse(sessionIdValue, out var sessionId) ||
                    await sessions.GetActiveAsync(userId, sessionId, context.RequestAborted) is not { } session)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return;
                }

                var grantValue = context.User.FindFirst(IdentityConstants.Claims.BreakGlassGrantId)?.Value;
                if (!string.IsNullOrWhiteSpace(grantValue))
                {
                    if (!Guid.TryParse(grantValue, out var grantId) ||
                        !Guid.TryParse(tenantIdValue, out var tenantId) ||
                        session.TenantId != tenantId ||
                        session.BreakGlassGrantId != grantId)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return;
                    }

                    try
                    {
                        var grant = await breakGlassService.RequireActiveAsync(
                            grantId,
                            tenantId,
                            context.RequestAborted);
                        if (grant.UserId != userId || !grant.ReadOnly)
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            return;
                        }

                        if (!HttpMethods.IsGet(context.Request.Method) &&
                            !HttpMethods.IsHead(context.Request.Method) &&
                            !HttpMethods.IsOptions(context.Request.Method))
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            return;
                        }

                        await next(context);
                    }
                    catch (DomainException)
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    }
                    finally
                    {
                        await unitOfWork.ExecuteAsync(
                            token => securityAuditLogWriter.AppendAsync(
                                userId,
                                context.User.FindFirst(IdentityConstants.Claims.ImpersonatorId) is { } impersonatorClaim &&
                                    Guid.TryParse(impersonatorClaim.Value, out var impersonatorId)
                                        ? impersonatorId
                                        : null,
                                grantId,
                                IdentityConstants.AuditActions.BreakGlassRequest,
                                IdentityConstants.EntityTypes.HttpRequest,
                                null,
                                $"Method={context.Request.Method}; Path={context.Request.Path}; StatusCode={context.Response.StatusCode}",
                                timeProvider.GetUtcNow(),
                                token),
                            CancellationToken.None);
                    }

                    return;
                }
            }

            await next(context);
        }
    }
}
