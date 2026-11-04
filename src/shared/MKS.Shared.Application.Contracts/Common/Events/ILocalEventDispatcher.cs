namespace MKS.Shared.Application.Contracts.Common.Events
{
    /// <summary>Dispatches local events to all registered handlers of their runtime event type.</summary>
    public interface ILocalEventDispatcher
    {
        Task DispatchAsync(object localEvent, CancellationToken cancellationToken = default);
    }
}
