using MKS.Identity.Application.Authorization;
using MKS.Identity.Application.Contracts.Authorization.DTOs;
using MKS.Identity.Domain.Shared.Constants;
using MKS.Shared.Domain.Shared.Common;

namespace MKS.Identity.Domain.Tests;

public sealed class RoleCrudContractTests
{
    [Fact]
    public async Task GetManyRejectsEmptyOrDuplicateIdentifiers()
    {
        var service = CreateService();
        var roleId = Guid.NewGuid();

        var emptyException = await Assert.ThrowsAsync<DomainException>(
            () => service.GetManyAsync([]));
        var duplicateException = await Assert.ThrowsAsync<DomainException>(
            () => service.GetManyAsync([roleId, roleId]));

        Assert.Equal(IdentityDomainErrorCodes.InvalidInput, emptyException.LocalizationKey);
        Assert.Equal(IdentityDomainErrorCodes.InvalidInput, duplicateException.LocalizationKey);
    }

    [Fact]
    public async Task BulkMutationRejectsDuplicateIdentifiersBeforeAccessingPersistence()
    {
        var service = CreateService();
        var roleId = Guid.NewGuid();

        var updateException = await Assert.ThrowsAsync<DomainException>(
            () => service.UpdateManyAsync([
                new UpdateRoleItemDto(roleId, "First", 1),
                new UpdateRoleItemDto(roleId, "Second", 1)
            ]));
        var deleteException = await Assert.ThrowsAsync<DomainException>(
            () => service.DeleteManyAsync([roleId, roleId]));

        Assert.Equal(IdentityDomainErrorCodes.InvalidInput, updateException.LocalizationKey);
        Assert.Equal(IdentityDomainErrorCodes.InvalidInput, deleteException.LocalizationKey);
    }

    [Fact]
    public async Task BulkMutationRejectsMoreThanOneThousandItems()
    {
        var service = CreateService();
        var inputs = Enumerable.Range(0, 1001)
            .Select(_ => new UpdateRoleItemDto(Guid.NewGuid(), "Role", 1))
            .ToArray();

        var exception = await Assert.ThrowsAsync<DomainException>(
            () => service.UpdateManyAsync(inputs));

        Assert.Equal(IdentityDomainErrorCodes.InvalidInput, exception.LocalizationKey);
    }

    [Fact]
    public async Task BulkCreateRejectsDuplicateRoleCodesBeforeAccessingPersistence()
    {
        var service = CreateService();
        var inputs = new[]
        {
            new CreateRoleDto("First", "support", ["function"], []),
            new CreateRoleDto("Second", "SUPPORT", ["function"], [])
        };

        var exception = await Assert.ThrowsAsync<DomainException>(
            () => service.CreateManyAsync(inputs));

        Assert.Equal(IdentityDomainErrorCodes.IdentifierAlreadyExists, exception.LocalizationKey);
    }

    [Fact]
    public void RoleRequestContractsDoNotAcceptClientScopeOverrides()
    {
        Assert.DoesNotContain(
            typeof(CreateRoleDto).GetProperties(),
            property => property.Name is nameof(RoleDto.AppId) or nameof(RoleDto.TenantId));
        Assert.DoesNotContain(
            typeof(UpdateRoleDto).GetProperties(),
            property => property.Name is nameof(RoleDto.AppId) or nameof(RoleDto.TenantId));
    }

    private static RoleApplicationService CreateService() =>
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
            null!);
}
