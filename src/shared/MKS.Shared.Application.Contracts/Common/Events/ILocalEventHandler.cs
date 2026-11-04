namespace MKS.Shared.Application.Contracts.Common.Events
{
    /// <summary>Handles one application-local domain event.</summary>
    public interface ILocalEventHandler
    {
        Type EventType { get; }
        Task HandleAsync(object localEvent, CancellationToken cancellationToken = default);
    }

    /// <summary>Handles one strongly typed application-local domain event.</summary>
    public interface ILocalEventHandler<in TEvent> : ILocalEventHandler
        where TEvent : class
    {
        Task HandleAsync(TEvent localEvent, CancellationToken cancellationToken = default);

        Type ILocalEventHandler.EventType => typeof(TEvent);

        Task ILocalEventHandler.HandleAsync(object localEvent, CancellationToken cancellationToken) =>
            localEvent is TEvent typedEvent
                ? HandleAsync(typedEvent, cancellationToken)
                : throw new ArgumentException(
                    $"Expected local event of type '{typeof(TEvent)}'.",
                    nameof(localEvent));
    }
}
