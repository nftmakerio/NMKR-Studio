using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Api.Controllers.SharedClasses;
using NMKR.Api.Controllers.v2.Paymenttransactions.Processing;
using NMKR.Api.Controllers.v2.Paymenttransactions.Processing.Decentral;
using NMKR.Api.Controllers.v2.Paymenttransactions.Processing.Legacy;
using NMKR.Api.Controllers.v2.Paymenttransactions.Processing.PaymentGatewayTransaction;
using NMKR.Api.Controllers.v2.Paymenttransactions.Processing.SmartContract;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using MassTransit;
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
   
    public class ProceedPaymentTransactionController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IBus _bus;

        public ProceedPaymentTransactionController(IConnectionMultiplexer redis, IBus bus)
        {
            _redis = redis;
            _bus = bus;
        }


        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentTransactionResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{paymenttransactionuid}/GetTransactionState")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Paymenttransactions" }
        )]
        public async Task<IActionResult> GetTransactionState([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey,
            string paymenttransactionuid)
        {
            return await ProceedTransaction( apikey, paymenttransactionuid, ProceedPaymentTransactionCommands.GetTransactionState);
        }


        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetPaymentAddressResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{paymenttransactionuid}/GetPaymentAddress")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Paymenttransactions" }
        )]
        public async Task<IActionResult> GetPaymentAddress([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey,
            string paymenttransactionuid)
        {
            return await ProceedTransaction( apikey,paymenttransactionuid, ProceedPaymentTransactionCommands.GetPaymentAddress);
        }

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentTransactionResultClass))]
        [ProducesResponseType(StatusCodes.Status412PreconditionFailed, Type = typeof(RejectedErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpPost("{paymenttransactionuid}/SignDecentralPayment")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Paymenttransactions" }
        )]
        public async Task<IActionResult> SignDecentralPayment([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey,
            string paymenttransactionuid, [FromBody] SignDecentralClass submittransaction)
        {
            return await ProceedTransaction( apikey, paymenttransactionuid, ProceedPaymentTransactionCommands.SignDecentralPayment, submittransaction);
        }

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CheckAddressResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{paymenttransactionuid}/CheckPaymentAddress")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Paymenttransactions" }
        )]
        public async Task<IActionResult> CheckPaymentAddress([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey,
            string paymenttransactionuid)
        {
            return await ProceedTransaction(apikey,paymenttransactionuid, ProceedPaymentTransactionCommands.CheckPaymentAddress);
        }


        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentTransactionResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpPost("{paymenttransactionuid}/CancelTransaction")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Paymenttransactions" }
        )]
        public async Task<IActionResult> CancelTransaction([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey,
            string paymenttransactionuid, [FromBody] BuyerClass submittransaction)
        {
            return await ProceedTransaction(apikey,paymenttransactionuid, ProceedPaymentTransactionCommands.CancelTransaction, submittransaction);
        }
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentTransactionResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpPost("{paymenttransactionuid}/ExtendReservationTime")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Paymenttransactions" }
        )]
        public async Task<IActionResult> ExtendReservationTime([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey,
            string paymenttransactionuid, int extendTimeInMinutes)
        {
            return await ProceedTransaction(apikey,paymenttransactionuid, ProceedPaymentTransactionCommands.ExtendReservationTime, extendTimeInMinutes);
        }
        
        
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{paymenttransactionuid}/CancelTransaction")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Paymenttransactions" }
        )]
        public async Task<IActionResult> CancelTransaction([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey,
            string paymenttransactionuid)
        {
            return await ProceedTransaction(apikey,paymenttransactionuid, ProceedPaymentTransactionCommands.CancelTransaction);
        }


        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<PricelistClass>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{paymenttransactionuid}/GetPriceListForProject")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Paymenttransactions" }
        )]
        public async Task<IActionResult> GetPriceListForProject([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey,
            string paymenttransactionuid)
        {
            return await ProceedTransaction( apikey,paymenttransactionuid, ProceedPaymentTransactionCommands.GetPriceListForProject);
        }

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentTransactionResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status423Locked, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpPost("{paymenttransactionuid}/LockNft")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Paymenttransactions" }
        )]
        public async Task<IActionResult> LockNft([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey,
            string paymenttransactionuid, [FromBody] SellerClass submittransaction)
        {
            return await ProceedTransaction(apikey,paymenttransactionuid, ProceedPaymentTransactionCommands.LockNft, submittransaction);
        }
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentTransactionResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status423Locked, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpPost("{paymenttransactionuid}/LockAda")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Paymenttransactions" }
        )]
        public async Task<IActionResult> LockAda([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey,
            string paymenttransactionuid, [FromBody] BuyerClass submittransaction)
        {
            return await ProceedTransaction( apikey,paymenttransactionuid, ProceedPaymentTransactionCommands.LockAda, submittransaction);
        }

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentTransactionResultClass))]
        [ProducesResponseType(StatusCodes.Status412PreconditionFailed, Type = typeof(RejectedErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status423Locked, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpPost("{paymenttransactionuid}/SubmitTransaction")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Paymenttransactions" }
        )]
        public async Task<IActionResult> SubmitTransaction([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey,
            string paymenttransactionuid, [FromBody] SubmitTransactionClass submittransaction)
        {
            return await ProceedTransaction(apikey,paymenttransactionuid, ProceedPaymentTransactionCommands.SubmitTransaction, submittransaction);
        }
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentTransactionResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpPost("{paymenttransactionuid}/BetOnAuction")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Paymenttransactions" }
        )]
        public async Task<IActionResult> BetOnAuction([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey,
            string paymenttransactionuid, [FromBody] BuyerClass submittransaction)
        {
            return await ProceedTransaction(apikey,paymenttransactionuid, ProceedPaymentTransactionCommands.BetOnAuction, submittransaction);
        }
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentTransactionResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status423Locked, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpPost("{paymenttransactionuid}/BuyDirectsale")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Paymenttransactions" }
        )]
        public async Task<IActionResult> BuyDirectsale([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey,
            string paymenttransactionuid, [FromBody] BuyerClass submittransaction)
        {
            return await ProceedTransaction(apikey,paymenttransactionuid, ProceedPaymentTransactionCommands.BuyDirectSale, submittransaction);
        }
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentTransactionResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status423Locked, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{paymenttransactionuid}/GetBuyoutSmartcontractAddress")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Paymenttransactions" }
        )]
        public async Task<IActionResult> GetBuyoutSmartcontractAddress([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey,
            string paymenttransactionuid)
        {
            return await ProceedTransaction(apikey,paymenttransactionuid, ProceedPaymentTransactionCommands.GetBuyoutSmartcontractAddress, null);
        }
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentTransactionResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status423Locked, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpPost("{paymenttransactionuid}/SellDirectsaleOffer")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Paymenttransactions" }
        )]
        public async Task<IActionResult> SellDirectsaleOffer([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey,
            string paymenttransactionuid, [FromBody] SellerClass submittransaction)
        {
            return await ProceedTransaction(apikey,paymenttransactionuid, ProceedPaymentTransactionCommands.SellDirectSaleOffer, submittransaction);
        }

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentTransactionResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status423Locked, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{paymenttransactionuid}/EndTransaction")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Paymenttransactions" }
        )]
        public async Task<IActionResult> EndTransaction([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey,
            string paymenttransactionuid)
        {
            return await ProceedTransaction(apikey,paymenttransactionuid, ProceedPaymentTransactionCommands.EndTransaction);
        }

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentTransactionResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status423Locked, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{paymenttransactionuid}/ReservePaymentgatewayMintAndSendNft")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Paymenttransactions" }
        )]
        public async Task<IActionResult> ReservePaymentgatewayMintAndSendNft([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey,
            string paymenttransactionuid, [FromQuery]int? reservationTimeInMinutes=null)
        {
            return await ProceedTransaction( apikey,paymenttransactionuid, ProceedPaymentTransactionCommands.ReservePaymentgatewayMintAndSendNft, reservationTimeInMinutes);
        }
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentTransactionResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpPost("{paymenttransactionuid}/MintAndSendPaymentgatewayNft")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Paymenttransactions" }
        )]
        public async Task<IActionResult> MintAndSendPaymentgatewayNft([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey,
            string paymenttransactionuid, [FromBody] MintAndSendReceiverClass submittransaction)
        {
            return await ProceedTransaction(apikey,paymenttransactionuid, ProceedPaymentTransactionCommands.MintAndSendPaymentgatewayNft, submittransaction);
        }

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaymentTransactionResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpPost("{paymenttransactionuid}/UpdateCustomProperties")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Paymenttransactions" }
        )]
        public async Task<IActionResult> UpdateCustomProperties([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey,
            string paymenttransactionuid, [FromBody] Dictionary<string,string> submittransaction)
        {
            return await ProceedTransaction(apikey,paymenttransactionuid, ProceedPaymentTransactionCommands.UpdateCustomProperties, submittransaction);
        }

        private async Task<IActionResult> ProceedTransaction(string apikey, string paymenttransactionuid, ProceedPaymentTransactionCommands command, object postparameter=null)
        {
           // string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

           
            if (Request.Method.Equals("HEAD"))
                return null;

          

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress ?? IPAddress.None;
            string apifunction = this.GetType().Name;

            var result = CheckCachedAccess.CheckApikeyOrToken(_redis, apifunction,
               "", apikey, remoteIpAddress?.ToString());


            if (result.ResultState != ResultStates.Ok)
            {
                return Unauthorized(result);
            }

            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            var preparedfast = await (from a in db.Preparedpaymenttransactions
                where a.Transactionuid == paymenttransactionuid
                select a).AsNoTracking().FirstOrDefaultAsync();

            await GlobalFunctions.LogMessageAsync(db, "Api-Call: ProceedPaymentTransaction - "+command,
                JsonConvert.SerializeObject(postparameter,
                    new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }));

            if (preparedfast == null)
            {
                result.ErrorCode = 1101;
                result.ErrorMessage = "Transaction UID not found";
                result.ResultState = ResultStates.Error;
                await GlobalFunctions.LogMessageAsync(db, "API: ProceedPaymentTransaction - Transaction UID not found (fast)", paymenttransactionuid + " "+ remoteIpAddress + " "+apikey + " "+command);
                await db.Database.CloseConnectionAsync();
                return StatusCode(404, result);
            }

            if (preparedfast.State == nameof(PaymentTransactionsStates.error) && command != ProceedPaymentTransactionCommands.GetTransactionState)
            {
                result.ErrorCode = 1111;
                result.ErrorMessage = "Transaction has errors";
                result.ResultState = ResultStates.Error;
                await GlobalFunctions.LogMessageAsync(db, "API: ProceedPaymentTransaction - Transaction has errors", paymenttransactionuid + " " + remoteIpAddress + " "+ apikey + " " + command + Environment.NewLine + preparedfast.State.ToString());
                await db.Database.CloseConnectionAsync();
                return StatusCode(406, result);
            }


            // Get Transactionstate always cached
            if (command == ProceedPaymentTransactionCommands.GetTransactionState)
            {
                var data1=RedisFunctions.GetData<PaymentTransactionResultClass>(_redis, "TransactionState_"+ paymenttransactionuid);
                if (data1 != null)
                {
                    await db.Database.CloseConnectionAsync();
                    return Ok(data1);
                }

                var data = StaticTransactionFunctions.GetTransactionState(db, _redis, preparedfast.Transactionuid, false);
                RedisFunctions.SetData(_redis, "TransactionState_" + paymenttransactionuid,
                                       data, 30);
                await db.Database.CloseConnectionAsync();
                return Ok(data);

            }


            switch (preparedfast.State)
            {
                case nameof(PaymentTransactionsStates.canceled):
                    result.ErrorCode = 1112;
                    result.ErrorMessage = "Transaction is already canceled";
                    result.ResultState = ResultStates.Error;
                    await db.Database.CloseConnectionAsync();
                    return StatusCode(404, result);
                case nameof(PaymentTransactionsStates.expired):
                    result.ErrorCode = 1102;
                    result.ErrorMessage = "Transaction UID is expired";
                    result.ResultState = ResultStates.Error;
                    await db.Database.CloseConnectionAsync();
                    return StatusCode(404, result);
            }

            var preparedtransaction = await (from a in db.Preparedpaymenttransactions
                    .Include(a => a.PreparedpaymenttransactionsCustomproperties)
                    .AsSplitQuery()
                    .Include(a => a.PreparedpaymenttransactionsNfts)
                    .ThenInclude(a => a.Nft)
                    .AsSplitQuery()
                    .Include(a => a.Nftproject)
                    .ThenInclude(a => a.Smartcontractssettings)
                    .Include(a => a.Nftproject)
                    .ThenInclude(a => a.Settings)
                    .Include(a => a.Nftaddresses)
                    .AsSplitQuery()
                    .Include(a => a.PreparedpaymenttransactionsSmartcontractsjsons)
                    .Include(a => a.Smartcontracts)
                    .ThenInclude(a => a.Smartcontractsjsontemplates)
                    .Include(a => a.Legacyauctions)
                    .AsSplitQuery()
                    .Include(a => a.Legacydirectsales)
                    .AsSplitQuery()
                    .Include(a => a.Mintandsend)
                    .AsSplitQuery()
                    .Include(a => a.PreparedpaymenttransactionsTokenprices)
                    .AsSplitQuery()
                    .Include(a=>a.PreparedpaymenttransactionsSmartcontractOutputs)
                    .ThenInclude(a=>a.PreparedpaymenttransactionsSmartcontractOutputsAssets)
                    .Include(a => a.Nftproject)
                    .ThenInclude(a => a.Customerwallet)
                    .AsSplitQuery()
                    .Include(a=>a.Buyoutaddresses)
                    .ThenInclude(a=>a.BuyoutsmartcontractaddressesNfts)
                    .AsSplitQuery()
                where a.Transactionuid == paymenttransactionuid
                select a).FirstOrDefaultAsync();

            if (preparedtransaction == null)
            {
                result.ErrorCode = 1152;
                result.ErrorMessage = "Transaction UID not found";
                result.ResultState = ResultStates.Error;
                await db.Database.CloseConnectionAsync();
                return StatusCode(404, result);
            }

            if (preparedtransaction.PreparedpaymenttransactionsSmartcontractsjsons != null)
            {
                // Delete the last action, if the transaction is too old
                var lastjson = preparedtransaction.PreparedpaymenttransactionsSmartcontractsjsons.LastOrDefault();
                if (lastjson != null)
                {
                    if ((lastjson.Templatetype == nameof(DatumTemplateTypes.bet)) &&
                        lastjson.Created < DateTime.Now.AddMinutes(-3) && lastjson.Submitted == null &&
                        preparedtransaction.Smartcontractstate ==
                        nameof(PaymentTransactionSubstates.readytosignbybuyer) &&
                        preparedtransaction.Transactiontype ==
                        nameof(PaymentTransactionTypes.smartcontract_auction) &&
                        command != ProceedPaymentTransactionCommands.SubmitTransaction
                       )
                    {
                        preparedtransaction.PreparedpaymenttransactionsSmartcontractsjsons.Remove(lastjson);
                        db.PreparedpaymenttransactionsSmartcontractsjsons.Remove(lastjson);
                        preparedtransaction.Smartcontractstate = nameof(PaymentTransactionSubstates.waitingforbid);
                        await db.SaveChangesAsync();
                    }

                    if ((lastjson.Templatetype == nameof(DatumTemplateTypes.buy)) &&
                        lastjson.Created < DateTime.Now.AddMinutes(-3) && lastjson.Submitted == null &&
                        preparedtransaction.Smartcontractstate ==
                        nameof(PaymentTransactionSubstates.readytosignbybuyer) &&
                        preparedtransaction.Transactiontype ==
                        nameof(PaymentTransactionTypes.smartcontract_directsale) &&
                        command != ProceedPaymentTransactionCommands.SubmitTransaction
                       )
                    {
                        preparedtransaction.PreparedpaymenttransactionsSmartcontractsjsons.Remove(lastjson);
                        db.PreparedpaymenttransactionsSmartcontractsjsons.Remove(lastjson);
                        preparedtransaction.Smartcontractstate = nameof(PaymentTransactionSubstates.waitingforsale);
                        await db.SaveChangesAsync();
                    }
                }
            }

            IProcessPaymentTransactionInterface tx=null;
            switch (command)
            {
                case ProceedPaymentTransactionCommands.CheckPaymentAddress:
                    tx = new CheckPaymentAddressClass(_redis);
                    break;
                case ProceedPaymentTransactionCommands.GetPaymentAddress:
                    tx = new GetPaymentAddressClass(_redis);
                    break;
                case ProceedPaymentTransactionCommands.SignDecentralPayment
                    when (preparedtransaction.Transactiontype ==
                          nameof(PaymentTransactionTypes.decentral_mintandsend_random) ||
                          preparedtransaction.Transactiontype ==
                          nameof(PaymentTransactionTypes.decentral_mintandsend_specific)):
                    tx = new SignDecentralPaymentMintAndSendClass(_redis);
                    break;
                case ProceedPaymentTransactionCommands.SignDecentralPayment
                    when (preparedtransaction.Transactiontype ==
                          nameof(PaymentTransactionTypes.decentral_mintandsale_random) ||
                          preparedtransaction.Transactiontype ==
                          nameof(PaymentTransactionTypes.decentral_mintandsale_specific)):
                    tx = new SignDecentralPaymentMintAndSaleClass(_redis);
                    break;
                case ProceedPaymentTransactionCommands.CancelTransaction
                    when (preparedtransaction.Transactiontype ==
                          nameof(PaymentTransactionTypes.paymentgateway_nft_specific) ||
                          preparedtransaction.Transactiontype ==
                          nameof(PaymentTransactionTypes.paymentgateway_nft_random)):
                    tx = new CancelPaymentTransactionClass(_redis,_bus);
                    break;
                case ProceedPaymentTransactionCommands.CancelTransaction
                    when (preparedtransaction.Transactiontype ==
                          nameof(PaymentTransactionTypes.nmkr_pay_specific) ||
                          preparedtransaction.Transactiontype ==
                          nameof(PaymentTransactionTypes.nmkr_pay_random)):
                    tx = new CancelNmkrPayTransactionClass(_redis);
                    break;
                case ProceedPaymentTransactionCommands.CancelTransaction
                    when (preparedtransaction.Transactiontype ==
                          nameof(PaymentTransactionTypes.decentral_mintandsale_random) ||
                          preparedtransaction.Transactiontype ==
                          nameof(PaymentTransactionTypes.decentral_mintandsend_random) ||
                          preparedtransaction.Transactiontype ==
                          nameof(PaymentTransactionTypes.decentral_mintandsend_specific) ||
                          preparedtransaction.Transactiontype ==
                          nameof(PaymentTransactionTypes.decentral_mintandsale_specific)):
                    tx = new CancelDecentralPaymentClass(_redis);
                    break;
                case ProceedPaymentTransactionCommands.CancelTransaction
                    when (preparedtransaction.Transactiontype ==
                          nameof(PaymentTransactionTypes.paymentgateway_mintandsend_random) ||
                          preparedtransaction.Transactiontype ==
                          nameof(PaymentTransactionTypes.paymentgateway_mintandsend_specific)):
                    tx = new CancelPaymentgatewayMintAndSendClass(_redis);
                    break;
                case ProceedPaymentTransactionCommands.CancelTransaction when preparedtransaction.Transactiontype ==
                                                                              nameof(PaymentTransactionTypes.smartcontract_directsale):
                    tx = new CancelSmartContractDirectSaleTransactionClass(_redis);
                    break;
                case ProceedPaymentTransactionCommands.CancelTransaction when preparedtransaction.Transactiontype ==
                                                                              nameof(PaymentTransactionTypes.smartcontract_directsale_offer):
                    tx = new CancelSmartContractDirectSaleOfferTransactionClass(_redis);
                    break;
                case ProceedPaymentTransactionCommands.CancelTransaction when preparedtransaction.Transactiontype ==
                                                                              nameof(PaymentTransactionTypes.legacy_directsale):
                    tx = new CancelLegacyDirectSaleTransactionClass(_redis);
                    break;
                case ProceedPaymentTransactionCommands.ExtendReservationTime:
                    tx = new ExtendReservationTimeClass(_redis);
                    break;
                case ProceedPaymentTransactionCommands.GetPriceListForProject:
                    tx = new GetPriceListFoProjectClass(_redis);
                    break;
                case ProceedPaymentTransactionCommands.LockNft
                    when (preparedtransaction.Transactiontype ==
                          nameof(PaymentTransactionTypes.smartcontract_directsale) ||
                          preparedtransaction.Transactiontype ==
                          nameof(PaymentTransactionTypes.smartcontract_auction)):
                    tx = new LockNftOnSmartContractClass(_redis);
                    break;
                case ProceedPaymentTransactionCommands.GetBuyoutSmartcontractAddress
                    when (preparedtransaction.Transactiontype ==
                          nameof(PaymentTransactionTypes.smartcontract_directsale)):
                    tx = new GetBuyOutSmartcontractAddress(_redis);
                    break;
                case ProceedPaymentTransactionCommands.LockAda:
                    tx = new LockAdaOnSmartContractClass(_redis);
                    break;
                case ProceedPaymentTransactionCommands.LockNft
                    when (preparedtransaction.Transactiontype == nameof(PaymentTransactionTypes.legacy_auction) ||
                          preparedtransaction.Transactiontype == nameof(PaymentTransactionTypes.legacy_directsale)):
                    tx = new LockNftOnLegacyTransactionsClass(_redis);
                    break;
                case ProceedPaymentTransactionCommands.SubmitTransaction
                    when (preparedtransaction.Transactiontype ==
                          nameof(PaymentTransactionTypes.paymentgateway_nft_specific) ||
                          preparedtransaction.Transactiontype ==
                          nameof(PaymentTransactionTypes.paymentgateway_nft_random)):
                    tx = new SubmitPaymentgatewayPaymentTransactionClass(_redis);
                    break;
                case ProceedPaymentTransactionCommands.SubmitTransaction
                    when (preparedtransaction.Transactiontype ==
                          nameof(PaymentTransactionTypes.smartcontract_auction) ||
                          preparedtransaction.Transactiontype ==
                          nameof(PaymentTransactionTypes.smartcontract_directsale) ||
                          preparedtransaction.Transactiontype == nameof(PaymentTransactionTypes.legacy_auction) ||
                          preparedtransaction.Transactiontype == nameof(PaymentTransactionTypes.legacy_directsale) ||
                          preparedtransaction.Transactiontype==nameof(PaymentTransactionTypes.smartcontract_directsale_offer)):
                    tx = new SubmitSmartContractPaymentTransactionClass(_redis);
                    break;
                case ProceedPaymentTransactionCommands.SubmitTransaction
                    when (preparedtransaction.Transactiontype ==
                          nameof(PaymentTransactionTypes.decentral_mintandsend_random) ||
                          preparedtransaction.Transactiontype ==
                          nameof(PaymentTransactionTypes.decentral_mintandsend_specific)):
                    tx = new SubmitDecentralPaymentTransactionClass(_redis);
                    break;
                case ProceedPaymentTransactionCommands.SubmitTransaction
                    when (preparedtransaction.Transactiontype ==
                          nameof(PaymentTransactionTypes.decentral_mintandsale_random) ||
                          preparedtransaction.Transactiontype ==
                          nameof(PaymentTransactionTypes.decentral_mintandsale_specific)):
                    tx = new SubmitDecentralPaymentTransactionClass(_redis);
                    break;
                case ProceedPaymentTransactionCommands.BetOnAuction when (preparedtransaction.Transactiontype ==
                                                                          nameof(PaymentTransactionTypes
                                                                              .smartcontract_auction)):
                    tx = new BetOnSmartContractAuctionClass(_redis);
                    break;
                case ProceedPaymentTransactionCommands.BetOnAuction when (preparedtransaction.Transactiontype ==
                                                                          nameof(PaymentTransactionTypes
                                                                              .legacy_auction)):
                    tx = new BetOnLegacyAuctionClass(_redis);
                    break;
                case ProceedPaymentTransactionCommands.BuyDirectSale when (preparedtransaction.Transactiontype ==
                                                                           nameof(PaymentTransactionTypes
                                                                               .smartcontract_directsale)):
                    tx = new BuyDirectsaleSmartContractClass(_redis);
                    break;
                case ProceedPaymentTransactionCommands.SellDirectSaleOffer when (preparedtransaction.Transactiontype ==
                    nameof(PaymentTransactionTypes
                        .smartcontract_directsale_offer)):
                    tx = new SellDirectsaleOfferSmartContractClass(_redis);
                    break;
                case ProceedPaymentTransactionCommands.BuyDirectSale when (preparedtransaction.Transactiontype ==
                                                                           nameof(PaymentTransactionTypes
                                                                               .legacy_directsale)):
                    tx = new BuyDirectsaleLegacyClass(_redis);
                    break;
                case ProceedPaymentTransactionCommands.EndTransaction when (preparedtransaction.Transactiontype ==
                                                                            nameof(PaymentTransactionTypes.smartcontract_auction) ||
                                                                            preparedtransaction.Transactiontype ==
                                                                            nameof(PaymentTransactionTypes.smartcontract_directsale)):
                    tx = new EndSmartContractTransactionClass(_redis);
                    break;
                case ProceedPaymentTransactionCommands.ReservePaymentgatewayMintAndSendNft:
                    tx = new ReservePaymentgatewayMintAndSendNftClass(_redis);
                    break;
                case ProceedPaymentTransactionCommands.MintAndSendPaymentgatewayNft:
                    tx = new MintAndSendPaymentgatewayNftClass(_redis);
                    break;
                case ProceedPaymentTransactionCommands.UpdateCustomProperties:
                    tx = new UpdateCustomPropertiesClass(_redis);
                    break;
            }

            if (tx!=null)
                return await tx.ProcessTransaction(db,apikey, remoteIpAddress.ToString(), result, preparedtransaction, postparameter);


            result.ErrorCode = 1100;
            result.ErrorMessage = "State not known";
            result.ResultState = ResultStates.Error;
            await db.Database.CloseConnectionAsync();
            return StatusCode(500, result);
        }

    }
}
