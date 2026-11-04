using MKS.Identity.Domain.Shared.Enums;
using MKS.Identity.Domain.Users.Entities;
using MKS.Identity.Domain.Users.Services;
using MKS.Shared.Domain.Shared.Common;

namespace MKS.Identity.Domain.Tests;

public sealed class IdentifierNormalizerTests
{
    [Theory]
    [InlineData("  USER@Example.COM  ", CIdentifierType.Email, "user@example.com")]
    [InlineData("  +1 (202) 555-0100 ", CIdentifierType.Phone, "+12025550100")]
    [InlineData(" Admin ", CIdentifierType.Username, "admin")]
    public void Normalize_CanonicalizesSupportedIdentifiers(
        string value,
        CIdentifierType type,
        string expected) =>
        Assert.Equal(expected, IdentifierNormalizer.Normalize(value, type));

    [Fact]
    public void UserCreate_DerivesUsernameFromEmailWithoutDuplicatingIdentifier()
    {
        var user = UserEntity.Create("user@example.com", null, null);

        var identifier = Assert.Single(user.Identifiers);
        Assert.Equal(CIdentifierType.Email, identifier.Type);
        Assert.True(identifier.IsDerived);
    }

    [Fact]
    public void Normalize_RejectsNonE164PhoneNumbers()
    {
        var exception = Assert.Throws<DomainException>(
            () => IdentifierNormalizer.Normalize("202-555-0100", CIdentifierType.Phone));

        Assert.Equal("Identity.Errors.InvalidIdentifier", exception.LocalizationKey);
    }
}
