namespace MKS.Shared.Domain.Apps.Entities
{
    public interface IHasAppEntity
    {
        Guid AppId { get; set; }
    }

    public interface IMayHaveAppEntity
    {
        Guid? AppId { get; set; }
    }
}