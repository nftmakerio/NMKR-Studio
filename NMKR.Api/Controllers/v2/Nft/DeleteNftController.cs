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
    public class DeleteNftController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public DeleteNftController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Deletes a nft from the database (nft uid)
        /// </summary>
        /// <remarks>
        /// You can delete a nft, if it is not in sold or reserved state
        /// </remarks>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="nftprojectid">The id of your project</param>
        /// <param Name="nftid">The ID of the nft you want to delete</param>
        /// <response code="200">Returns the Nft Class</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="404">The nft was not found</response>            
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{nftuid}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "NFT" }
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string nftuid)
        {
          //  string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            if (Request.Method.Equals("HEAD"))
                return null;

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;

            string apifunction = this.GetType().Name;
            string apiparameter = nftuid;

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
                nftx.State = "deleted";
                await db.SaveChangesAsync();
                await db.Database.CloseConnectionAsync();
                return Ok();
            }


            if (nftx.State != "free")
            {
                result.ResultState = ResultStates.Error;
                result.ErrorMessage = "Nft is not free";
                result.ErrorCode = 119;
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 406, result, apiparameter);
                await db.Database.CloseConnectionAsync();
                return StatusCode(406, result);
            }

            db.Nfts.Remove(nftx); // Remove from Database
            await db.SaveChangesAsync();

            await db.Database.CloseConnectionAsync();
            return Ok();
        }

    }
}

