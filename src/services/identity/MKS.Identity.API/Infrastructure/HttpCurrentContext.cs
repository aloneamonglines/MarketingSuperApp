using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using MKS.Identity.Domain.Shared.Constants;
using MKS.Shared.Application.Contracts.Common.Interfaces;

namespace MKS.Identity.API.Infrastructure
{
    /// <summary>
    /// Reads identity and scope only from claims validated by the authentication middleware.
    /// </summary>
    public sealed class HttpCurrentContext(IHttpContextAccessor httpContextAccessor) : ICurrentContext
    {
        private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;

        /// <inheritdoc />
        public Guid? UserId => ReadGuid(ClaimTypes.NameIdentifier, IdentityConstants.Claims.Subject);
        /// <inheritdoc />
        public Guid? SessionId => ReadGuid(IdentityConstants.Claims.SessionId);
        /// <inheritdoc />
        public Guid? TenantId => ReadGuid(IdentityConstants.Claims.TenantId);
        /// <inheritdoc />
        public Guid? AppId => ReadGuid(IdentityConstants.Claims.AppId);
        /// <inheritdoc />
        public Guid? ImpersonatorId => ReadGuid(IdentityConstants.Claims.ImpersonatorId);
        /// <inheritdoc />
        public Guid? BreakGlassGrantId => ReadGuid(IdentityConstants.Claims.BreakGlassGrantId);

        private Guid? ReadGuid(params string[] claimTypes)
        {
            foreach (var claimType in claimTypes)
            {
                var value = Principal?.FindFirst(claimType)?.Value;
                if (Guid.TryParse(value, out var identifier) && identifier != Guid.Empty)
                {
                    return identifier;
                }
            }

            return null;
        }
    }
}
