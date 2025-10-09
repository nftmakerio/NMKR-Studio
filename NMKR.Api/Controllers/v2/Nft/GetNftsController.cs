using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

namespace NMKR.Api.Controllers.v2.Nft
{
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class GetNftsController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public GetNftsController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Returns detail information about nfts with a specific state with Pagination support. (project uid)
        /// </summary>
        /// <remarks>
        /// You will receive all information (fingerprint, ipfshash, etc.) about the nfts within a specific state.
        /// State "all" lists all available nft in this project. The other states are: "free", "reserved", "sold" and "error"
        /// </remarks>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="projectuid">The uid of your project (not the id)</param>
        /// <param Name="state">The state of the nfts you want to receive</param>
        /// <param Name="count">How may NFTs do you want. Max 50. Min 1</param>
        /// <param Name="page">The page number. Starts with 1</param>
        /// <param name="orderby">(Optional) The sort order of the result. Possible values are: id (default),id_desc (descending order), selldate (on sold nfts) and selldate_desc (descending order)</param>
        /// <response code="200">Returns a List of the NFT Class</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="406">The state was not known - possible states are: free, reserved, sold, error and all</response>
        /// <response code="406">The projectuid was not found</response>            
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<NFT>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{projectuid}/{state}/{count:int}/{page:int}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "NFT" }
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string projectuid, string state, [Range(1, 100)] int count, [Range(1, int.MaxValue)] int page, [FromQuery] string? orderby = "id")
        {
          //  string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            if (Request.Method.Equals("HEAD"))
                return null;
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = projectuid + state + count + "-" + page+"_"+orderby;

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
            if (page == 0)
            {
                result.ErrorCode = 20;
                result.ErrorMessage = "Pagenumber must start with 1";
                result.ResultState = ResultStates.Error;
                await db.Database.CloseConnectionAsync();
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 406, result, apiparameter);
                return StatusCode(406, result);
            }

            if (count == 0)
            {
                result.ErrorCode = 20;
                result.ErrorMessage = "Count must be at least 1";
                result.ResultState = ResultStates.Error;
                await db.Database.CloseConnectionAsync();
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 406, result, apiparameter);
                return StatusCode(406, result);
            }

            if (count > 50)
            {
                result.ErrorCode = 20;
                result.ErrorMessage = "Count can not exceed 50";
                result.ResultState = ResultStates.Error;
                await db.Database.CloseConnectionAsync();
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 406, result, apiparameter);
                return StatusCode(406, result);
            }

            var project = await (from a in db.Nftprojects
                where a.Uid == projectuid
                select a).FirstOrDefaultAsync();

            if (project == null)
            {
                result.ErrorCode = 50;
                result.ErrorMessage = "Projectuid not known";
                result.ResultState = ResultStates.Error;
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 404, result, apiparameter);
                return StatusCode(406, result);
            }

            if (orderby != "id" && orderby!="id_desc" && orderby != "selldate" && orderby!= "selldate_desc")
            {
                result.ErrorCode = 22;
                result.ErrorMessage = "Orderby is not valid";
                result.ResultState = ResultStates.Error;
                await db.Database.CloseConnectionAsync();
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 406, result, apiparameter);
                return StatusCode(406, result);
            }

            List<NMKR.Shared.Model.Nft> nft=new();
            if (orderby == "selldate")
            {
                nft = await (from a in db.Nfts
                        .Include(a => a.Nftproject)
                        .AsSplitQuery()
                    where a.NftprojectId == project.Id && (a.State == state || state == "all") &&
                          a.MainnftId == null && a.Isroyaltytoken == false
                    orderby a.Selldate 
                    select a).AsNoTracking().Skip((page - 1) * count).Take(count).ToListAsync();
            }
            if (orderby == "selldate_desc")
            {
                nft = await (from a in db.Nfts
                        .Include(a => a.Nftproject)
                        .AsSplitQuery()
                    where a.NftprojectId == project.Id && (a.State == state || state == "all") &&
                          a.MainnftId == null && a.Isroyaltytoken == false
                    orderby a.Selldate descending
                    select a).AsNoTracking().Skip((page - 1) * count).Take(count).ToListAsync();
            }
            if (orderby == "id")
            {
                nft = await (from a in db.Nfts
                        .Include(a => a.Nftproject)
                        .AsSplitQuery()
                    where a.NftprojectId == project.Id && (a.State == state || state == "all") &&
                          a.MainnftId == null && a.Isroyaltytoken == false
                    orderby a.Id 
                    select a).AsNoTracking().Skip((page - 1) * count).Take(count).ToListAsync();
            }
            if (orderby == "id_desc")
            {
                nft = await (from a in db.Nfts
                        .Include(a => a.Nftproject)
                        .AsSplitQuery()
                    where a.NftprojectId == project.Id && (a.State == state || state == "all") &&
                          a.MainnftId == null && a.Isroyaltytoken == false
                    orderby a.Id descending 
                    select a).AsNoTracking().Skip((page - 1) * count).Take(count).ToListAsync();
            }


            List<NFT> nftl = new();
            foreach (var a in nft)
            {
                string paylink =  GeneralConfigurationClass.Paywindowlink + "p=" + project.Uid.Replace("-", "") + "&n=" + a.Uid.Replace("-", "");
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
                    AssetId = string.IsNullOrEmpty(a.Assetid) ? GlobalFunctions.GetAssetId(a.Nftproject.Policyid, a.Nftproject.Tokennameprefix, a.Name) : a.Assetid,
                    Assetname = string.IsNullOrEmpty(a.Assetname) ? GlobalFunctions.ToHexString(a.Nftproject.Tokennameprefix + a.Name) : a.Assetname,
                    InitialMintTxHash = a.Initialminttxhash,
                    PolicyId = a.Policyid,
                    Series = a.Series,
                    Id = a.Id,
                    Uid = a.Uid,
                    Selldate = a.Selldate,
                    Price = GlobalFunctions.GetPrice(db,_redis, a,1),
                    PriceSolana=GlobalFunctions.GetPrice(db,_redis, a,1,Coin.SOL),
                    PriceAptos = GlobalFunctions.GetPrice(db, _redis, a, 1, Coin.APT),
                    PaymentGatewayLinkForSpecificSale = paylink
                });
            }

            await db.Database.CloseConnectionAsync();
            CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200, nftl, apiparameter);
            return Ok(nftl);
        }
    }
}
