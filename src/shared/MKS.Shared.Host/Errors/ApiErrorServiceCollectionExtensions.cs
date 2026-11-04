using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MKS.Shared.Application.Contracts.Common.DTOs;
using MKS.Shared.Host.Validation;

namespace MKS.Shared.Host.Errors
{
    /// <summary>Registers standard API exception handling and request contract validation.</summary>
    public static class ApiErrorServiceCollectionExtensions
    {
        public static IServiceCollection AddSharedApiErrors(
            this IServiceCollection services,
            params Assembly[] contractAssemblies)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(contractAssemblies);

            services.TryAddScoped<IApiErrorMessageLocalizer, SharedApiErrorMessageLocalizer>();
            services.AddScoped<ContractValidationActionFilter>();
            services.Configure<MvcOptions>(options =>
                options.Filters.AddService<ContractValidationActionFilter>());
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var failures = context.ModelState
                        .Where(entry => entry.Value?.Errors.Count > 0)
                        .SelectMany(entry => entry.Value!.Errors.Select(_ =>
                            new ApiValidationFailure(
                                entry.Key,
                                "Shared.Validation.Invalid",
                                "Shared.Validation.Invalid")))
                        .ToArray();
                    throw new RequestValidationException(failures);
                };
            });

            foreach (var assembly in contractAssemblies.Distinct())
            {
                services.AddValidatorsFromAssembly(assembly);
            }

            return services;
        }

        public static IApplicationBuilder UseSharedApiErrors(this IApplicationBuilder application)
        {
            ArgumentNullException.ThrowIfNull(application);
            return application.UseMiddleware<GlobalExceptionMiddleware>();
        }
    }
}
