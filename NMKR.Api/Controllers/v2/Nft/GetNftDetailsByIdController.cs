using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Api.Controllers.SharedClasses;
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
    public class GetNftDetailsByIdController : ControllerBase
    {

        private readonly IConnectionMultiplexer _redis;

        public GetNftDetailsByIdController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Returns detail information about one nft specified by Id (nft uid)
        /// </summary>
        /// <remarks>
        /// You will receive all information (fingerprint, ipfshash, etc.) about one nfts with the submitted id
        /// </remarks>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="nftuid">The uid of the nft</param>
        /// <param Name="nftid">The ID of the nft you want to receive the details</param>
        /// <response code="200">Returns the Nft Class</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="404">The nft was not found</response>            
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(NftDetailsClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
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

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = nftuid;

            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok( JsonConvert.DeserializeObject<NftDetailsClass>(cachedResult.ResultString));
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


            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            var nftx =await (from a in db.Nfts
                    .Include(a=>a.Nftproject)
                    .ThenInclude(a=>a.Settings)
                    .AsSplitQuery()
                where
                    a.Uid == nftuid && a.MainnftId == null
                select a).AsNoTracking().FirstOrDefaultAsync();


            if (nftx == null)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorMessage = "Nft not found";
                result.ErrorCode = 404;
                await db.Database.CloseConnectionAsync();
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 404, result, apiparameter);
                return NotFound(result);
            }
            var nftdetails = await GetNftDetailsClass.GetNftDetailsAsync(db, _redis, nftx);
            await db.Database.CloseConnectionAsync();
            CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200, nftdetails, apiparameter);
            return Ok(nftdetails);
        }
    }
}
