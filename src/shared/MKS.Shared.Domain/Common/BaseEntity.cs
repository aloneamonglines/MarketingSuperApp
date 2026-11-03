namespace MKS.Shared.Domain.Common
{
    public abstract class BaseEntity : IEntity
    {
        public virtual Guid Id { get; set; } = Guid.CreateVersion7(timestamp: DateTimeOffset.UtcNow);
    }
}