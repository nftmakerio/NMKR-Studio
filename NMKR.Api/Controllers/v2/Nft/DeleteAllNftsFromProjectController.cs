using System.Linq;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.EntityFrameworkCore;

namespace NMKR.Api.Controllers.v2.Nft
{
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class DeleteAllNftsFromProjectController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public DeleteAllNftsFromProjectController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Deletes all nfts from the database 
        /// </summary>
        /// <remarks>
        /// This function deletes all NFTs from a project. You can delete a nft, if it is not in sold or reserved state. All other nfts will be deleted.
        /// </remarks>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="nftprojectid">The id of your project</param>
        /// <param Name="nftid">The ID of the nft you want to delete</param>
        /// <response code="200">Returns the Nft Class</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="404">The nft was not found</response>            
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DeleteAllNftsResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{projectuid}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] {"NFT"}
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string projectuid)
        {
           // string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            if (Request.Method.Equals("HEAD"))
                return null;

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;

            string apifunction = this.GetType().Name;
            string apiparameter = projectuid;

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

            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);

            var project = await (from a in db.Nftprojects
                where a.Uid == projectuid
                select a).FirstOrDefaultAsync();

            if (project == null)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorMessage = "Project not found";
                result.ErrorCode = 404;
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 404, result, apiparameter);
                await db.Database.CloseConnectionAsync();
                return NotFound(result);
            }

            var nfts = await (from a in db.Nfts
                where a.NftprojectId == project.Id && a.MainnftId == null
                select a).ToListAsync();

            DeleteAllNftsResultClass res = new DeleteAllNftsResultClass() {SuccessfullyDeleted = 0};

            foreach (var nftx in nfts)
            {
                if (nftx.Soldcount != 0)
                {
                    res.ErrorDetails.Add(new DeleteAllNftsDetail()
                        {ErrorMessage = "NFT already sold", NftName = nftx.Name, NftUid = nftx.Uid});
                    continue;
                }

                if (nftx.Reservedcount != 0)
                {
                    res.ErrorDetails.Add(new DeleteAllNftsDetail()
                        {ErrorMessage = "NFT is in reserved state", NftName = nftx.Name, NftUid = nftx.Uid});
                    continue;
                }

                if (nftx.State == "burned")
                {
                    res.ErrorDetails.Add(new DeleteAllNftsDetail()
                        {ErrorMessage = "NFT already burned", NftName = nftx.Name, NftUid = nftx.Uid});
                    continue;
                }

                if (nftx.State != "free")
                {
                    res.ErrorDetails.Add(new DeleteAllNftsDetail()
                        {ErrorMessage = "NFT is not in 'free' state", NftName = nftx.Name, NftUid = nftx.Uid});
                    continue;
                }

                db.Nfts.Remove(nftx); // Remove from Database
                res.SuccessfullyDeleted++;
            }
            await db.SaveChangesAsync();

            return Ok(res);
        }
    }
}
