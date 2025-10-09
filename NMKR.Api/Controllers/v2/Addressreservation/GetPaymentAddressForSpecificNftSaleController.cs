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
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class GetPaymentAddressForSpecificNftSaleController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public GetPaymentAddressForSpecificNftSaleController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        [HttpGet("{nftuid}/{tokencount:long}/{price:long}/{customeripaddress}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Address reservation (sale)" }
        )]
        [ApiExplorerSettingsAttribute(IgnoreApi = true)]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string nftuid, long tokencount, long price, string customeripaddress, 
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

            ReserveMultipleNftsClassV2 reserve = new()
            {
                ReserveNfts = new[] { new ReserveNftsClassV2() { NftUid = nftuid, Tokencount = tokencount } }
            };

            ReserveAddressQueueClass raqc = new()
            {
                ApiKey = apikey,
                CardanoLovelace = price,
                Price=price,
                RemoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Referer = referer,
                Reservenfts = reserve,
                CustomerIpAddress = customeripaddress,
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
                CountNft = 1,
                ReservationTimeInMinutes = revervationtimeinminutes
            };


            var result = await ReserveSpecificNftByApiClass.RequestSpecificAddress(_redis, raqc);
            return result.StatusCode == 0 ? Ok(result.SuccessResult) : StatusCode(result.StatusCode, result.ApiError);
        }


        [HttpGet("{nftuid}/{tokencount:long}/{customeripaddress}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Address reservation (sale)" }
        )]
        [ApiExplorerSettingsAttribute(IgnoreApi = true)]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string nftuid, long tokencount, string customeripaddress, 
            [FromQuery] string? referer,
            [FromQuery] string? customproperty, 
            [FromQuery] string? optionalreceiveraddress,
            [FromQuery] string? optionalrefundaddress,
            [FromQuery] bool? acceptheigheramounts,
            [FromQuery] uint? revervationtimeinminutes,
            [FromQuery] AddressType addresstype = AddressType.Enterprise,
            [FromQuery] Blockchain blockchain = Blockchain.Cardano)
        {
          //  string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            ReserveMultipleNftsClassV2 reserve = new()
            {
                ReserveNfts = new[] { new ReserveNftsClassV2() { NftUid = nftuid, Tokencount = tokencount } }
            };

            ReserveAddressQueueClass raqc = new()
            {
                ApiKey = apikey,
                RemoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Referer = referer,
                Reservenfts = reserve,
                CustomerIpAddress = customeripaddress,
                CustomProperty = customproperty,
                OptionalReceiverAddress = optionalreceiveraddress,
                OptionalRefundAddress = optionalrefundaddress,
                AcceptHeigherAmounts = acceptheigheramounts,
                Addresstype = addresstype,
                Coin = GlobalFunctions.ConvertToCoin(blockchain),
                CountNft = 1,
                ReservationTimeInMinutes = revervationtimeinminutes
            };


            var result = await ReserveSpecificNftByApiClass.RequestSpecificAddress(_redis, raqc);
            return result.StatusCode == 0 ? Ok(result.SuccessResult) : StatusCode(result.StatusCode, result.ApiError);
        }




        [HttpPost("{customeripaddress}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Address reservation (sale)" }
        )]
        [ApiExplorerSettingsAttribute(IgnoreApi = true)]
        public async Task<IActionResult> Post([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string customeripaddress, 
            [FromBody] ReserveMultipleNftsClassV2 reservenfts, 
            [FromQuery] string? referer, 
            [FromQuery] string? customproperty, 
            [FromQuery] string? optionalreceiveraddress,
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
                RemoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Referer = referer,
                Reservenfts = reservenfts,
                CustomerIpAddress = customeripaddress,
                CustomProperty = customproperty,
                OptionalReceiverAddress = optionalreceiveraddress,
                OptionalRefundAddress = optionalrefundaddress,
                AcceptHeigherAmounts = acceptheigheramounts,
                Addresstype = addresstype,
                Coin = GlobalFunctions.ConvertToCoin(blockchain),
                CountNft = reservenfts.ReserveNfts.Length,
                ReservationTimeInMinutes = revervationtimeinminutes
            };


            var result = await ReserveSpecificNftByApiClass.RequestSpecificAddress(_redis, raqc);
            return result.StatusCode == 0 ? Ok(result.SuccessResult) : StatusCode(result.StatusCode, result.ApiError);
        }







        /// <summary>
        /// Returns an address for a specific nft sale (no random distribution) (project and nft id)
        /// </summary>
        /// <remarks>
        /// When you call this API, you will receive an address where the buyer has to pay the amount of ada you define. The address will be monitored until it exipred. The count of nft will be reserved until it expires or the buyer has send the ada to this address.
        /// If the buyer has send the amount of ada, the nfts will be minted and send to his senderaddress and the nfts state changes to sold.
        ///
        /// IMPORTANT:
        /// Please notice, that the call is limited to 300 addressreservations per minute. You will get the error 429 if you call this routine more than 300 times a minute.
        /// Please do not implement this function on your start page. And please prevent the call of this function from bots with a captcha.
        /// </remarks>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="nftuid">The uid of the nft you want to mint and send</param>
        /// <param Name="tokencount">The amount of tokens you want to send (only if multiple tokens available)</param>
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
        /// <response code="404">The id of the nft is not found or not assigned to this project</response>            
        /// <response code="406">The demanded ada amount is too less. The minimium is 5 ADA - eg 5000000 lovelace</response>
        /// <response code="409">There is a conflict with the selected nft. See errormessage in the resultset</response>
        /// <response code="500">Internal server error - see the errormessage in the resultset</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetPaymentAddressResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{nftuid}/{tokencount:long}/{price:long}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Address reservation (sale)" }
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string nftuid, long tokencount, long price, 
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

            ReserveMultipleNftsClassV2 reserve = new()
            {
                ReserveNfts = new[] { new ReserveNftsClassV2() { NftUid = nftuid, Tokencount = tokencount } }
            };

            ReserveAddressQueueClass raqc = new()
            {
                ApiKey = apikey,
                CardanoLovelace = price,
                Price= price,
                RemoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Referer = referer,
                Reservenfts = reserve,
                CustomerIpAddress = "127.0.0.1",
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
                CountNft = 1,
                ReservationTimeInMinutes = revervationtimeinminutes
            };


            var result = await ReserveSpecificNftByApiClass.RequestSpecificAddress(_redis, raqc);
            return result.StatusCode == 0 ? Ok(result.SuccessResult) : StatusCode(result.StatusCode, result.ApiError);
        }


        /// <summary>
        /// Returns an address for a specific nft sale (no random distribution) - price from pricelist or specific nft price (project and nft id)
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
        /// <param Name="nftuid">The uid of the nft you want to mint and send</param>
        /// <param Name="tokencount">The amount of tokens you want to send (only if multiple tokens available)</param>
        /// <param Name="price">The amount the buyer has to send in lovelace (ADA) - so 1 ADA is 1000000 lovelace or in lamports (SOL) or in Octas (APT)</param>
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
        /// <response code="404">The id of the nft is not found or not assigned to this project</response>            
        /// <response code="406">The demanded ada amount is too less. The minimium is 5 ADA - eg 5000000 lovelace</response>
        /// <response code="409">There is a conflict with the selected nft. See errormessage in the resultset</response>
        /// <response code="500">Internal server error - see the errormessage in the resultset</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetPaymentAddressResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{nftuid}/{tokencount:long}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Address reservation (sale)" }
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string nftuid, long tokencount, 
            [FromQuery] string? referer,
            [FromQuery] string? customproperty,
            [FromQuery] string? optionalreceiveraddress,
            [FromQuery] string? optionalrefundaddress,
            [FromQuery] bool? acceptheigheramounts,
            [FromQuery] uint? revervationtimeinminutes,
            [FromQuery] AddressType addresstype = AddressType.Enterprise,
            [FromQuery] Blockchain blockchain = Blockchain.Cardano)
        {
            //  string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            ReserveMultipleNftsClassV2 reserve = new()
            {
                ReserveNfts = new[] { new ReserveNftsClassV2() { NftUid = nftuid, Tokencount = tokencount } }
            };

            ReserveAddressQueueClass raqc = new()
            {
                ApiKey = apikey,
                RemoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Referer = referer,
                Reservenfts = reserve,
                CustomerIpAddress = "127.0.0.1",
                CustomProperty = customproperty,
                OptionalReceiverAddress = optionalreceiveraddress,
                OptionalRefundAddress = optionalrefundaddress,
                AcceptHeigherAmounts = acceptheigheramounts,
                Addresstype = addresstype,
                Coin = GlobalFunctions.ConvertToCoin(blockchain),
                CountNft = 1,
                ReservationTimeInMinutes = revervationtimeinminutes
            };


            var result = await ReserveSpecificNftByApiClass.RequestSpecificAddress(_redis, raqc);
            return result.StatusCode == 0 ? Ok(result.SuccessResult) : StatusCode(result.StatusCode, result.ApiError);
        }




        /// <summary>
        /// Returns an address for a multiple specific nfts sale (no random distribution) (project id)
        /// </summary>
        /// <remarks>
        /// When you call this API, you will receive an address where the buyer has to pay the amount of ada you define. The address will be monitored until it exipred. The count of nft will be reserved until it expires or the buyer has send the ada to this address.
        /// If the buyer has send the amount of ada, the nfts will be minted and send to his senderaddress and the nfts state changes to sold.
        /// </remarks>
        /// <param Name="apikey">The apikey you have created on NMKR Studio</param>
        /// <param Name="customeripaddress">The IP-Address from your Customer</param>
        /// <param Name="optionalrefundaddress">The address where the ada will be refunded if the buyer has send more ada than demanded or if the transaction fails</param>
        /// <param Name="acceptheigheramounts">Accept higher amounts then demanded - they will be refunded to the customer or the optional refundaddress</param>
        /// <param name="referer">(Optional) A referer code</param>
        /// <param name="customproperty">(Optional) A custom property which can be set. Will be returned at webhooks or checkaddress</param>
        /// <param name="optionalreceiveraddress">(Optional) You can specify a different receiver of the nft</param>
        /// <param name="optionalrefundaddress">(Optional) If there was refund because of minting error or saleconditions you can specify the receiver of the ada/sol </param>
        /// <param name="revervationtimeinminutes">(Optional) The time in minutes the address will be reserved</param>
        /// <param name="acceptheigheramounts">(Optional) The address accepts the correct or a higher amount for minting. If false or null the exact amount must be received</param>
        /// <param name="addresstype">(Optional) The Base Addresses are with Stakekeys, the Enterprise Addresses without - only available in the cardano network - Default Enterprise</param>
        /// <param name="blockchain">(Optional) The blockchain where the address should be created - Default Cardano</param>
        /// <response code="200">Returns the GetPaymentAddressResultClass Class</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="404">The id of the nft is not found or not assigned to this project</response>            
        /// <response code="406">The demanded ada amount is too less. The minimium is 5 ADA - eg 5000000 lovelace</response>
        /// <response code="409">There is a conflict with the selected nft. See errormessage in the resultset</response>
        /// <response code="500">Internal server error - see the errormessage in the resultset</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetPaymentAddressResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpPost]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Address reservation (sale)" }
        )]
        public async Task<IActionResult> Post([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, 
            [FromBody] ReserveMultipleNftsClassV2 reservenfts,
            [FromQuery] string? referer,
            [FromQuery] string? customproperty,
            [FromQuery] string? optionalreceiveraddress,
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
                RemoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Referer = referer,
                Reservenfts = reservenfts,
                CustomerIpAddress = "127.0.0.1",
                CustomProperty = customproperty,
                OptionalReceiverAddress = optionalreceiveraddress,
                OptionalRefundAddress = optionalrefundaddress,
                AcceptHeigherAmounts = acceptheigheramounts,
                Addresstype = addresstype,
                Coin = GlobalFunctions.ConvertToCoin(blockchain),
                CountNft = reservenfts.ReserveNfts.Length,
                ReservationTimeInMinutes = revervationtimeinminutes
            };


            var result = await ReserveSpecificNftByApiClass.RequestSpecificAddress(_redis, raqc);
            return result.StatusCode == 0 ? Ok(result.SuccessResult) : StatusCode(result.StatusCode, result.ApiError);
        }



    }
}
