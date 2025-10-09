using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Api.Controllers.SharedClasses;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;

namespace NMKR.Api.Controllers.v2.NmkrPay
{
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class GetNmkrPayStatusController : ControllerBase
    {

        private readonly IConnectionMultiplexer _redis;

        public GetNmkrPayStatusController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Returns the state of a payment link for NMKR Pay
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="paymenttransaction">The GetNmkrPayLinkClass as Body Content</param>
        /// <response code="200">Returns the PaymentTransactionResultClass Class</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>
        /// <response code="406">See the errormessage in the resultset for further information</response>
        /// <response code="500">Internal server error - see the errormessage in the resultset</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetNmkrPayLinkResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{paymenttransactionuid}")]
        [SwaggerOperation(
            Tags = new[] { "NMKR Pay" }
        )]
        [MapToApiVersion("2")]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string paymenttransactionuid)
        {
           // string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            if (Request.Method.Equals("HEAD"))
                return null;

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;

            var result = CheckCachedAccess.CheckApikeyOrToken(_redis, apifunction,
                "", apikey, remoteIpAddress?.ToString());

            if (result.ResultState != ResultStates.Ok)
            {
                return Unauthorized(result);
            }


            var res = StaticTransactionFunctions.GetTransactionState(db,_redis, paymenttransactionuid, false, false);

            return Ok(new GetNmkrPayLinkResultClass(res));
        }
    }
}
