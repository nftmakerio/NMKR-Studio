using NMKR.Shared.NotificationClasses;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;
using Asp.Versioning;

namespace NMKR.Api.Controllers.v2.Misc
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class TestRabbitMqController : ControllerBase
    {

        private readonly IConnectionMultiplexer _redis;

        private readonly IBus _bus;

        public TestRabbitMqController(IConnectionMultiplexer redis, IBus bus)
        {
            _redis = redis;
            _bus = bus;
        }


        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Tools" }
        )]
        public async Task<IActionResult> Get()
        {
            await _bus.Publish(new RmqTransactionClass { AddressId = 1234, ProjectId = 1111, EventType = NotificationEventTypes.transactioncanceled, TransactionId = null });
            return Ok("OK");
        }

    }
}
