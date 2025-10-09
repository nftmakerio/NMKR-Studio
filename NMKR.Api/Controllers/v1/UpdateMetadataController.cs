using NMKR.Shared.Classes;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Functions;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using NMKR.Shared.Functions.Metadata;
using NMKR.Api.Controllers.SharedClasses;

namespace NMKR.Api.Controllers
{
    [Route("[controller]")]
    [ApiVersion("1")]
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
        /// <param Name="apikey">The apikey you have created on NMKR Studio</param>
        /// <param Name="nftprojectid">The id of your project</param>
        /// <param Name="nftid">The ID of the NFT you want to change the metadata</param>
        /// <param Name="metdata">The UploadMetadataClass as Body Content</param>
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
        [HttpPost("{apikey}/{nftprojectid:int}/{nftid:int}")]
        [MapToApiVersion("1")]
        public async Task<IActionResult> Post(string apikey, int nftprojectid, int nftid, [FromBody] UploadMetadataClass metadata)
        {
            await using (var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options))
            {
                if (Request.Method.Equals("HEAD"))
                    return null;

                var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
                var result = CheckApiAccess.CheckApiKey(db, apikey, remoteIpAddress?.ToString(),
                    nftprojectid);
                if (result.ResultState != ResultStates.Ok)
                    return Unauthorized(result);

                await GlobalFunctions.UpdateLastActionProjectAsync(db, nftprojectid,_redis);


                var nft = await (from a in db.Nfts
                        .Include(a=>a.Nftproject).AsSplitQuery()
                    where a.Id == nftid && a.NftprojectId == nftprojectid
                    select a).FirstOrDefaultAsync();

                if (nft == null)
                {
                    result.ResultState = ResultStates.Error;
                    result.ErrorCode = 99;
                    result.ErrorMessage = "NFT not found";
                    LogClass.LogMessage(db, "API-CALL:NFT for Metadataupdate not found " + nftprojectid);
                    return StatusCode(404, result);
                }

                if (!string.IsNullOrEmpty(metadata.Metadata) && !nft.Nftproject.Cip68)
                {
                    var chk1 = new CheckMetadataForCip25Fields();
                    var checkmetadata = chk1.CheckMetadata(metadata.Metadata,
                        nft.Nftproject.Policyid, "", true, false);
                    if (!checkmetadata.IsValid)
                    {
                        result.ErrorCode = 115;
                        result.ErrorMessage = checkmetadata.ErrorMessage;
                        result.ResultState = ResultStates.Error;
                        await db.Database.CloseConnectionAsync();
                        return StatusCode(406, result);
                    }
                }

                if (metadata.Metadata == "")
                    metadata.Metadata = null;

                nft.Metadataoverride = metadata.Metadata;
                await db.SaveChangesAsync();

                var nftdetails = await GetNftDetailsClass.GetNftDetailsAsync(db, _redis, nft);

                await db.Database.CloseConnectionAsync();
                return Ok(nftdetails);
            }
        }

    }
}
