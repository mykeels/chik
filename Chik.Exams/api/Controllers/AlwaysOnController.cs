using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Chik.Exams.Api;

[ApiController]
[Route("")]
[AllowAnonymous]
public class AlwaysOnController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var csprojVersion = AssemblyExtensions.GetCsprojVersion();
        var now = DateTime.UtcNow;
        var clientIpAddress = Request.GetClientIpAddress();
        var clientIpAddressCountry = Request.GetClientIpAddressCountry();

        return Ok(new {
            Name = "Chik.Exams",
            Version = csprojVersion,
            Now = now,
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
            IpAddress = new {
                Address = clientIpAddress,
                Country = clientIpAddressCountry,
                Timezone = clientIpAddressCountry is not null ? IpAddressLocation.GetCountryTimezone(clientIpAddressCountry) : null
            }
        });
    }
}