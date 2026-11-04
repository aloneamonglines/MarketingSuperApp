namespace MKS.Shared.Domain.Shared.Common.Events
{
    public interface IHasDistributedEvent
    {
        IReadOnlyCollection<IIntegrationEvent> DistributedEvents { get; }
    }
}