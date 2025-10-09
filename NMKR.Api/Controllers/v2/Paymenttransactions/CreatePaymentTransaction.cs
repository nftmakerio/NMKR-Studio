using System;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Api.Controllers.SharedClasses;
using NMKR.Api.Controllers.v2.Paymenttransactions.Prepare;
using NMKR.Api.Controllers.v2.Paymenttransactions.Prepare.Decentral;
using NMKR.Api.Controllers.v2.Paymenttransactions.Prepare.Legacy;
using NMKR.Api.Controllers.v2.Paymenttransactions.Prepare.NmkrPay;
using NMKR.Api.Controllers.v2.Paymenttransactions.Prepare.PaymentGatewayTransaction;
using NMKR.Api.Controllers.v2.Paymenttransactions.Prepare.SmartContract;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;

namespace NMKR.Api.Controllers.v2.Paymenttransactions
{
     [ExtendedApiExplorerSettings(IgnoreApi = true)]
    // [ApiExplorerSettings(IgnoreApi = true)]

    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class CreatePaymentTransaction : ControllerBase
    {

        private readonly IConnectionMultiplexer _redis;
        private readonly PaymentTransactionOutputFormat _outputFormat = PaymentTransactionOutputFormat.PrepardPaymentTransaction;

        public CreatePaymentTransaction(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        internal CreatePaymentTransaction(IConnectionMultiplexer redis, PaymentTransactionOutputFormat outputFormat= PaymentTransactionOutputFormat.PrepardPaymentTransaction)
        {
            _redis = redis;
            _outputFormat = outputFormat;
        }

        /// <summary>
        /// Creates a payment transaction
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="paymenttransaction">The CreatePaymentTransactionClass as Body Content</param>
        /// <response code="200">Returns the PaymentTransactionResultClass Class</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>
        /// <response code="406">See the errormessage in the resultset for further information</response>
        /// <response code="500">Internal server error - see the errormessage in the resultset</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentTransactionResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpPost]
        [SwaggerOperation(
            Tags = new[] { "Paymenttransactions" }
        )]
        [MapToApiVersion("2")]
        public async Task<IActionResult> Post([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, [FromBody] CreatePaymentTransactionClass paymenttransaction)
        {
          //  string apikey = Request.Headers["authorization"];
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
                    .Include(a=>a.Customerwallet).AsSplitQuery()
                    .Include(a=>a.Customer).AsSplitQuery()
                where a.Uid == paymenttransaction.ProjectUid
                select a).FirstOrDefaultAsync();
            if (project == null)
            {
                result.ErrorCode = 1001;
                result.ErrorMessage = "Project UID not found";
                result.ResultState = ResultStates.Error;
                return StatusCode(404, result);
            }

            PreparedPaymentTransactionsBaseClass pptbc = paymenttransaction.PaymentTransactionType switch
            {
                PaymentTransactionTypes.paymentgateway_nft_random => new
                    PreparedPaymentTransactionsPaymentGatewayRandomClass(db, _redis,
                        paymenttransaction.PaymentTransactionType),
                PaymentTransactionTypes.paymentgateway_nft_specific => new
                    PreparedPaymentTransactionsPaymentGatewaySpecificClass(db,_redis,
                        paymenttransaction.PaymentTransactionType),
                PaymentTransactionTypes.smartcontract_directsale => new
                    PreparedPaymentTransactionsSmartcontractsDirectsaleClass(db, _redis,
                        paymenttransaction.PaymentTransactionType),
                PaymentTransactionTypes.smartcontract_directsale_offer => new
                    PreparedPaymentTransactionsSmartcontractsDirectsaleOffersClass(db, _redis,
                        paymenttransaction.PaymentTransactionType),
                PaymentTransactionTypes.smartcontract_auction => new
                    PreparedPaymentTransactionsSmartcontractsAuctionClass(db, _redis,
                        paymenttransaction.PaymentTransactionType),
                PaymentTransactionTypes.legacy_auction => new PreparedPaymentTransactionsLegacyAuctionClass(db,_redis,
                    paymenttransaction.PaymentTransactionType),
                PaymentTransactionTypes.legacy_directsale => new PreparedPaymentTransactionsLegacyDirectsaleClass(
                    db, _redis, paymenttransaction.PaymentTransactionType),
                PaymentTransactionTypes.decentral_mintandsend_specific => new
                    PreparedPaymentTransactionsDecentralMintAndSendSpecificClass(db,  _redis,
                        paymenttransaction.PaymentTransactionType),
                PaymentTransactionTypes.decentral_mintandsend_random => new
                    PreparedPaymentTransactionsDecentralMintAndSendRandomClass(db, _redis,
                        paymenttransaction.PaymentTransactionType),
                PaymentTransactionTypes.decentral_mintandsale_specific => new
                    PreparedPaymentTransactionsDecentralMintAndSaleSpecificClass(db,_redis,
                        paymenttransaction.PaymentTransactionType),
                PaymentTransactionTypes.decentral_mintandsale_random => new
                    PreparedPaymentTransactionsDecentralMintAndSaleRandomClass(db, _redis,
                        paymenttransaction.PaymentTransactionType),
                PaymentTransactionTypes.paymentgateway_mintandsend_random => new
                    PreparedPaymentTransactionsPaymentGatewayMintAndSendRandomClass(db, _redis,
                        paymenttransaction.PaymentTransactionType),
                PaymentTransactionTypes.paymentgateway_mintandsend_specific => new
                    PreparedPaymentTransactionsPaymentGatewayMintAndSendSpecificClass(db, _redis,
                        paymenttransaction.PaymentTransactionType),
                PaymentTransactionTypes.nmkr_pay_random => new
                    PreparedPaymentTransactionsNmkrPayRandomClass(db,  _redis,
                        paymenttransaction.PaymentTransactionType),
                PaymentTransactionTypes.nmkr_pay_specific => new
                    PreparedPaymentTransactionsNmkrPaySpecificClass(db, _redis,
                        paymenttransaction.PaymentTransactionType),
                _ => throw new ArgumentOutOfRangeException()
            };

            result = pptbc.CheckParameter(paymenttransaction,project, result, out var statuscode);
            if (result.ResultState != ResultStates.Ok)
                return StatusCode(statuscode, result);

            result = pptbc.SaveTransaction(paymenttransaction, result, project,
                out var preparedpaymenttransaction, out statuscode);
            if (result.ResultState != ResultStates.Ok)
            {
                return StatusCode(statuscode, result);
            }

            var res = StaticTransactionFunctions.GetTransactionState(db, _redis,
                preparedpaymenttransaction, preparedpaymenttransaction.Transactionuid, false, null);

            return _outputFormat == PaymentTransactionOutputFormat.NmkrPay ? Ok(new GetNmkrPayLinkResultClass(res)) : Ok(res);
        }
    }
}
