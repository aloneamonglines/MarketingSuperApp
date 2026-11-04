using MKS.Shared.Application.Contracts.Common.Events;

namespace MKS.Shared.Application.Events
{
    /// <summary>Dispatches an event to all matching handlers registered in the current scope.</summary>
    public sealed class LocalEventDispatcher(IServiceProvider serviceProvider) : ILocalEventDispatcher
    {
        public async Task DispatchAsync(object localEvent, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(localEvent);
            if (serviceProvider.GetService(typeof(IEnumerable<ILocalEventHandler>))
                is not IEnumerable<ILocalEventHandler> handlers)
            {
                return;
            }

            foreach (var handler in handlers)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!handler.EventType.IsInstanceOfType(localEvent))
                {
                    continue;
                }

                await handler.HandleAsync(localEvent, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
