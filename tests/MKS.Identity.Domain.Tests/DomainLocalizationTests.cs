using System.Globalization;
using MKS.Identity.Domain.Shared.Constants;
using MKS.Identity.Domain.Shared.Localization;
using MKS.Shared.Domain.Shared.Localization;
using MKS.Shared.Domain.Shared.Common;
using MKS.Shared.Domain.Shared.Common.ValueObjects;
using MKS.Shared.Domain.Shared.Constants;
using MKS.Shared.Infrastructure.Localization;

namespace MKS.Identity.Domain.Tests;

public sealed class DomainLocalizationTests
{
    [Fact]
    public void AddressValidationUsesSharedLocalizationKeysAndArguments()
    {
        var exception = Assert.Throws<DomainException>(
            () => Address.Create(new string('a', 256), "City", "VN"));

        Assert.Equal(SharedDomainErrorCodes.Address.ComponentTooLong, exception.LocalizationKey);
        Assert.Equal([255], exception.LocalizationArguments);
    }

    [Fact]
    public void ImageValidationUsesSharedLocalizationKeys()
    {
        var exception = Assert.Throws<DomainException>(() => Image.Create("javascript:alert(1)"));

        Assert.Equal(SharedDomainErrorCodes.Image.UrlSchemeInvalid, exception.LocalizationKey);
    }

    [Fact]
    public void SharedAddressErrorsAreLocalizedWithArguments()
    {
        var originalCulture = CultureInfo.CurrentUICulture;
        try
        {
            var localizer = new JsonStringLocalizer<SharedResource>();
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("vi");

            var message = localizer[SharedDomainErrorCodes.Address.ComponentTooLong, 255].Value;

            Assert.Equal("Thành phần địa chỉ không được vượt quá 255 ký tự.", message);
        }
        finally
        {
            CultureInfo.CurrentUICulture = originalCulture;
        }
    }

    [Fact]
    public void IdentityProtocolErrorsAreLocalized()
    {
        var originalCulture = CultureInfo.CurrentUICulture;
        try
        {
            var localizer = new JsonStringLocalizer<IdentityResource>();
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("vi");

            var message = localizer[IdentityProtocolMessages.InvalidAppContext].Value;

            Assert.Equal("Ngữ cảnh ứng dụng không hợp lệ.", message);
        }
        finally
        {
            CultureInfo.CurrentUICulture = originalCulture;
        }
    }
}
