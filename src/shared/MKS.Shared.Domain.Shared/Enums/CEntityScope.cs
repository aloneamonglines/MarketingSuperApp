namespace MKS.Shared.Domain.Shared.Enums
{
    /// <summary>
    /// Defines the data isolation scope of an entity.
    /// </summary>
    public enum CEntityScope
    {
        /// <summary>
        /// The entity is shared across the platform.
        /// </summary>
        Global = 0,
        /// <summary>
        /// The entity belongs to an application.
        /// </summary>
        App = 1,
        /// <summary>
        /// The entity belongs to a tenant.
        /// </summary>
        Tenant = 2
    }
}
