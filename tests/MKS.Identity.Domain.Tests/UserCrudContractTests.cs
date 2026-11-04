using MKS.Identity.Application.Contracts.Users.DTOs;
using MKS.Identity.Application.Users;
using MKS.Identity.Domain.Shared.Constants;
using MKS.Shared.Domain.Shared.Common;

namespace MKS.Identity.Domain.Tests;

public sealed class UserCrudContractTests
{
    [Fact]
    public async Task UserBulkEndpointsRejectEmptyOrDuplicateIdentifiers()
    {
        var service = CreateService();
        var userId = Guid.NewGuid();

        var emptyException = await Assert.ThrowsAsync<DomainException>(
            () => service.GetManyAsync(new GetManyUsersDto([])));
        var deactivateException = await Assert.ThrowsAsync<DomainException>(
            () => service.DeactivateManyAsync(new DeactivateUsersDto([userId, userId])));
        var updateException = await Assert.ThrowsAsync<DomainException>(
            () => service.UpdateProfilesAsync(new BulkUpdateUserProfilesDto([
                new UpdateUserProfileItemDto(userId, "First", "User", null),
                new UpdateUserProfileItemDto(userId, "Second", "User", null)
            ])));

        Assert.Equal(IdentityDomainErrorCodes.InvalidInput, emptyException.LocalizationKey);
        Assert.Equal(IdentityDomainErrorCodes.InvalidInput, deactivateException.LocalizationKey);
        Assert.Equal(IdentityDomainErrorCodes.InvalidInput, updateException.LocalizationKey);
    }

    [Fact]
    public async Task BulkCreateRejectsEmptyOrOversizedRequestsBeforeAccessingPersistence()
    {
        var service = CreateService();
        var user = new CreateUserDto(Guid.NewGuid(), "user@example.test", null, null, null, null);
        var tooManyUsers = Enumerable.Repeat(user, 1001).ToArray();

        var emptyException = await Assert.ThrowsAsync<DomainException>(
            () => service.CreateManyAsync(new CreateManyUsersDto([])));
        var oversizedException = await Assert.ThrowsAsync<DomainException>(
            () => service.CreateManyAsync(new CreateManyUsersDto(tooManyUsers)));

        Assert.Equal(IdentityDomainErrorCodes.InvalidInput, emptyException.LocalizationKey);
        Assert.Equal(IdentityDomainErrorCodes.InvalidInput, oversizedException.LocalizationKey);
    }

    [Fact]
    public void UserResponseDoesNotExposeAuthenticationIdentifiersOrSecrets()
    {
        var propertyNames = typeof(UserDto).GetProperties()
            .Select(property => property.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.DoesNotContain("PasswordHash", propertyNames);
        Assert.DoesNotContain("Credential", propertyNames);
        Assert.DoesNotContain("Email", propertyNames);
        Assert.DoesNotContain("PhoneNumber", propertyNames);
    }

    private static UserApplicationService CreateService() =>
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
            null!);
}
