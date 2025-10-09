using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.Metadata;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;

namespace NMKR.Api.Controllers.v2.Nft
{

    // Apikey Testnet: 

    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class CheckMetadataController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public CheckMetadataController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Checks if the metadata is valid
        /// </summary>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="projectuid">The uid of your project</param>
        /// <response code="200">Returns OK if the metadata are valid</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>
        /// <response code="404">The NFT was not found</response>     
        /// <response code="406">See the errormessage in the resultset for further information</response>
        /// <response code="500">Internal server error - see the errormessage in the resultset</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{nftuid}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] {"NFT"}
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore] [FromHeader(Name = "authorization")] string apikey,
            string nftuid)
        {
            return await Post(apikey, nftuid, new UploadMetadataClass() {Metadata = null});
        }

        /// <summary>
            /// Checks if the metadata is valid
            /// </summary>
            /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
            /// <param Name="projectuid">The uid of your project</param>
            /// <param Name="metdata">The Metadata as Override in the Body Content</param>
            /// <response code="200">Returns OK if the metadata are valid</response>
            /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>
            /// <response code="404">The NFT was not found</response>     
            /// <response code="406">See the errormessage in the resultset for further information</response>
            /// <response code="500">Internal server error - see the errormessage in the resultset</response>
            [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpPost("{nftuid}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "NFT" }
        )]
        public async Task<IActionResult> Post([OpenApiHeaderIgnore] [FromHeader(Name = "authorization")] string apikey, string nftuid, [FromBody] UploadMetadataClass metadata)
        {
           // string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            if (Request.Method.Equals("HEAD"))
                return null;

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = nftuid + metadata.Metadata;

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok(JsonConvert.DeserializeObject<CheckAddressResultClass>(cachedResult.ResultString));
                return StatusCode(cachedResult.Statuscode, JsonConvert.DeserializeObject<ApiErrorResultClass>(cachedResult.ResultString));
            }

            // Check if the Apikey is valid
            var result = CheckCachedAccess.CheckApikeyOrToken(_redis, apifunction,
               "", apikey, remoteIpAddress?.ToString());
            if (result.ResultState != ResultStates.Ok)
            {
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                return Unauthorized(result);
            }

            var nft = await (from a in db.Nfts
                    .Include(a => a.Nftproject)
                    .ThenInclude(a => a.Customer)
                where a.Uid == nftuid 
                select a).FirstOrDefaultAsync();

            if (nft == null)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 99;
                result.ErrorMessage = "NFT not found";
                LogClass.LogMessage(db, "API-CALL:NFT for Checkmetadata not found " + nftuid);
                return StatusCode(404, result);
            }

            if (!string.IsNullOrEmpty(metadata.Metadata) && !nft.Nftproject.Cip68)
            {
                var chk1 = new CheckMetadataForCip25Fields();
                var checkmetadata = chk1.CheckMetadata(metadata.Metadata, nft.Nftproject.Policyid, "", true, false);
                if (!checkmetadata.IsValid)
                {
                    result.ErrorCode = 115;
                    result.ErrorMessage = checkmetadata.ErrorMessage;
                    result.ResultState = ResultStates.Error;
                    await db.Database.CloseConnectionAsync();
                    return StatusCode(406, result);
                }

            }

         /*   if (string.IsNullOrEmpty(metadata.Metadata))
            {
                result.ErrorCode = 701;
                result.ErrorMessage = "Metadata is empty";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }*/
            var paywallet = await (from a in db.Adminmintandsendaddresses
                where a.Addressblocked == false && a.Lovelace > 1500000 && a.Coin == Coin.ADA.ToString()
                                   orderby a.Lovelace descending
                select a).FirstOrDefaultAsync();

            if (paywallet == null)
            {
                result.ErrorCode = 731;
                result.ErrorMessage = "All Testwallets are currently in use. Please try again later";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            // we take a senderaddress from NFT-Maker - not the customer. This is because if we dont know if the customer has ada in his account. If not, the tx-in is missing
            var senderaddress = paywallet.Address; // first admin mit and send address

            // Just 2 Test addresses - we will 
            string recevieraddress = "addr1q9xr9tdw7e6zpj0sltqf4wj6ae7avr5wcgy3wt67ffp575l8xh6xlg9a5vuem3r4eg3qljvjpt0g9r0p9w7fgg5k6gss92cy8r";
            if (!GlobalFunctions.IsMainnet())
                recevieraddress = "addr_test1qqrmtwl66uvj6gd4hfw6h964q7y0xcjdsnjnwjwctxfru7dycwrylu0nqy3f7ue8akrzc96zlk3x3t86ec6fckn3ualq9ql6yu";

            BuildTransactionClass buildtransaction = new();
            var s = nft.Nftproject.Cip68
                ? ConsoleCommand.CheckMetadataCip68(db, _redis, nft, senderaddress, recevieraddress, metadata.Metadata, "", GlobalFunctions.IsMainnet(), out  buildtransaction)
                : ConsoleCommand.CheckMetadata(db, _redis, nft, senderaddress, recevieraddress, metadata.Metadata,  "", GlobalFunctions.IsMainnet(), out  buildtransaction);
            if (!s.Contains("OK"))
            {
                result.ErrorCode = 701;
                result.ErrorMessage = "Metadata are not accepted by the network - see InnerErrorMessage for details";
                result.InnerErrorMessage = buildtransaction.ErrorMessage.Replace(GeneralConfigurationClass.TempFilePath, "");
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            result.ResultState = ResultStates.Ok;
            return Ok(result);
        }
    }
}

