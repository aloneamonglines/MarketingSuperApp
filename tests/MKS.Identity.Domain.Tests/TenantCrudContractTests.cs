using MKS.Identity.Application.Contracts.MultiTenancy.DTOs;
using MKS.Identity.Application.MultiTenancy;
using MKS.Identity.Domain.Shared.Constants;
using MKS.Shared.Domain.Shared.Common;

namespace MKS.Identity.Domain.Tests;

public sealed class TenantCrudContractTests
{
    [Fact]
    public async Task BulkCreateRejectsEmptyOrOversizedRequestsBeforeAccessingPersistence()
    {
        var service = CreateService();
        var input = new CreateTenantDto("Tenant", "tenant", "admin@example.test", null, null);
        var tooMany = Enumerable.Repeat(input, 1001).ToArray();

        var emptyException = await Assert.ThrowsAsync<DomainException>(
            () => service.CreateManyAsync([]));
        var oversizedException = await Assert.ThrowsAsync<DomainException>(
            () => service.CreateManyAsync(tooMany));

        Assert.Equal(IdentityDomainErrorCodes.InvalidInput, emptyException.LocalizationKey);
        Assert.Equal(IdentityDomainErrorCodes.InvalidInput, oversizedException.LocalizationKey);
    }

    private static TenantApplicationService CreateService() =>
        new(
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);
}
