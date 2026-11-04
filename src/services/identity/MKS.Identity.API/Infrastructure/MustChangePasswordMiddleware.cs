using MKS.Identity.Domain.Shared.Constants;

namespace MKS.Identity.API.Infrastructure
{
    /// <summary>
    /// Restricts temporary-password accounts to the password change workflow.
    /// </summary>
    public sealed class MustChangePasswordMiddleware(RequestDelegate next)
    {
        /// <summary>Invokes the next middleware when the account may access the requested endpoint.</summary>
        public async Task InvokeAsync(HttpContext context)
        {
            var mustChangePassword =
                context.User.Identity?.IsAuthenticated == true &&
                string.Equals(
                    context.User.FindFirst(IdentityConstants.Claims.MustChangePassword)?.Value,
                    "true",
                    StringComparison.OrdinalIgnoreCase);
            var isPasswordChange = context.Request.Path.Equals(
                IdentityConstants.ApiPaths.ChangePassword,
                StringComparison.OrdinalIgnoreCase);
            if (mustChangePassword && !isPasswordChange)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = IdentityDomainErrorCodes.MustChangePassword
                });
                return;
            }

            await next(context);
        }
    }
}
