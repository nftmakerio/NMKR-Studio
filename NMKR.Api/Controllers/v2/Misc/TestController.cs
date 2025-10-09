using System.Net;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Classes;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.v2.Misc
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class TestController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IBus _bus;

        public TestController(IConnectionMultiplexer redis, IBus bus)
        {
            _redis = redis;
            _bus = bus;
        }
        public async Task<IActionResult> Get()
        {
            var remoteIpAddress = (HttpContext.Connection.RemoteIpAddress) ?? IPAddress.None;

            return Ok(GeneralConfigurationClass.KoiosApi + " "+remoteIpAddress.ToString());
        }
    }
}
