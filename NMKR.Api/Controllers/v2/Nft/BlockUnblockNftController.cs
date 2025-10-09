using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
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
    public class BlockUnblockNftController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public BlockUnblockNftController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Blocks/Unblocks an nft  (nft uid)
        /// </summary>
        /// <remarks>
        /// You can block an nft, if it is not already sold or reserved and you can unblock blocked nfts
        /// </remarks>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="nftprojectid">The id of your project</param>
        /// <param Name="nftid">The ID of the nft you want to delete</param>
        /// <param Name="blockNft">Indicates, if you want to block the NFT (true) or to unblock the NFT (false)</param>
        /// <response code="200">Returns the Nft Class</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="404">The nft was not found</response>            
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{nftuid}/{blockNft}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "NFT" }
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string nftuid, bool blockNft)
        {
           // string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            if (Request.Method.Equals("HEAD"))
                return null;

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;

            string apifunction = this.GetType().Name;
            string apiparameter = nftuid + blockNft;

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


            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            var nftx = await (from a in db.Nfts
                where a.Uid == nftuid && a.MainnftId == null
                select a).FirstOrDefaultAsync();


            if (nftx == null)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorMessage = "Nft not found";
                result.ErrorCode = 404;
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 404, result, apiparameter);
                await db.Database.CloseConnectionAsync();
                return NotFound(result);
            }

            if (nftx.Soldcount != 0)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorMessage = "Nft is already sold";
                result.ErrorCode = 117;
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 406, result, apiparameter);
                await db.Database.CloseConnectionAsync();
                return StatusCode(406, result);
            }

            if (nftx.Reservedcount != 0)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorMessage = "Nft is in reserved state";
                result.ErrorCode = 118;
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 406, result, apiparameter);
                await db.Database.CloseConnectionAsync();
                return StatusCode(406, result);
            }

            if (nftx.State == "burned")
            {
                result.ResultState = ResultStates.Error;
                result.ErrorMessage = "Nft is in burned state";
                result.ErrorCode = 121;
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 406, result, apiparameter);
                await db.Database.CloseConnectionAsync();
                return StatusCode(406, result);
            }


            if (nftx.State != "free" && blockNft)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorMessage = "Nft is not free - Block not possible";
                result.ErrorCode = 119;
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 406, result, apiparameter);
                await db.Database.CloseConnectionAsync();
                return StatusCode(406, result);
            }
            if (nftx.State != "blocked" && !blockNft)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorMessage = "Nft is not blocked - unblock not possible";
                result.ErrorCode = 122;
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 406, result, apiparameter);
                await db.Database.CloseConnectionAsync();
                return StatusCode(406, result);
            }

            nftx.State = blockNft ? "blocked" : "free";

            await db.SaveChangesAsync();

            await db.Database.CloseConnectionAsync();
            return Ok();
        }

    }
}

