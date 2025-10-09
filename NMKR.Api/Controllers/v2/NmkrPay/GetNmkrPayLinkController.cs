using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Api.Controllers.SharedClasses;
using NMKR.Api.Controllers.v2.Paymenttransactions.Prepare.NmkrPay;
using NMKR.Api.Controllers.v2.Paymenttransactions.Prepare;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;

namespace NMKR.Api.Controllers.v2.NmkrPay
{
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class GetNmkrPayLinkController : ControllerBase
    {

        private readonly IConnectionMultiplexer _redis;

        public GetNmkrPayLinkController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Returns a payment link for NMKR Pay
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
        [HttpPost]
        [SwaggerOperation(
            Tags = new[] {"NMKR Pay"}
        )]
        [MapToApiVersion("2")]
        public async Task<IActionResult> Post([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, [FromBody] GetNmkrPayLinkClass paymenttransaction)
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
                paymenttransaction.ProjectUid, apikey, remoteIpAddress?.ToString());

            if (result.ResultState != ResultStates.Ok)
            {
                return Unauthorized(result);
            }

            await GlobalFunctions.LogMessageAsync(db, "Api-Call: CreatePaymentTransaction",
                JsonConvert.SerializeObject(paymenttransaction,
                    new JsonSerializerSettings() {ReferenceLoopHandling = ReferenceLoopHandling.Ignore}));

            var project = await (from a in db.Nftprojects
                    .Include(a => a.Smartcontractssettings).AsSplitQuery()
                    .Include(a => a.Customerwallet).AsSplitQuery()
                    .Include(a => a.Customer).AsSplitQuery()
                where a.Uid == paymenttransaction.ProjectUid
                select a).FirstOrDefaultAsync();
            if (project == null)
            {
                result.ErrorCode = 1001;
                result.ErrorMessage = "Project UID not found";
                result.ResultState = ResultStates.Error;
                return StatusCode(404, result);
            }

            PreparedPaymentTransactionsBaseClass pptbc = paymenttransaction.PaymentTransactionType == NmkrPayTransactionTypes.nmkr_pay_random
                ? new PreparedPaymentTransactionsNmkrPayRandomClass(db, _redis, PaymentTransactionTypes.nmkr_pay_random)
                : new PreparedPaymentTransactionsNmkrPaySpecificClass(db, _redis, PaymentTransactionTypes.nmkr_pay_specific);

            var transactionClass =
                new CreatePaymentTransactionClass(paymenttransaction, paymenttransaction.PaymentTransactionType==NmkrPayTransactionTypes.nmkr_pay_random? PaymentTransactionTypes.nmkr_pay_random:PaymentTransactionTypes.nmkr_pay_specific);

            result = pptbc.CheckParameter(transactionClass, project, result, out var statuscode);
            if (result.ResultState != ResultStates.Ok)
                return StatusCode(statuscode, result);

            result = pptbc.SaveTransaction(transactionClass, result, project,
                out var preparedpaymenttransaction, out statuscode);
            if (result.ResultState != ResultStates.Ok)
            {
                return StatusCode(statuscode, result);
            }

            var res = StaticTransactionFunctions.GetTransactionState(db, _redis,
                preparedpaymenttransaction, preparedpaymenttransaction.Transactionuid, false, null);

            return Ok(new GetNmkrPayLinkResultClass(res));
        }
    }
}
