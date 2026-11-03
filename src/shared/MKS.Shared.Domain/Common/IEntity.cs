namespace MKS.Shared.Domain.Common
{
    /// <summary>
    /// Represents a base entity with a unique identifier.
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// The unique identifier for the entity.
        /// </summary>
        Guid Id { get; set;}
    }
}