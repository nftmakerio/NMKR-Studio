using System;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Api.Controllers.SharedClasses;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.Api;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;
using NMKR.Shared.Enums;

namespace NMKR.Api.Controllers.v2.SmartContracts
{
    /// <summary>
    /// Returns an address to buy out a directsale smartcontract
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class GetBuyOutSmartcontractAddressController : Controller
    {
        private readonly IConnectionMultiplexer _redis;

        /// <summary>
        /// Returns an address to buy out a directsale smartcontract
        /// </summary>
        /// <param name="redis"></param>
        public GetBuyOutSmartcontractAddressController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetPaymentAddressResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{customerid}/{txHashLockedinAssets}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] {"Smartcontracts"}
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, int customerid,
            string txHashLockedinAssets)
        {
          //  string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            if (Request.Method.Equals("HEAD"))
                return null;

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;

            string apifunction = this.GetType().Name;
            string apiparameter = txHashLockedinAssets;

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
                return StatusCode(cachedResult.Statuscode,
                    JsonConvert.DeserializeObject<ApiErrorResultClass>(cachedResult.ResultString));


            // Check if the Apikey is valid
            var result = CheckCachedAccess.CheckApikeyOrToken(_redis, apifunction,
               "", apikey, remoteIpAddress?.ToString());
            if (result.ResultState != ResultStates.Ok)
            {
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                return Unauthorized(result);
            }

            if (!txHashLockedinAssets.Contains("#"))
            {
                result.ErrorCode = 4433;
                result.ErrorMessage = "TXHash neeeds the TxHash separated by #";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }


            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);


            var customer = await (from a in db.Customers
                where a.Id == customerid
                select a).FirstOrDefaultAsync();

            if (customer == null)
            {
                result.ErrorCode = 4436;
                result.ErrorMessage = "Customer not found";
                result.ResultState = ResultStates.Error;
                return StatusCode(404, result);
            }


            CreatePaymentTransactionClass cptc = new CreatePaymentTransactionClass()
            {
                ProjectUid = await StaticTransactionFunctions.GetDefaultProjectUid(db, "directsaleV2"),
                PaymentTransactionType = PaymentTransactionTypes.smartcontract_directsale,
                DirectSaleParameters = new DirectSaleParameterClass()
                    {TxHashForAlreadyLockedinAssets = txHashLockedinAssets},
                CustomerIpAddress = remoteIpAddress.ToString()
            };


            var pt = await ApiFunctions.CallCreatePaymentTransactionAsync(cptc);

            if (pt == null)
            {
                result.ErrorCode = 4432;
                result.ErrorMessage = "Can not create the Transaction";
                result.ResultState = ResultStates.Error;
                return StatusCode(500, result);
            }

            var cn = ConsoleCommand.CreateNewPaymentAddress(GlobalFunctions.IsMainnet());
            if (cn.ErrorCode != 0)
            {
                result.ErrorCode = cn.ErrorCode;
                result.ErrorMessage = cn.ErrorMessage;
                result.ResultState = ResultStates.Error;
                return StatusCode(500,result);
            }

            CryptographyProcessor cp = new();
            string salt = cp.CreateSalt(30);
            string password = salt + GeneralConfigurationClass.Masterpassword;

            Buyoutsmartcontractaddress bsca = new Buyoutsmartcontractaddress()
            {
                Transactionid = pt.PaymentTransactionUid, Expiredate = DateTime.Now.AddMinutes(30),
                Lovelace = pt.DirectSaleResults.SellingPrice + pt.DirectSaleResults.LockedInAmount,
                Lockamount = pt.DirectSaleResults.LockedInAmount,
                Additionalamount = 2000000,
                Smartcontracttxhash = txHashLockedinAssets, State = "active", Address = cn.Address,
                Skey = Encryption.EncryptString(cn.privateskey, password),
                Vkey = Encryption.EncryptString(cn.privatevkey, password), CustomerId = customerid, Salt = salt,
            };
            await db.Buyoutsmartcontractaddresses.AddAsync(bsca);
            await db.SaveChangesAsync();

            foreach (var smartcontractDirectsaleReceiverClass in pt.DirectSaleResults.Receivers)
            {
                BuyoutsmartcontractaddressesReceiver bscar=new BuyoutsmartcontractaddressesReceiver()
                {
                    Lovelace = smartcontractDirectsaleReceiverClass.AmountInLovelace, 
                    Receiveraddress = smartcontractDirectsaleReceiverClass.Address, 
                    BuyoutsmartcontractaddressesId = bsca.Id, 
                    Pkh = smartcontractDirectsaleReceiverClass.Pkh, 
                };
                await db.BuyoutsmartcontractaddressesReceivers.AddAsync(bscar);
                await db.SaveChangesAsync();
            }

            var rates = await GlobalFunctions.GetNewRatesAsync(_redis, Coin.ADA);
            GetPaymentAddressResultClass pnrc = new()
            {
                Expires = bsca.Expiredate,
                PaymentAddress = bsca.Address,
                Debug = "",

                PriceInEur = (float)Math.Round((rates.EurRate * (bsca.Lovelace+bsca.Additionalamount ) / 1000000), 2),
                PriceInUsd = (float)Math.Round(((rates.UsdRate) * (bsca.Lovelace + bsca.Additionalamount ) / 1000000), 2),
                PriceInJpy = (float)Math.Round(((rates.JpyRate) * (bsca.Lovelace + bsca.Additionalamount ) / 1000000), 2),

                PriceInLovelace = (long)(bsca.Lovelace + bsca.Additionalamount ),
                Effectivedate = rates.EffectiveDate,
                SendbackToUser = bsca.Additionalamount + bsca.Lockamount,
                Revervationtype = "specific"
            };


            return Ok(pnrc);
        }

      
    }
}
