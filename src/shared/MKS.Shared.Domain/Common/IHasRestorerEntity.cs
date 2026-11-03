namespace MKS.Shared.Domain.Common
{
    public interface IHasRestorerEntity
    {
        Guid? RestorerId { get; set; }
        DateTimeOffset? RestoredAt { get; set; }
    }
}