using System.Linq;
using NMKR.Shared.Classes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace NMKR.Api.Controllers.v2.Nft
{
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class DuplicateNftController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public DuplicateNftController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Duplicates a nft/token inside a project. If a token already exists, it will be skipped
        /// </summary>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="nftuid">The UID of the nft/token you want to duplicate</param>
        /// <response code="200">Duplicate was successful. Returns the NtProjectDetails Class</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="404">The nft was not found</response>
        /// <response code="406">The nft is not valid</response>            
        [ProducesResponseType(StatusCodes.Status200OK, Type=typeof(NftProjectsDetails))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [HttpPost("{nftuid}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] {"NFT"}
        )]
        public async Task<IActionResult> Post([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string nftuid, [FromBody] DuplicateNftClass duplicateclass)
        {
            // string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
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

            var nft = await (from a in db.Nfts
            .Include(a=>a.Nftproject)
                             where a.Uid == nftuid 
                             select a).AsNoTracking().FirstOrDefaultAsync();


            if (nft == null)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 4001;
                result.ErrorMessage = "Nft UID is not valid";
                LogClass.LogMessage(db, $"API-CALL: Nft UID is not valid {nftuid}");
                return StatusCode(404, result);
            }

            if (nft.Isroyaltytoken)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 4002;
                result.ErrorMessage = "Nft is a royalty token";
                LogClass.LogMessage(db, $"API-CALL: Nft is a royalty token {nftuid}");
                return StatusCode(406, result);
            }

            if (nft.Nftproject.State == "deleted")
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 4003;
                result.ErrorMessage = "Project is deleted";
                LogClass.LogMessage(db, $"API-CALL: Project is deleted {nftuid}");
                return StatusCode(406, result);
            }

            if (nft.Nftproject.State == "finished")
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 4004;
                result.ErrorMessage = "Project is already finished";
                LogClass.LogMessage(db, $"API-CALL: Project is already finished {nftuid}");
                return StatusCode(406, result);
            }

            if (duplicateclass.CountDuplicates <= 0)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 4005;
                result.ErrorMessage = "Count is too small";
                LogClass.LogMessage(db, $"API-CALL: Count is too small");
                return StatusCode(406, result);
            }
            if (duplicateclass.CountDuplicates > 9999)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 4005;
                result.ErrorMessage = "Count is too large";
                LogClass.LogMessage(db, $"API-CALL: Count is too large");
                return StatusCode(406, result);
            }


            await duplicateclass.StartDuplicating(db, nftuid);

            var proj = await (from a in db.Nftprojects
                    .Include(a => a.Customerwallet)
                    .AsSplitQuery()
                    .Include(a => a.Usdcwallet)
                    .AsSplitQuery()
                where a.Id == nft.NftprojectId 
                select a).AsNoTracking().FirstOrDefaultAsync();


            if (proj == null)
            {
                result.ResultState = ResultStates.Error;
                result.ErrorMessage = "Project not found";
                result.ErrorCode = 404;
                await db.Database.CloseConnectionAsync();
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 404, result, apiparameter);
                return NotFound(result);
            }

            var proj1 = new NftProjectsDetails(db, proj);

            return Ok(proj1);
        }
    }
}
