using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;

namespace NMKR.Api.Controllers.v2.Misc
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class TestWebhookController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public TestWebhookController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

       
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status412PreconditionFailed)]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable)]

        [HttpPost]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Tools" }
        )]
        public async Task<IActionResult> Post([FromQuery] string? payloadHash, [FromBody] object payloadobject)
        {
            var secret = "abcde12345";

            string payload = payloadobject.ToString();

            if (string.IsNullOrEmpty(payload))
                return StatusCode(406);

            if (string.IsNullOrEmpty(payloadHash))
                return StatusCode(412);

            var hash = GlobalFunctions.GetHMAC(payload,secret);

        /*    if (hash != payloadHash)
                return StatusCode(406);*/

            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            await GlobalFunctions.LogMessageAsync(db, "Webhook received", payload);
            await db.Database.CloseConnectionAsync();
            return Ok();

        }
    }
}
