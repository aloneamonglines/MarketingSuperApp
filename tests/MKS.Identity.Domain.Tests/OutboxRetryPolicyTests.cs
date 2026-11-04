using MKS.Identity.Application.Common;
using MKS.Identity.Application.Contracts.Common.Options;

namespace MKS.Identity.Domain.Tests;

public sealed class OutboxRetryPolicyTests
{
    [Fact]
    public void RetryDelayDoublesAndIsCapped()
    {
        var options = new OutboxProcessingOptions
        {
            InitialRetryDelay = TimeSpan.FromSeconds(2),
            MaxRetryDelay = TimeSpan.FromSeconds(10)
        };

        Assert.Equal(TimeSpan.FromSeconds(2), OutboxRetryPolicy.GetDelay(1, options));
        Assert.Equal(TimeSpan.FromSeconds(4), OutboxRetryPolicy.GetDelay(2, options));
        Assert.Equal(TimeSpan.FromSeconds(8), OutboxRetryPolicy.GetDelay(3, options));
        Assert.Equal(TimeSpan.FromSeconds(10), OutboxRetryPolicy.GetDelay(4, options));
        Assert.Equal(TimeSpan.FromSeconds(10), OutboxRetryPolicy.GetDelay(20, options));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RetryDelayRejectsInvalidAttempt(int attempt)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => OutboxRetryPolicy.GetDelay(attempt, new OutboxProcessingOptions()));
    }
}
