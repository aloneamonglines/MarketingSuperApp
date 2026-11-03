namespace MKS.Shared.Domain.Shared.Enums
{
    public enum CMasterStatus
    {
        /// <summary>
        /// No status has been assigned
        /// </summary>
        None = 0,
        /// <summary>
        /// The master record is active and can be used in the system
        /// </summary>
        Active = 1,
        /// <summary>
        /// The master record is inactive and cannot be used in the system
        /// </summary>
        Inactive = 2
    }
}