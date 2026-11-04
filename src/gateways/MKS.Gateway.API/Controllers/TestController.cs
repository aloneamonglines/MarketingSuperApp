using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using MKS.Gateway.API.Constants;
using MKS.Gateway.API.Localization;
using MKS.Shared.Domain.Shared.Constants;
using MKS.Shared.Domain.Shared.Localization;

namespace MKS.Gateway.API.Controllers
{
    [ApiController]
    [Route("api/test/localization")]
    public sealed class TestController(
        IStringLocalizer<SharedResource> sharedLocalizer,
        IStringLocalizer<GatewayResource> gatewayLocalizer) : ControllerBase
    {
        [HttpGet]
        public IActionResult Get([FromQuery] string name = GatewayConstants.TestDisplayName)
        {
            return Ok(new
            {
                Culture = System.Globalization.CultureInfo.CurrentUICulture.Name,
                Shared = new
                {
                    Title = sharedLocalizer[SystemConstants.LocalizationTest.Title].Value,
                    Greeting = sharedLocalizer[SystemConstants.LocalizationTest.Greeting, name].Value,
                    SystemUser = sharedLocalizer[SystemConstants.SystemUser.Name].Value,
                    ActiveStatus = sharedLocalizer[SystemConstants.MasterStatus.Active].Value
                },
                Gateway = new
                {
                    Title = gatewayLocalizer[GatewayConstants.LocalizationKeys.Title].Value,
                    Greeting = gatewayLocalizer[GatewayConstants.LocalizationKeys.Greeting, name].Value,
                    Welcome = gatewayLocalizer[GatewayConstants.LocalizationKeys.Welcome, name].Value
                }
            });
        }
    }
}
