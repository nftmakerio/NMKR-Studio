using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;

namespace NMKR.Api.Controllers.v2.Projects
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class GetAllNftsFromProjectController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public GetAllNftsFromProjectController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Returns detail information about all nfts with a specific state. 
        /// </summary>
        /// <remarks>
        /// You will receive all information (fingerprint, ipfshash, etc.) about the nfts within a specific state.
        /// State "all" lists all available nft in this project. The other states are: "free", "reserved", "sold" and "error"
        /// </remarks>
        /// <param Name="apikey">The apikey you have created on NMKR Studio</param>
        /// <param Name="nftprojectid">The id of your project</param>
        /// <param Name="state">The state of the nfts you want to receive</param>
        /// <response code="200">Returns a List of the NFT Class</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="406">The state was not known - possible states are: free, reserved, sold, error and all</response>            
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<NFT>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{apikey}/{projectuid}/{state}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "NFT" }
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string projectuid, string state)
        {
          //  string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");
            if (Request.Method.Equals("HEAD"))
                return null;
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = projectuid + state;

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok(JsonConvert.DeserializeObject<List<NFT>>(cachedResult.ResultString));
                return StatusCode(cachedResult.Statuscode,
                    JsonConvert.DeserializeObject<ApiErrorResultClass>(cachedResult.ResultString));
            }


            // Check if the Apikey is valid
            var result = CheckCachedAccess.CheckApikeyOrToken(_redis, apifunction,
               "", apikey, remoteIpAddress?.ToString());
            if (result.ResultState != ResultStates.Ok)
            {
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                return Unauthorized(result);
            }


            if (state != "free" && state != "reserved" && state != "all" && state != "sold" && state != "error" &&
                state != "burned")
            {
                result.ErrorCode = 20;
                result.ErrorMessage = "State not known";
                result.ResultState = ResultStates.Error;
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 406, result, apiparameter);
                return StatusCode(406, result);
            }

            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            List<NMKR.Shared.Model.Nft> nft;
            if (state == "sold")
            {
                nft = await (from a in db.Nfts
                        .Include(a => a.Nftproject)
                    where a.Nftproject.Uid == projectuid && (a.State == state ) &&
                          a.MainnftId == null && a.Isroyaltytoken == false
                    orderby a.Selldate descending
                    select a).AsNoTracking().ToListAsync();
            }
            else
            {
                nft = await (from a in db.Nfts
                        .Include(a => a.Nftproject)
                    where a.Nftproject.Uid == projectuid && (a.State == state || state == "all") &&
                          a.MainnftId == null && a.Isroyaltytoken == false
                    select a).AsNoTracking().ToListAsync();
            }


            List<NFT> nftl = new();
            foreach (var a in nft)
            {
                nftl.Add(new()
                {
                    IpfsLink = "ipfs://" + a.Ipfshash,
                    Name = a.Name,
                    Displayname = a.Displayname,
                    Detaildata = a.Detaildata,
                    GatewayLink = GeneralConfigurationClass.IPFSGateway + a.Ipfshash,
                    State = a.State,
                    Minted = a.Minted,
                    Fingerprint = a.Fingerprint,
                    AssetId = a.Assetid,
                    Assetname = a.Assetname,
                    InitialMintTxHash = a.Initialminttxhash,
                    PolicyId = a.Policyid,
                    Series = a.Series,
                    Id = a.Id,
                    Uid = a.Uid,
                    Price = GlobalFunctions.GetPrice(db,_redis, a,1),
                    PriceSolana=GlobalFunctions.GetPrice(db,_redis,a,1,Coin.SOL),
                });
            }

            await db.Database.CloseConnectionAsync();
            CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200, nftl, apiparameter);
            return Ok(nftl);
        }
    }
}
