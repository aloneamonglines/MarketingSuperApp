using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Localization;
using MKS.Shared.Domain.Shared.Localization;
using MKS.Shared.Infrastructure.Localization;

namespace MKS.Shared.Host.Localization
{
    public static class LocalizationServiceCollectionExtensions
    {
        private static readonly string[] ResourceCultures = ["en", "vi"];

        public static IServiceCollection AddSharedLocalization(
            this IServiceCollection services,
            string defaultCultureName = "en")
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentException.ThrowIfNullOrWhiteSpace(defaultCultureName);

            var defaultCulture = CultureInfo.GetCultureInfo(defaultCultureName);
            var supportedCultures = ResourceCultures
                .Append(defaultCulture.Name)
                .Select(CultureInfo.GetCultureInfo)
                .DistinctBy(culture => culture.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture(defaultCulture);
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            services.TryAddSingleton<IStringLocalizer<SharedResource>>(
                _ => new JsonStringLocalizer<SharedResource>(defaultCulture.Name));

            return services;
        }

        public static IApplicationBuilder UseSharedLocalization(this IApplicationBuilder application)
        {
            ArgumentNullException.ThrowIfNull(application);
            return application.UseRequestLocalization();
        }
    }
}
