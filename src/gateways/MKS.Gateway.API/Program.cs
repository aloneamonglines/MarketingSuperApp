using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Localization;
using MKS.Gateway.API.Constants;
using MKS.Gateway.API.Localization;
using MKS.Identity.Domain.Shared.Constants;
using MKS.Shared.Domain.Shared.Localization;
using MKS.Shared.Host.Localization;
using MKS.Shared.Host.Errors;
using MKS.Shared.Infrastructure.Localization;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);
var authority = builder.Configuration[GatewayConstants.IdentityAuthorityConfigurationKey]
    ?? throw new InvalidOperationException($"{GatewayConstants.IdentityAuthorityConfigurationKey} is required.");

builder.Services.AddOpenApi();
builder.Services.AddControllers();
builder.Services.AddSharedLocalization();
builder.Services.AddSharedApiErrors(typeof(MKS.Shared.Application.Contracts.Common.DTOs.PagedAndSortedResultRequestDto).Assembly);
builder.Services.AddSingleton<IStringLocalizer<GatewayResource>>(
    _ => new JsonStringLocalizer<GatewayResource>());
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authority;
        options.Audience = IdentityConstants.Resources.Gateway;
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.MapInboundClaims = false;
        options.TokenValidationParameters.NameClaimType = IdentityConstants.Claims.Subject;
    });
builder.Services.AddAuthorization();
builder.Services.AddCors(options => options.AddPolicy(GatewayConstants.CorsPolicyName, policy =>
{
    var origins = builder.Configuration.GetSection(GatewayConstants.CorsAllowedOriginsConfigurationSection).Get<string[]>() ?? [];
    if (origins.Length > 0)
    {
        policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
    }
}));
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        var scope = context.User.FindFirst(IdentityConstants.Claims.TenantId)?.Value
            ?? context.User.FindFirst(IdentityConstants.Claims.Subject)?.Value
            ?? context.Connection.RemoteIpAddress?.ToString()
            ?? GatewayConstants.UnknownRateLimitPartition;
        return RateLimitPartition.GetFixedWindowLimiter(
            scope,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 120,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            });
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection(GatewayConstants.ReverseProxyConfigurationSection))
    .AddTransforms(transformBuilder =>
    {
        transformBuilder.AddRequestTransform(transformContext =>
        {
            foreach (var header in transformContext.ProxyRequest.Headers
                         .Where(header => header.Key.StartsWith(GatewayConstants.TenantHeaderPrefix, StringComparison.OrdinalIgnoreCase))
                         .Select(header => header.Key)
                         .ToArray())
            {
                transformContext.ProxyRequest.Headers.Remove(header);
            }

            return ValueTask.CompletedTask;
        });
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Use(async (context, next) =>
{
    foreach (var header in context.Request.Headers.Keys
                 .Where(name => name.StartsWith(GatewayConstants.TenantHeaderPrefix, StringComparison.OrdinalIgnoreCase))
                 .ToArray())
    {
        context.Request.Headers.Remove(header);
    }

    var correlationId = Guid.NewGuid().ToString("N");
    context.Response.OnStarting(() =>
    {
        context.Response.Headers[GatewayConstants.CorrelationIdHeader] = correlationId;
        return Task.CompletedTask;
    });
    await next();
});

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors(GatewayConstants.CorsPolicyName);
app.UseSharedLocalization();
app.UseSharedApiErrors();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.UseSharedLocalization();
app.MapControllers();
app.MapReverseProxy();

app.Run();

public partial class Program
{
}
