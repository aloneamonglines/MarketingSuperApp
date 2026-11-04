using MKS.Identity.Domain.Platform.Entities;
using MKS.Shared.Domain.Shared.Common;

namespace MKS.Identity.Domain.Tests;

public sealed class BreakGlassGrantTests
{
    [Fact]
    public void Create_ProducesReadOnlyGrantBoundByExpiry()
    {
        var now = DateTimeOffset.UtcNow;
        var grant = BreakGlassGrantEntity.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Investigate a production access incident",
            now,
            TimeSpan.FromMinutes(30),
            TimeSpan.FromMinutes(60));

        Assert.True(grant.ReadOnly);
        Assert.Equal(now.AddMinutes(30), grant.ExpiresAt);
        Assert.True(grant.IsActiveAt(now));
        Assert.False(grant.IsActiveAt(grant.ExpiresAt));
    }

    [Fact]
    public void Create_RejectsShortReasonWithLocalizedDomainError()
    {
        var exception = Assert.Throws<DomainException>(() => BreakGlassGrantEntity.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Incident",
            DateTimeOffset.UtcNow,
            TimeSpan.FromMinutes(10),
            TimeSpan.FromMinutes(60)));

        Assert.Equal("Identity.Errors.BreakGlassReason", exception.LocalizationKey);
    }

    [Fact]
    public void Create_RejectsDurationAboveConfiguredMaximum()
    {
        var exception = Assert.Throws<DomainException>(() => BreakGlassGrantEntity.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Investigate a production access incident",
            DateTimeOffset.UtcNow,
            TimeSpan.FromMinutes(61),
            TimeSpan.FromMinutes(60)));

        Assert.Equal("Identity.Errors.BreakGlassDuration", exception.LocalizationKey);
    }
}
