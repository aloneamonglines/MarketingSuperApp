namespace MKS.Shared.Domain.Shared.Enums
{
    /// <summary>Describes a persisted change made to an entity.</summary>
    public enum CEntityChangeType
    {
        Create = 0,
        Update = 1,
        SoftDelete = 2,
        Restore = 3,
        HardDelete = 4
    }
}
