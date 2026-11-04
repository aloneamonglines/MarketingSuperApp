namespace MKS.Identity.Domain.Shared.Enums
{
    /// <summary>
    /// Describes the lifecycle state of a tenant membership.
    /// </summary>
    public enum CMembershipStatus
    {
        /// <summary>Invalid or unset status.</summary>
        None = 0,
        /// <summary>Membership is active.</summary>
        Active = 1,
        /// <summary>Membership is temporarily suspended.</summary>
        Suspended = 2,
        /// <summary>Membership was removed.</summary>
        Removed = 3
    }
}
