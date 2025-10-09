using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Api.Controllers.SharedClasses;
using NMKR.Shared.Classes;
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
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class UpdateMetadataController : ControllerBase
    {

        private readonly IConnectionMultiplexer _redis;

        public UpdateMetadataController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Updates the Metadata for one specific NFT
        /// </summary>
        /// <remarks>
        /// With this API you can update the Metadata Override for one specific NFT
        /// If you leave the field blank, the Metadata override will be deleted and the Metadata from the project will be used.
        /// </remarks>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="projectuid">The uid of your project</param>
        /// <param Name="nftuid">The UID of the NFT you want to change the metadata</param>
        /// <param Name="metadataAsJson">The Metadata (JSON Document) as Body Content</param>
        /// <response code="200">Returns the Nftdetails Class</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>
        /// <response code="404">The NFT was not found</response>     
        /// <response code="406">See the errormessage in the resultset for further information</response>
        /// <response code="500">Internal server error - see the errormessage in the resultset</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(NftDetailsClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpPost("{projectuid}/{nftuid}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "NFT" }
        )]
        public async Task<IActionResult> Post([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string projectuid, string nftuid, [FromBody] JsonDocument metadataAsJson)
        {
          //  string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
           
            if (Request.Method.Equals("HEAD"))
                return null;

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;


            string apifunction = this.GetType().Name;
            string apiparameter = projectuid;

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
                return StatusCode(cachedResult.Statuscode, JsonConvert.DeserializeObject<ApiErrorResultClass>(cachedResult.ResultString));


            // Check if the Apikey is valid
            var result = CheckCachedAccess.CheckApikeyOrToken(_redis, apifunction,
                "", apikey, remoteIpAddress?.ToString());
            if (result.ResultState != ResultStates.Ok)
            {
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                return Unauthorized(result);
            }


            string metadata = "";
            try
            {
                await using var stream = new MemoryStream();
                var writer = new Utf8JsonWriter(stream, new() { Indented = false });
                metadataAsJson.WriteTo(writer);
                await writer.FlushAsync();

                metadata = Encoding.UTF8.GetString(stream.ToArray());
            }
            catch
            {
                result.ErrorCode = 579;
                result.ErrorMessage = "Metdata is not a valid JSON Document";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }


            var project1 = await (from a in db.Nftprojects
                where a.Uid == projectuid
                select a).FirstOrDefaultAsync();

            if (project1 == null)
            {
                LogClass.LogMessage(db, "API-CALL from " + remoteIpAddress + ": ERROR: Project not found " + projectuid);
                result.ErrorCode = 570;
                result.ErrorMessage = "Internal error (1). Please contact support";
                result.ResultState = ResultStates.Error;
                return StatusCode(500, result);
            }

            int nftprojectid = project1.Id;
            await GlobalFunctions.UpdateLastActionProjectAsync(db, nftprojectid,_redis);


            var nft = await (from a in db.Nfts
                    .Include(a=>a.Nftproject)
                    .ThenInclude(a=>a.Customer)

                where a.Uid == nftuid && a.NftprojectId == nftprojectid
                select a).FirstOrDefaultAsync();

            if (nft == null)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 99;
                result.ErrorMessage = "NFT not found";
                LogClass.LogMessage(db, "API-CALL:NFT for Metadataupdate not found " + nftprojectid);
                return StatusCode(404, result);
            }

            if (!string.IsNullOrEmpty(metadata) && !nft.Nftproject.Cip68)
            {
                var chk1 = new CheckMetadataForCip25Fields();
                var checkmetadata = chk1.CheckMetadata(metadata, nft.Nftproject.Policyid, "", true, false);
                if (!checkmetadata.IsValid)
                {
                    result.ErrorCode = 115;
                    result.ErrorMessage = checkmetadata.ErrorMessage;
                    result.ResultState = ResultStates.Error;
                    await db.Database.CloseConnectionAsync();
                    return StatusCode(406, result);
                }
            }

            if (metadata == "")
                metadata = null;

            nft.Metadataoverride = metadata;
            await db.SaveChangesAsync();

            var nftdetails = await GetNftDetailsClass.GetNftDetailsAsync(db, _redis, nft);

            await db.Database.CloseConnectionAsync();
            return Ok(nftdetails);
        }

    }
}
