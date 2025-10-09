using System.Collections.Generic;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Api.Controllers.SharedClasses;
using NMKR.Shared.Classes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace NMKR.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ApiVersion("1")]

    public class GetAddressForSpecificNftSaleController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public GetAddressForSpecificNftSaleController(IConnectionMultiplexer redis)
        {
            _redis = redis;
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
        /// <param Name="apikey">The apikey you have created on NMKR Studio</param>
        /// <param Name="nftprojectid">The id of your project</param>
        /// <param Name="nftid">The id of the nft you want to mint and send</param>
        /// <param Name="tokencount">The amount of tokens you want to send (only if multiple tokens available)</param>
        /// <param Name="lovelace">The adaamount the buyer has to send in lovelace - so 1 ADA is 1000000 lovelace</param>
        /// <param name="referer">(Optional) A referer code</param>
        /// <param name="customproperty">(Optional) A custom property which can be set. Will be returned at webhooks or checkaddress</param>
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
        [HttpGet("{apikey}/{nftprojectid:int}/{nftid:int}/{tokencount:long}/{lovelace:long}")]
        [MapToApiVersion("1")]
        public async Task<IActionResult> Get(string apikey, int nftprojectid, int nftid, long tokencount, long lovelace, [FromQuery] string? referer, [FromQuery] string? customproperty, [FromQuery] string? optionalreceiveraddress)
        {
            return await Get(apikey, nftprojectid, nftid, tokencount, lovelace, null, null, null, referer, customproperty, optionalreceiveraddress);
        }


        private async Task<IActionResult> Get(string apikey, int nftprojectid, int nftid, long tokencount, long lovelace, long? priceintoken, string tokenpolicyid, string tokenassetid, string referer, string customproperty, string optionalreceiveraddress)
        {
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");


            ReserveMultipleNftsClassV2 reserve = new()
            {
                ReserveNfts = new[] {new ReserveNftsClassV2() {NftId = nftid, Tokencount = tokencount}}
            };

            ReserveAddressQueueClass raqc = new()
            {
                ApiKey = apikey,
                NftprojectId = nftprojectid,
                CardanoLovelace = lovelace,
                Price= lovelace,
                RemoteIpAddress =  HttpContext.Connection.RemoteIpAddress?.ToString(),
                Referer = referer,
                Reservenfts = reserve,
                CustomProperty = customproperty,
                OptionalReceiverAddress = optionalreceiveraddress,
            };


            var result = await ReserveSpecificNftByApiClass.RequestSpecificAddress(_redis, raqc);
            return result.StatusCode == 0 ? Ok(result.SuccessResult) : StatusCode(result.StatusCode, result.ApiError);

        }

        /// <summary>
        /// Returns an address for a specific nft sale (no random distribution)
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
        /// <param Name="lovelace">The adaamount the buyer has to send in lovelace - so 1 ADA is 1000000 lovelace</param>
        /// <param name="referer">(Optional) A referer code</param>
        /// <param name="customproperty">(Optional) A custom property which can be set. Will be returned at webhooks or checkaddress</param>
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
        [HttpGet("{apikey}/{nftuid}/{tokencount:long}/{lovelace:long}")]
        [MapToApiVersion("1")]
        public async Task<IActionResult> Get(string apikey, string nftuid, long tokencount, long lovelace, [FromQuery] string? referer, [FromQuery] string? customproperty, [FromQuery] string? optionalreceiveraddress)
        {
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");


            ReserveMultipleNftsClassV2 reserve = new()
            {
                ReserveNfts = new[] { new ReserveNftsClassV2() { NftUid  = nftuid, Tokencount = tokencount } }
            };

            ReserveAddressQueueClass raqc = new()
            {
                ApiKey = apikey,
                CardanoLovelace = lovelace,
                Price = lovelace,
                RemoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Referer = referer,
                Reservenfts = reserve,
                CustomProperty = customproperty,
                OptionalReceiverAddress = optionalreceiveraddress,
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
        /// <param Name="nftprojectid">The id of your project</param>
        /// <param Name="nftid">The id of the nft you want to mint and send</param>
        /// <param Name="tokencount">The amount of tokens you want to send (only if multiple tokens available)</param>
        /// <param Name="lovelace">The adaamount the buyer has to send in lovelace - so 1 ADA is 1000000 lovelace</param>
        /// <param name="referer">(Optional) A referer code</param>
        /// <param name="customproperty">(Optional) A custom property which can be set. Will be returned at webhooks or checkaddress</param>
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
        [HttpGet("{apikey}/{nftprojectid:int}/{nftid:int}/{tokencount:long}")]
        [MapToApiVersion("1")]
        public async Task<IActionResult> Get(string apikey, int nftprojectid, int nftid, long tokencount, [FromQuery] string? referer, [FromQuery] string? customproperty, [FromQuery] string? optionalreceiveraddress)
        {
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");


            ReserveMultipleNftsClassV2 reserve = new()
            {
                ReserveNfts = new[] { new ReserveNftsClassV2() { NftId = nftid, Tokencount = tokencount } }
            };

            ReserveAddressQueueClass raqc = new()
            {
                ApiKey = apikey,
                NftprojectId = nftprojectid,
                RemoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Referer = referer,
                Reservenfts = reserve,
                CustomProperty = customproperty,
                OptionalReceiverAddress = optionalreceiveraddress,
            };


            var result = await ReserveSpecificNftByApiClass.RequestSpecificAddress(_redis, raqc);
            return result.StatusCode == 0 ? Ok(result.SuccessResult) : StatusCode(result.StatusCode, result.ApiError);
        }




        /// <summary>
        /// Returns an address for a specific nft sale (no random distribution) - price from pricelist or specific nft price (nft uid)
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
        /// <param Name="nftid">The id of the nft you want to mint and send</param>
        /// <param Name="tokencount">The amount of tokens you want to send (only if multiple tokens available)</param>
        /// <param Name="lovelace">The adaamount the buyer has to send in lovelace - so 1 ADA is 1000000 lovelace</param>
        /// <param name="referer">(Optional) A referer code</param>
        /// <param name="customproperty">(Optional) A custom property which can be set. Will be returned at webhooks or checkaddress</param>
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
        [HttpGet("{apikey}/{nftuid}/{tokencount:long}")]
        [MapToApiVersion("1")]
        public async Task<IActionResult> Get(string apikey, string nftuid, long tokencount, [FromQuery] string? referer, [FromQuery] string? customproperty, [FromQuery] string? optionalreceiveraddress)
        {
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
                CustomProperty = customproperty,
                OptionalReceiverAddress = optionalreceiveraddress,
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
        /// <param Name="nftprojectid">The id of your project</param>
        /// <param name="referer">(Optional) A referer code</param>
        /// <param name="customproperty">(Optional) A custom property which can be set. Will be returned at webhooks or checkaddress</param>
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
        [HttpPost("{apikey}/{nftprojectid:int}")]
        [MapToApiVersion("1")]

        public async Task<IActionResult> Post(string apikey, int nftprojectid, [FromBody] ReserveMultipleNftsClass reservenfts, [FromQuery] string? referer, [FromQuery] string? customproperty, [FromQuery] string? optionalreceiveraddress)
        {
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            ReserveMultipleNftsClassV2 reserve = new();
            List<ReserveNftsClassV2> r2 = new();
            if (reservenfts != null && reservenfts.ReserveNfts != null)
            {
                foreach (var reservenftsReserveNft in reservenfts.ReserveNfts)
                {
                 r2.Add(new(){NftId = reservenftsReserveNft.NftId, Tokencount = reservenftsReserveNft.Tokencount});   
                }
            }

            reserve.ReserveNfts = r2.ToArray();


            ReserveAddressQueueClass raqc = new()
            {
                ApiKey = apikey,
                CardanoLovelace = reservenfts.Lovelace==0?null:reservenfts.Lovelace,
                Price = reservenfts.Lovelace == 0 ? null : reservenfts.Lovelace,
                NftprojectId = nftprojectid,
                RemoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Referer = referer,
                Reservenfts = reserve,
                CustomProperty = customproperty,
                OptionalReceiverAddress = optionalreceiveraddress,
            };


            var result = await ReserveSpecificNftByApiClass.RequestSpecificAddress(_redis, raqc);
            return result.StatusCode == 0 ? Ok(result.SuccessResult) : StatusCode(result.StatusCode, result.ApiError);
        }
    }
}
