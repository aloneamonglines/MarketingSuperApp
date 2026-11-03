namespace MKS.Shared.Domain.Common
{
    public interface IHasDeleterEntity
    {
        Guid? DeleterId { get; set; }
        DateTimeOffset? DeletedAt { get; set; }
        bool IsDeleted { get; set; }
    }
}