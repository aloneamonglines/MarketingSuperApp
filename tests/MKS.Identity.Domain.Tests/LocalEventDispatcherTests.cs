using System.Diagnostics;
using MKS.Shared.Application.Contracts.Common.Events;
using MKS.Shared.Application.Events;
using MKS.Shared.Domain.Shared.Common.Events;

namespace MKS.Identity.Domain.Tests;

public sealed class LocalEventDispatcherTests
{
    [Fact]
    public async Task DispatchInvokesRegisteredTypedHandlers()
    {
        var handler = new TestEventHandler();
        var dispatcher = new LocalEventDispatcher(new TestServiceProvider([handler]));
        var localEvent = new TestEvent("payload");

        await dispatcher.DispatchAsync(localEvent);

        Assert.Same(localEvent, handler.HandledEvent);
    }

    [Fact]
    public async Task DispatchWithoutRegisteredHandlersCompletes()
    {
        var dispatcher = new LocalEventDispatcher(new TestServiceProvider([]));

        await dispatcher.DispatchAsync(new TestEvent("payload"));
    }

    [Fact]
    public async Task DefaultEntityCreatedHandlerEmitsStandardLifecycleActivity()
    {
        Activity? stoppedActivity = null;
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == "MKS.EntityLifecycle",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStopped = activity => stoppedActivity = activity
        };
        ActivitySource.AddActivityListener(listener);
        var dispatcher = new LocalEventDispatcher(
            new TestServiceProvider([new EntityCreatedEventHandler()]));

        await dispatcher.DispatchAsync(
            new EntityCreatedEvent(new object(), Guid.NewGuid(), DateTimeOffset.UtcNow));

        Assert.NotNull(stoppedActivity);
        Assert.Equal("entity.created", stoppedActivity.OperationName);
    }

    private sealed record TestEvent(string Value);

    private sealed class TestEventHandler : ILocalEventHandler<TestEvent>
    {
        public TestEvent? HandledEvent { get; private set; }

        public Task HandleAsync(TestEvent localEvent, CancellationToken cancellationToken = default)
        {
            HandledEvent = localEvent;
            return Task.CompletedTask;
        }
    }

    private sealed class TestServiceProvider(IReadOnlyCollection<ILocalEventHandler> handlers) : IServiceProvider
    {
        public object? GetService(Type serviceType)
        {
            if (serviceType != typeof(IEnumerable<ILocalEventHandler>))
            {
                return null;
            }

            return handlers;
        }
    }
}
