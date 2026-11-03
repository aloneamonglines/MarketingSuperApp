namespace MKS.Shared.Domain.Common
{
    public interface IHasCreatorEntity
    {
        Guid? CreatorId { get; set; }
        DateTimeOffset CreatedAt { get; set; }
    }
}