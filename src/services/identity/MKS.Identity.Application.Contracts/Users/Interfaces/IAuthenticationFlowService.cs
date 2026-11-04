using MKS.Identity.Application.Contracts.Users.DTOs;
using MKS.Shared.Application.Contracts.Common.Interfaces;

namespace MKS.Identity.Application.Contracts.Users.Interfaces
{
    /// <summary>
    /// Authenticates credentials and validates the requested application or tenant context.
    /// </summary>
    public interface IAuthenticationFlowService : IApplicationService
    {
        /// <summary>Authenticates credentials and creates a revocable sign-in session.</summary>
        Task<AuthenticatedUserDto> AuthenticateAsync(
            AuthenticateUserDto input,
            CancellationToken cancellationToken = default);

        /// <summary>Authenticates a platform administrator against an active break-glass grant.</summary>
        Task<AuthenticatedUserDto> AuthenticateBreakGlassAsync(
            AuthenticateUserDto input,
            Guid grantId,
            CancellationToken cancellationToken = default);
    }
}
