using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using MKS.Gateway.API.Localization;
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
        public IActionResult Get([FromQuery] string name = "Marketing Super App")
        {
            return Ok(new
            {
                Culture = System.Globalization.CultureInfo.CurrentUICulture.Name,
                Shared = new
                {
                    Title = sharedLocalizer["System.LocalizationTest.Title"].Value,
                    Greeting = sharedLocalizer["System.LocalizationTest.Greeting", name].Value,
                    SystemUser = sharedLocalizer["System.User.Name"].Value,
                    ActiveStatus = sharedLocalizer["System.MasterStatus.Active"].Value
                },
                Gateway = new
                {
                    Title = gatewayLocalizer["Test.Title"].Value,
                    Greeting = gatewayLocalizer["Test.Greeting", name].Value,
                    Welcome = gatewayLocalizer["Test.Welcome", name].Value
                }
            });
        }
    }
}
