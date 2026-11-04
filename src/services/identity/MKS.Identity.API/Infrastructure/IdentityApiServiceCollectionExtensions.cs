using System.Security.Cryptography.X509Certificates;
using MKS.Identity.API.Infrastructure;
using MKS.Identity.Application.Contracts.Common.Options;
using MKS.Identity.Application.DependencyInjection;
using MKS.Identity.Infrastructure.DependencyInjection;
using MKS.Identity.Infrastructure.Persistence;
using MKS.Identity.Infrastructure.Messaging;
using MKS.Identity.Domain.Shared.Constants;
using MKS.Identity.Domain.Shared.Localization;
using MKS.Shared.Application.Contracts.Common.Interfaces;
using MKS.Shared.Host.Errors;
using MKS.Shared.Host.Localization;
using MKS.Shared.Host.Notifications;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Server;
using OpenIddict.Validation.AspNetCore;

namespace MKS.Identity.API.Infrastructure
{
    /// <summary>
    /// Composes the Identity API's application, persistence, localization, and OpenIddict services.
    /// </summary>
    public static class IdentityApiServiceCollectionExtensions
    {
        /// <summary>Registers Identity service dependencies and OpenIddict endpoints.</summary>
        public static IServiceCollection AddIdentityApi(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configuration);
            ArgumentNullException.ThrowIfNull(environment);

            var identityOptions = configuration.GetSection(IdentityOptions.SectionName).Get<IdentityOptions>()
                ?? new IdentityOptions();
            if (string.IsNullOrWhiteSpace(identityOptions.Issuer))
            {
                throw new InvalidOperationException(IdentityConstants.ConfigurationErrors.IssuerRequired);
            }
            if (!Uri.TryCreate(identityOptions.Issuer, UriKind.Absolute, out var issuer))
            {
                throw new InvalidOperationException(IdentityConstants.ConfigurationErrors.IssuerMustBeAbsolute);
            }
            services.AddOptions<IdentityOptions>()
                .Bind(configuration.GetSection(IdentityOptions.SectionName))
                .Validate(
                    options =>
                        Uri.TryCreate(options.Issuer, UriKind.Absolute, out _) &&
                        options.AccessTokenMinutes > 0 &&
                        options.RefreshTokenDays > 0 &&
                        options.FailedSignInThreshold > 0 &&
                        options.LockoutMinutes > 0 &&
                        options.BreakGlassMaximumMinutes > 0,
                    IdentityConstants.ConfigurationErrors.IdentityOptionsInvalid)
                .ValidateOnStart();

            services.AddHttpContextAccessor();
            services.TryAddScoped<ICurrentContext, HttpCurrentContext>();
            services.AddSharedLocalization();
            services.AddSharedApiErrors(
                typeof(MKS.Identity.Application.Contracts.Users.DTOs.CreateUserDto).Assembly,
                typeof(MKS.Shared.Application.Contracts.Common.DTOs.PagedAndSortedResultRequestDto).Assembly);
            services.Replace(ServiceDescriptor.Scoped<IApiErrorMessageLocalizer, IdentityApiErrorMessageLocalizer>());
            services.AddSharedNotifications();
            services.AddIdentityInfrastructure(configuration);
            services.AddIdentityApplication(options =>
            {
                options.DefaultPassword = identityOptions.DefaultPassword;
                options.FailedSignInThreshold = identityOptions.FailedSignInThreshold;
                options.LockoutDuration = TimeSpan.FromMinutes(identityOptions.LockoutMinutes);
                options.BreakGlassMaximumDuration = TimeSpan.FromMinutes(identityOptions.BreakGlassMaximumMinutes);
            });
            services.AddHostedService<IdentityDatabaseInitializer>();
            services.AddHostedService<OutboxWorker>();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme;
            });
            services.AddAuthorization();

            services.AddOpenIddict()
                .AddCore(options => options.UseEntityFrameworkCore()
                    .UseDbContext<IdentityDbContext>()
                    .ReplaceDefaultEntities<Guid>())
                .AddServer(options =>
                {
                    options.SetIssuer(issuer);
                    options.SetTokenEndpointUris("/connect/token");
                    options.AllowPasswordFlow();
                    options.AllowRefreshTokenFlow();
                    options.AllowCustomFlow(IdentityConstants.BreakGlassGrantType);
                    options.AcceptAnonymousClients();
                    options.SetAccessTokenLifetime(TimeSpan.FromMinutes(identityOptions.AccessTokenMinutes));
                    options.SetRefreshTokenLifetime(TimeSpan.FromDays(identityOptions.RefreshTokenDays));
                    options.DisableAccessTokenEncryption();

                    if (environment.IsDevelopment())
                    {
                        options.AddEphemeralSigningKey();
                        options.AddEphemeralEncryptionKey();
                    }
                    else
                    {
                        var signingCertificate = LoadCertificate(identityOptions.SigningCertificate, "Signing");
                        var encryptionCertificate = LoadCertificate(identityOptions.EncryptionCertificate, "Encryption");
                        options.AddSigningCertificate(signingCertificate);
                        options.AddEncryptionCertificate(encryptionCertificate);
                    }

                    options.AddEventHandler<OpenIddictServerEvents.HandleTokenRequestContext>(
                        handler => handler.UseScopedHandler<PasswordGrantHandler>());
                    // This must run before OpenIddict clones principals for the newly issued tokens.
                    options.AddEventHandler<OpenIddictServerEvents.ProcessSignInContext>(
                        handler => handler
                            .UseScopedHandler<RefreshTokenSessionHandler>()
                            .SetOrder(OpenIddictServerHandlers.RestoreInternalClaims.Descriptor.Order + 500));
                    var aspNetCore = options.UseAspNetCore();
                    if (environment.IsDevelopment())
                    {
                        aspNetCore.DisableTransportSecurityRequirement();
                    }
                })
                .AddValidation(options =>
                {
                    options.UseLocalServer();
                    options.UseAspNetCore();
                });

            return services;
        }

        private static X509Certificate2 LoadCertificate(
            IdentityOptions.CertificateOptions certificate,
            string name)
        {
            var path = certificate.Path;
            var password = certificate.Password;
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path) || string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException(
                    string.Format(
                        System.Globalization.CultureInfo.InvariantCulture,
                        IdentityConstants.ConfigurationErrors.CertificateConfigurationInvalid,
                        name));
            }

            return X509CertificateLoader.LoadPkcs12FromFile(
                path,
                password,
                X509KeyStorageFlags.EphemeralKeySet | X509KeyStorageFlags.Exportable);
        }
    }
}
