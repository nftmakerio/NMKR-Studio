using System.Threading.Tasks;
using CardanoSharp.Wallet.Enums;
using NMKR.Api.Controllers.SharedClasses;
using NMKR.Shared.Classes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using Asp.Versioning;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace NMKR.Api.Controllers
{
    /// <summary>
    /// GetAddressForRandomNftSale - Returns an payin address for a random nft sale
    /// </summary>
   // [ApiExplorerSettings(IgnoreApi = true)]
    [Route("[controller]")]
    [ApiVersion("1")]

    public class GetAddressForRandomNftSaleController : Controller
    {
        private readonly IConnectionMultiplexer _redis;

        /// <summary>
        /// Returns an address for a random nft sale 
        /// </summary>
        /// <param name="redis"></param>
        public GetAddressForRandomNftSaleController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Returns an address for a random nft sale (project id)
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
        /// <param Name="nftprojectid">The id of your project</param>
        /// <param Name="countnft">The count of the nft/tokens you want to mint and send.</param>
        /// <param Name="lovelace">The adaamount the buyer has to send in lovelace - so 1 ADA is 1000000 lovelace</param>
        /// <param name="referer">(Optional) A referer code</param>
        /// <param name="customproperty">(Optional) A custom property which can be set. Will be returned at webhooks or checkaddress</param>
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
        [HttpGet("{apikey}/{nftprojectid:int}/{countnft:int}/{lovelace:long}")]
        [MapToApiVersion("1")]
        public async Task<IActionResult> Get(string apikey, int nftprojectid, int countnft, long lovelace, [FromQuery] string? referer, [FromQuery] string? customproperty, [FromQuery] string? optionalreceiveraddress)
        {

            ReserveAddressQueueClass raqc = new()
            {
                ApiKey = apikey, NftprojectId = nftprojectid, CountNft = countnft, CardanoLovelace = lovelace, Price= lovelace,
                RemoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Referer = referer,
                CustomProperty = customproperty,
                OptionalReceiverAddress = optionalreceiveraddress,
                Addresstype = AddressType.Enterprise
            };

            
            var result=await ReserveRandomNftByApiClass.RequestRandomAddress(_redis, raqc);
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
        /// <param Name="nftprojectid">The id of your project</param>
        /// <param Name="countnft">The count of the nft/tokens you want to mint and send.</param>
        /// <param Name="lovelace">The adaamount the buyer has to send in lovelace - so 1 ADA is 1000000 lovelace</param>
        /// <param name="referer">(Optional) A referer code</param>
        /// <param name="customproperty">(Optional) A custom property which can be set. Will be returned at webhooks or checkaddress</param>
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
        [HttpGet("{apikey}/{nftprojectid:int}/{countnft:int}")]
        [MapToApiVersion("1")]
        public async Task<IActionResult> Get(string apikey, int nftprojectid, int countnft, [FromQuery] string? referer, [FromQuery] string? customproperty, [FromQuery] string? optionalreceiveraddress)
        {
            ReserveAddressQueueClass raqc = new()
            {
                ApiKey = apikey,
                NftprojectId = nftprojectid,
                CountNft = countnft,
                CardanoLovelace = null,
                Price = null,
                RemoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Referer=referer,
                CustomProperty = customproperty,
                OptionalReceiverAddress = optionalreceiveraddress,
                Addresstype = AddressType.Enterprise
            };


            var result = await ReserveRandomNftByApiClass.RequestRandomAddress(_redis, raqc);
            return result.StatusCode == 0 ? Ok(result.SuccessResult) : StatusCode(result.StatusCode, result.ApiError);
        }


        /// <summary>
        /// Returns an address for a random nft sale (price from pricelist) (project uid)
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
        /// <param Name="projectuid">The uid of your project (not the id)</param>
        /// <param Name="countnft">The count of the nft/tokens you want to mint and send.</param>
        /// <param Name="lovelace">The adaamount the buyer has to send in lovelace - so 1 ADA is 1000000 lovelace</param>
        /// <param name="referer">(Optional) A referer code</param>
        /// <param name="customproperty">(Optional) A custom property which can be set. Will be returned at webhooks or checkaddress</param>
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
        [HttpGet("{apikey}/{projectuid}/{countnft:long}")]
        [MapToApiVersion("1")]
        public async Task<IActionResult> Get(string apikey, string projectuid, int countnft, [FromQuery] string? referer, [FromQuery] string? customproperty, [FromQuery] string? optionalreceiveraddress)
        {
            ReserveAddressQueueClass raqc = new()
            {
                ApiKey = apikey,
                NftprojectUId = projectuid,
                CountNft = countnft,
                CardanoLovelace = null,
                Price = null,
                RemoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Referer = referer,
                CustomProperty = customproperty,
                OptionalReceiverAddress = optionalreceiveraddress,
                Addresstype = AddressType.Enterprise
            };


            var result = await ReserveRandomNftByApiClass.RequestRandomAddress(_redis, raqc);
            return result.StatusCode == 0 ? Ok(result.SuccessResult) : StatusCode(result.StatusCode, result.ApiError);

        }


        /// <summary>
        /// Returns an address for a random nft sale (project id)
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
        /// <param Name="nftprojectid">The id of your project</param>
        /// <param Name="countnft">The count of the nft/tokens you want to mint and send.</param>
        /// <param Name="lovelace">The adaamount the buyer has to send in lovelace - so 1 ADA is 1000000 lovelace</param>
        /// <param name="referer">(Optional) A referer code</param>
        /// <param name="customproperty">(Optional) A custom property which can be set. Will be returned at webhooks or checkaddress</param>
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
        [HttpGet("{apikey}/{projectuid}/{countnft:long}/{lovelace:long}")]
        [MapToApiVersion("1")]
        public async Task<IActionResult> Get(string apikey, string projectuid, int countnft, long lovelace, [FromQuery] string? referer, [FromQuery] string? customproperty, [FromQuery] string? optionalreceiveraddress)
        {
            ReserveAddressQueueClass raqc = new()
            {
                ApiKey = apikey,
                NftprojectUId = projectuid,
                CountNft = countnft,
                CardanoLovelace = lovelace,
                Price = lovelace,
                RemoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Referer = referer,
                CustomProperty = customproperty,
                OptionalReceiverAddress = optionalreceiveraddress,
                Addresstype = AddressType.Enterprise
            };

            var result = await ReserveRandomNftByApiClass.RequestRandomAddress(_redis, raqc);
            return result.StatusCode == 0 ? Ok(result.SuccessResult) : StatusCode(result.StatusCode, result.ApiError);
        }


    }
}
