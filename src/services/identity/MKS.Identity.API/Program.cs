using MKS.Identity.API.Infrastructure;
using MKS.Identity.Domain.Shared.Localization;
using MKS.Shared.Host.Localization;
using MKS.Shared.Host.Errors;
using MKS.Shared.Infrastructure.Localization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddIdentityApi(builder.Configuration, builder.Environment);
builder.Services.AddSingleton<Microsoft.Extensions.Localization.IStringLocalizer<IdentityResource>>(
    _ => new JsonStringLocalizer<IdentityResource>());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseSharedLocalization();
app.UseSharedApiErrors();
app.UseAuthentication();
app.UseMiddleware<SessionValidationMiddleware>();
app.UseMiddleware<MustChangePasswordMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program
{
}
