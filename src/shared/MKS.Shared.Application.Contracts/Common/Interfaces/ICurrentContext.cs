namespace MKS.Shared.Application.Contracts.Common.Interfaces
{
    /// <summary>
    /// Exposes identifiers from the authenticated and validated request context.
    /// </summary>
    public interface ICurrentContext
    {
        /// <summary>Gets the authenticated user identifier.</summary>
        Guid? UserId { get; }
        /// <summary>Gets the authenticated session identifier.</summary>
        Guid? SessionId { get; }
        /// <summary>Gets the active tenant identifier.</summary>
        Guid? TenantId { get; }
        /// <summary>Gets the active application identifier.</summary>
        Guid? AppId { get; }
        /// <summary>Gets the impersonated user identifier.</summary>
        Guid? ImpersonatorId { get; }
        /// <summary>Gets the active break-glass grant identifier.</summary>
        Guid? BreakGlassGrantId { get; }
    }
}
