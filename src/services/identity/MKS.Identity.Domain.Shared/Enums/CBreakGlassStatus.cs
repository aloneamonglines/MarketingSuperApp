namespace MKS.Identity.Domain.Shared.Enums
{
    /// <summary>
    /// Describes the lifecycle state of a break-glass authorization.
    /// </summary>
    public enum CBreakGlassStatus
    {
        /// <summary>Grant is active until its expiration.</summary>
        Active = 1,
        /// <summary>Grant expired.</summary>
        Expired = 2,
        /// <summary>Grant was revoked.</summary>
        Revoked = 3
    }
}
