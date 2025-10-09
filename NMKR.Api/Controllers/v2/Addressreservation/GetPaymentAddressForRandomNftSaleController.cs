using System.Threading.Tasks;
using Asp.Versioning;
using CardanoSharp.Wallet.Enums;
using NMKR.Api.Controllers.SharedClasses;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;

namespace NMKR.Api.Controllers.v2.Addressreservation
{

    /// <summary>
    /// Returns an address for a random nft sale (project id)
    /// </summary>
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class GetPaymentAddressForRandomNftSaleController : Controller
    {
        private readonly IConnectionMultiplexer _redis;

        /// <summary>
        /// Returns an address for a random nft sale (project id) Constructor
        /// </summary>
        /// <param name="redis"></param>
        public GetPaymentAddressForRandomNftSaleController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        [HttpGet("{projectuid}/{countnft:int}/{price:long}/{customeripaddress}")]
        [MapToApiVersion("2")]
        [ApiExplorerSettingsAttribute(IgnoreApi = true)]
        [SwaggerOperation(
            Tags = new[] {"Address reservation (sale)"}
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore] [FromHeader(Name = "authorization")] string apikey,
            string projectuid, long countnft, long price, string customeripaddress,
            [FromQuery] string? referer,
            [FromQuery] string? customproperty,
            [FromQuery] string? optionalreceiveraddress,
            [FromQuery] string? optionalpriceintokenpolicyid,
            [FromQuery] string? optionalpriceintokenassetnameinhex,
            [FromQuery] long? optionalpriceintokencount,
            [FromQuery] string? optionalrefundaddress,
            [FromQuery] bool? acceptheigheramounts,
            [FromQuery] uint? revervationtimeinminutes,
            [FromQuery] AddressType addresstype = AddressType.Enterprise,
            [FromQuery] Blockchain blockchain = Blockchain.Cardano)
        {
            //  string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");


            ReserveAddressQueueClass raqc = new()
            {
                ApiKey = apikey,
                NftprojectUId = projectuid,
                CountNft = countnft,
                CardanoLovelace = price,
                Price= price,
                RemoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                CustomerIpAddress = customeripaddress,
                Referer = referer,
                CustomProperty = customproperty,
                OptionalReceiverAddress = optionalreceiveraddress,
                TokenPolicyId = optionalpriceintokenpolicyid,
                TokenAssetIdHex = optionalpriceintokenassetnameinhex,
                TokenAssetId = optionalpriceintokenassetnameinhex.FromHex(),
                PriceInToken = optionalpriceintokencount,
                OptionalRefundAddress = optionalrefundaddress,
                AcceptHeigherAmounts = acceptheigheramounts,
                Addresstype = addresstype,
                Coin = GlobalFunctions.ConvertToCoin(blockchain),
                ReservationTimeInMinutes = revervationtimeinminutes
            };


            var result = await ReserveRandomNftByApiClass.RequestRandomAddress(_redis, raqc);
            return result.StatusCode == 0 ? Ok(result.SuccessResult) : StatusCode(result.StatusCode, result.ApiError);
        }

        /// <summary>
        /// Returns an address for a random nft sale (project id)
        /// </summary>
        /// <remarks>
        /// When you call this API, you will receive an address where the buyer has to pay the amount of ada you define. The address will be monitored until it expired. The count of nft will be reserved until it expires or the buyer has send the ada to this address.
        /// If the buyer has send the amount of ada, the nfts will be minted and send to his senderaddress and the nfts state changes to sold.
        /// 
        /// IMPORTANT:
        /// Please notice, that the call is limited to 300 address reservations per minute. You will get the error 429 if you call this routine more than 300 times a minute.
        /// Please do not implement this function on your start page. And please prevent the call of this function from bots with a captcha.
        /// </remarks>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="projectuid">The uid of your project</param>
        /// <param Name="countnft">The count of the nft/tokens you want to mint and send.</param>
        /// <param Name="price">The amount the buyer has to send in lovelace (ADA) - so 1 ADA is 1000000 lovelace or in lamports (SOL) or in Octas (APT)</param>
        /// <param Name="optionalrefundaddress">The address where the ada will be refunded if the buyer has send more ada than demanded or if the transaction fails</param>
        /// <param Name="acceptheigheramounts">Accept higher amounts then demanded - they will be refunded to the customer or the optional refundaddress</param>
        /// <param name="referer">(Optional) A referer code</param>
        /// <param name="customproperty">(Optional) A custom property which can be set. Will be returned at webhooks or checkaddress</param>
        /// <param name="optionalreceiveraddress">(Optional) You can specify a different receiver of the nft</param>
        /// <param name="optionalpriceintokenpolicyid">(Optional) You can specify an additional price in tokens (policyid)</param>
        /// <param name="optionalpriceintokenassetnameinhex">(Optional) You can specify an additional price in tokens (tokenname in hex)</param>
        /// <param name="optionalpriceintokencount">(Optional) You can specify an additional price in tokens (count)</param>
        /// <param name="optionalrefundaddress">(Optional) If there was refund because of minting error or saleconditions you can specify the receiver of the ada/sol </param>
        /// <param name="revervationtimeinminutes">(Optional) The time in minutes the address will be reserved</param>
        /// <param name="acceptheigheramounts">(Optional) The address accepts the correct or a higher amount for minting. If false or null the exact amount must be received</param>
        /// <param name="addresstype">The Base Addresses are with Stakekeys, the Enterprise Addresses without - only available in the cardano network</param>
        /// <param name="blockchain">The blockchain where the address should be created</param>
        /// <response code="200">Returns the GetPaymentAddressResultClass Class</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="404">No more nft available</response>            
        /// <response code="406">The demanded ada amount is too less. The minimium is 5 ADA - eg 5000000 lovelace</response>
        /// <response code="500">Internal server error - see the errormessage in the result</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetPaymentAddressResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{projectuid}/{countnft:int}/{price:long}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] {"Address reservation (sale)"}
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore] [FromHeader(Name = "authorization")] string apikey,
            string projectuid, long countnft, long price,
            [FromQuery] string? referer,
            [FromQuery] string? customproperty,
            [FromQuery] string? optionalreceiveraddress,
            [FromQuery] string? optionalpriceintokenpolicyid,
            [FromQuery] string? optionalpriceintokenassetnameinhex,
            [FromQuery] long? optionalpriceintokencount,
            [FromQuery] string? optionalrefundaddress,
            [FromQuery] bool? acceptheigheramounts,
            [FromQuery] uint? revervationtimeinminutes,
            [FromQuery] AddressType addresstype = AddressType.Enterprise,
            [FromQuery] Blockchain blockchain = Blockchain.Cardano)
        {
            //  string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");


            ReserveAddressQueueClass raqc = new()
            {
                ApiKey = apikey,
                NftprojectUId = projectuid,
                CountNft = countnft,
                CardanoLovelace = price,
                Price = price,
                RemoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                CustomerIpAddress = "127.0.0.1",
                Referer = referer,
                CustomProperty = customproperty,
                OptionalReceiverAddress = optionalreceiveraddress,
                TokenPolicyId = optionalpriceintokenpolicyid,
                TokenAssetIdHex = optionalpriceintokenassetnameinhex,
                TokenAssetId = optionalpriceintokenassetnameinhex.FromHex(),
                PriceInToken = optionalpriceintokencount,
                OptionalRefundAddress = optionalrefundaddress,
                AcceptHeigherAmounts = acceptheigheramounts,
                Addresstype = addresstype,
                Coin = GlobalFunctions.ConvertToCoin(blockchain),
                ReservationTimeInMinutes = revervationtimeinminutes
            };


            var result = await ReserveRandomNftByApiClass.RequestRandomAddress(_redis, raqc);
            return result.StatusCode == 0 ? Ok(result.SuccessResult) : StatusCode(result.StatusCode, result.ApiError);
        }



        [HttpGet("{projectuid}/{countnft:int}/{customeripaddress}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] {"Address reservation (sale)"}
        )]
        [ApiExplorerSettingsAttribute(IgnoreApi = true)]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore] [FromHeader(Name = "authorization")] string apikey,
            string projectuid, long countnft, string customeripaddress,
            [FromQuery] string? referer,
            [FromQuery] string? customproperty,
            [FromQuery] string? optionalreceiveraddress,
            [FromQuery] string? optionalrefundaddress,
            [FromQuery] bool? acceptheigheramounts,
            [FromQuery] uint? revervationtimeinminutes,
            [FromQuery] AddressType addresstype = AddressType.Enterprise,
            [FromQuery] Blockchain blockchain = Blockchain.Cardano)
        {
            //   string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");


            ReserveAddressQueueClass raqc = new()
            {
                ApiKey = apikey,
                NftprojectUId = projectuid,
                CountNft = countnft,
                CardanoLovelace = null,
                Price = null,
                RemoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                CustomerIpAddress = customeripaddress,
                Referer = referer,
                CustomProperty = customproperty,
                OptionalReceiverAddress = optionalreceiveraddress,
                OptionalRefundAddress = optionalrefundaddress,
                AcceptHeigherAmounts = acceptheigheramounts,
                Addresstype = addresstype,
                Coin = GlobalFunctions.ConvertToCoin(blockchain),
                ReservationTimeInMinutes = revervationtimeinminutes
            };


            var result = await ReserveRandomNftByApiClass.RequestRandomAddress(_redis, raqc);
            return result.StatusCode == 0 ? Ok(result.SuccessResult) : StatusCode(result.StatusCode, result.ApiError);
        }

        /// <summary>
        /// Returns an address for a random nft sale (price from pricelist) (project id)
        /// </summary>
        /// <remarks>
        /// When you call this API, you will receive an address where the buyer has to pay the amount of ada you define. The address will be monitored until it exipred. The count of nft will be reserved until it expires or the buyer has send the ada to this address.
        /// If the buyer has send the amount of ada, the nfts will be minted and send to his senderaddress and the nfts state changes to sold.
        /// 
        /// IMPORTANT:
        /// Please notice, that the call is limited to 300 addressreservations per minute. You will get the error 429 if you call this routine more than 300 times a minute.
        /// Please do not implement this function on your start page. And please prevent the call of this function from bots with a captcha.
        /// </remarks>
        /// <param Name="apikey">The apikey you have created on NMKR Studio</param>
        /// <param Name="projectuid">The uid of your project</param>
        /// <param Name="countnft">The count of the nft/tokens you want to mint and send.</param>
        /// <param Name="optionalrefundaddress">The address where the ada will be refunded if the buyer has send more ada than demanded or if the transaction fails</param>
        /// <param Name="acceptheigheramounts">Accept higher amounts then demanded - they will be refunded to the customer or the optional refundaddress</param>
        /// <param name="referer">(Optional) A referer code</param>
        /// <param name="customproperty">(Optional) A custom property which can be set. Will be returned at webhooks or checkaddress</param>
        /// <param name="optionalreceiveraddress">(Optional) You can specify a different receiver of the nft</param>
        /// <param name="optionalrefundaddress">(Optional) If there was refund because of minting error or saleconditions you can specify the receiver of the ada/sol </param>
        /// <param name="revervationtimeinminutes">(Optional) The time in minutes the address will be reserved</param>
        /// <param name="acceptheigheramounts">(Optional) The address accepts the correct or a higher amount for minting. If false or null the exact amount must be received</param>
        /// <param name="addresstype">The Base Addresses are with Stakekeys, the Enterprise Addresses without - only available in the cardano network</param>
        /// <param name="blockchain">The blockchain where the address should be created</param>
        /// <response code="200">Returns the GetPaymentAddressResultClass Class</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="404">No more nft available</response>            
        /// <response code="406">The demanded ada amount is too less. The minimium is 5 ADA - eg 5000000 lovelace</response>
        /// <response code="500">Internal server error - see the errormessage in the result</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetPaymentAddressResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{projectuid}/{countnft:int}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] {"Address reservation (sale)"}
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore] [FromHeader(Name = "authorization")] string apikey,
            string projectuid, long countnft,
            [FromQuery] string? referer,
            [FromQuery] string? customproperty,
            [FromQuery] string? optionalreceiveraddress,
            [FromQuery] string? optionalrefundaddress,
            [FromQuery] bool? acceptheigheramounts,
            [FromQuery] uint? revervationtimeinminutes,
            [FromQuery] AddressType addresstype = AddressType.Enterprise,
            [FromQuery] Blockchain blockchain = Blockchain.Cardano)
        {
            //   string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");


            ReserveAddressQueueClass raqc = new()
            {
                ApiKey = apikey,
                NftprojectUId = projectuid,
                CountNft = countnft,
                CardanoLovelace = null,
                Price = null,
                RemoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                CustomerIpAddress = "127.0.0.1",
                Referer = referer,
                CustomProperty = customproperty,
                OptionalReceiverAddress = optionalreceiveraddress,
                OptionalRefundAddress = optionalrefundaddress,
                AcceptHeigherAmounts = acceptheigheramounts,
                Addresstype = addresstype,
                Coin = GlobalFunctions.ConvertToCoin(blockchain),
                ReservationTimeInMinutes = revervationtimeinminutes
            };


            var result = await ReserveRandomNftByApiClass.RequestRandomAddress(_redis, raqc);
            return result.StatusCode == 0 ? Ok(result.SuccessResult) : StatusCode(result.StatusCode, result.ApiError);
        }
    }
}
