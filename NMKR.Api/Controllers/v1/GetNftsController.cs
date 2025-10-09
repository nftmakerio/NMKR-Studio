using System.Collections.Generic;
using NMKR.Shared.Classes;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Asp.Versioning;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NMKR.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ApiVersion("1")]
    public class GetNftsController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public GetNftsController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Returns detail information about all nfts with a specific state. *** DEPRECATED - use the paged version ***
        /// </summary>
        /// <remarks>
        /// You will receive all information (fingerprint, ipfshash, etc.) about the nfts within a specific state.
        /// State "all" lists all available nft in this project. The other states are: "free", "reserved", "sold" and "error"
        /// *** DEPRECATED - use the paged version ***
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
        [HttpGet("{apikey}/{nftprojectid:int}/{state}")]
        [MapToApiVersion("1")]
        public IActionResult Get(string apikey, int nftprojectid, string state)
        {
            if (Request.Method.Equals("HEAD"))
                return null;
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = nftprojectid + state;

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
                "", apikey, remoteIpAddress?.ToString() ?? string.Empty);
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

            using (var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options))
            {

                List<Nft> nft;
                if (state == "sold")
                {
                    nft = (from a in db.Nfts
                            .Include(a => a.Nftproject)
                            .ThenInclude(a => a.Settings)
                            .AsSplitQuery()
                           where a.NftprojectId == nftprojectid && (a.State == state) &&
                              a.MainnftId == null && a.Isroyaltytoken == false
                        orderby a.Selldate descending
                        select a).AsNoTracking().ToList();
                }
                else
                {
                    nft = (from a in db.Nfts
                            .Include(a => a.Nftproject)
                            .ThenInclude(a=>a.Settings)
                            .AsSplitQuery()
                           where a.NftprojectId == nftprojectid && (a.State == state || state == "all") &&
                              a.MainnftId == null && a.Isroyaltytoken == false
                        select a).AsNoTracking().ToList();
                }


                List<NFT> nftl = new();
                foreach (var a in nft)
                {
                    string paylink =  GeneralConfigurationClass.Paywindowlink + "p=" + a.Nftproject.Uid.Replace("-", "") + "&n=" + a.Uid.Replace("-", "");

                    var price = GlobalFunctions.GetPrice(db,_redis, a,1);
                    var pricesolana = GlobalFunctions.GetPrice(db, _redis, a, 1, Coin.SOL);
                    var priceaptos = GlobalFunctions.GetPrice(db, _redis, a, 1, Coin.APT);

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
                        Selldate=a.Selldate,
                        Price = price,
                        PriceSolana=pricesolana,
                        PriceAptos = priceaptos,
                        PaymentGatewayLinkForSpecificSale = paylink
                    });
                }

                db.Database.CloseConnection();
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200, nftl, apiparameter);
                return Ok(nftl);
            }
        }

        /// <summary>
        /// Returns detail information about nfts with a specific state with Pagination support. (project id)
        /// </summary>
        /// <remarks>
        /// You will receive all information (fingerprint, ipfshash, etc.) about the nfts within a specific state.
        /// State "all" lists all available nft in this project. The other states are: "free", "reserved", "sold" and "error"
        /// </remarks>
        /// <param Name="apikey">The apikey you have created on NMKR Studio</param>
        /// <param Name="nftprojectid">The id of your project</param>
        /// <param Name="state">The state of the nfts you want to receive</param>
        /// <param Name="count">How may NFTs do you want. Max 50. Min 1</param>
        /// <param Name="page">The page number. Starts with 1</param>
        /// <param name="orderby">(Optional) The sort order of the result. Possible values are: id (default),id_desc (descending order), selldate (on sold nfts) and selldate_desc (descending order)</param>
        /// <response code="200">Returns a List of the NFT Class</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="406">The state was not known - possible states are: free, reserved, sold, error and all</response>            
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<NFT>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{apikey}/{nftprojectid:int}/{state}/{count:int}/{page:int}")]
        [MapToApiVersion("1")]
        public IActionResult Get(string apikey, int nftprojectid, string state, int count, int page, [FromQuery] string? orderby = "id")
        {
            if (Request.Method.Equals("HEAD"))
                return null;
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = nftprojectid + state + count + "-" + page;

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
                "", apikey, remoteIpAddress?.ToString() ?? string.Empty);
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

            using (var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options))
            {
                if (page == 0)
                {
                    result.ErrorCode = 20;
                    result.ErrorMessage = "Pagenumber must start with 1";
                    result.ResultState = ResultStates.Error;
                    db.Database.CloseConnection();
                    CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 406, result, apiparameter);
                    return StatusCode(406, result);
                }

                if (count == 0)
                {
                    result.ErrorCode = 20;
                    result.ErrorMessage = "Count must be at least 1";
                    result.ResultState = ResultStates.Error;
                    db.Database.CloseConnection();
                    CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 406, result, apiparameter);
                    return StatusCode(406, result);
                }

                if (count > 50)
                {
                    result.ErrorCode = 20;
                    result.ErrorMessage = "Count can not exceed 50";
                    result.ResultState = ResultStates.Error;
                    db.Database.CloseConnection();
                    CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 406, result, apiparameter);
                    return StatusCode(406, result);
                }
                if (orderby != "id" && orderby != "id_desc" && orderby != "selldate" && orderby != "selldate_desc")
                {
                    result.ErrorCode = 22;
                    result.ErrorMessage = "Orderby is not valid";
                    result.ResultState = ResultStates.Error;
                    db.Database.CloseConnection();
                    CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 406, result, apiparameter);
                    return StatusCode(406, result);
                }

                List<Nft> nft = new();
                if (orderby == "selldate")
                {
                    nft = (from a in db.Nfts
                            .Include(a=>a.Nftproject)
                            .AsSplitQuery()
                        where a.NftprojectId == nftprojectid && (a.State == state || state == "all") &&
                              a.MainnftId == null && a.Isroyaltytoken == false
                        orderby a.Selldate
                        select a).AsNoTracking().Skip((page - 1) * count).Take(count).ToList();
                }
                if (orderby == "selldate_desc")
                {
                    nft = (from a in db.Nfts
                            .Include(a => a.Nftproject)
                            .AsSplitQuery()
                           where a.NftprojectId == nftprojectid && (a.State == state || state == "all") &&
                              a.MainnftId == null && a.Isroyaltytoken == false
                        orderby a.Selldate descending
                        select a).AsNoTracking().Skip((page - 1) * count).Take(count).ToList();
                }
                if (orderby == "id")
                {
                    nft = (from a in db.Nfts
                            .Include(a => a.Nftproject)
                            .AsSplitQuery()
                           where a.NftprojectId == nftprojectid && (a.State == state || state == "all") &&
                              a.MainnftId == null && a.Isroyaltytoken == false
                        orderby a.Id
                        select a).AsNoTracking().Skip((page - 1) * count).Take(count).ToList();
                }
                if (orderby == "id_desc")
                {
                    nft = (from a in db.Nfts
                            .Include(a => a.Nftproject)
                            .AsSplitQuery()
                           where a.NftprojectId == nftprojectid && (a.State == state || state == "all") &&
                              a.MainnftId == null && a.Isroyaltytoken == false
                        orderby a.Id descending
                        select a).AsNoTracking().Skip((page - 1) * count).Take(count).ToList();
                }


                List<NFT> nftl = new();
                foreach (var a in nft)
                {
                    string paylink =  GeneralConfigurationClass.Paywindowlink + "p=" + a.Nftproject.Uid.Replace("-", "") + "&n=" + a.Uid.Replace("-", "");

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
                        Selldate = a.Selldate,
                        Price = GlobalFunctions.GetPrice(db,_redis, a,1),
                        PriceSolana = GlobalFunctions.GetPrice(db, _redis, a, 1,Coin.SOL),
                        PriceAptos= GlobalFunctions.GetPrice(db, _redis, a, 1, Coin.APT),
                        PaymentGatewayLinkForSpecificSale = paylink
                    });
                }

                db.Database.CloseConnection();
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200, nftl, apiparameter);
                return Ok(nftl);
            }
        }


        /// <summary>
        /// Returns detail information about nfts with a specific state with Pagination support. (project uid)
        /// </summary>
        /// <remarks>
        /// You will receive all information (fingerprint, ipfshash, etc.) about the nfts within a specific state.
        /// State "all" lists all available nft in this project. The other states are: "free", "reserved", "sold" and "error"
        /// </remarks>
        /// <param Name="apikey">The apikey you have created on NMKR Studio</param>
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
        [HttpGet("{apikey}/{projectuid}/{state}/{count:int}/{page:int}")]
        [MapToApiVersion("1")]
        public IActionResult Get(string apikey, string projectuid, string state, int count, int page, [FromQuery] string? orderby = "id")
        {
            if (Request.Method.Equals("HEAD"))
                return null;
            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = projectuid + state + count + "-" + page;

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
                 "", apikey, remoteIpAddress?.ToString() ?? string.Empty);
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

            using (var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options))
            {
                if (page == 0)
                {
                    result.ErrorCode = 20;
                    result.ErrorMessage = "Pagenumber must start with 1";
                    result.ResultState = ResultStates.Error;
                    db.Database.CloseConnection();
                    CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 406, result, apiparameter);
                    return StatusCode(406, result);
                }

                if (count == 0)
                {
                    result.ErrorCode = 20;
                    result.ErrorMessage = "Count must be at least 1";
                    result.ResultState = ResultStates.Error;
                    db.Database.CloseConnection();
                    CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 406, result, apiparameter);
                    return StatusCode(406, result);
                }

                if (count > 50)
                {
                    result.ErrorCode = 20;
                    result.ErrorMessage = "Count can not exceed 50";
                    result.ResultState = ResultStates.Error;
                    db.Database.CloseConnection();
                    CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 406, result, apiparameter);
                    return StatusCode(406, result);
                }

                var project = (from a in db.Nftprojects
                    where a.Uid == projectuid
                    select a).FirstOrDefault();

                if (project == null)
                {
                    result.ErrorCode = 50;
                    result.ErrorMessage = "Projectuid not known";
                    result.ResultState = ResultStates.Error;
                    db.Database.CloseConnection();
                    CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 404, result, apiparameter);
                    return StatusCode(406, result);
                }

                if (orderby != "id" && orderby != "id_desc" && orderby != "selldate" && orderby != "selldate_desc")
                {
                    result.ErrorCode = 22;
                    result.ErrorMessage = "Orderby is not valid";
                    result.ResultState = ResultStates.Error;
                    db.Database.CloseConnection();
                    CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 406, result, apiparameter);
                    return StatusCode(406, result);
                }

                List<Nft> nft = new();
                if (orderby == "selldate")
                {
                    nft = (from a in db.Nfts
                            .Include(a => a.Nftproject)
                            .AsSplitQuery()
                           where a.NftprojectId == project.Id && (a.State == state || state == "all") &&
                              a.MainnftId == null && a.Isroyaltytoken == false
                        orderby a.Selldate
                        select a).AsNoTracking().Skip((page - 1) * count).Take(count).ToList();
                }
                if (orderby == "selldate_desc")
                {
                    nft = (from a in db.Nfts
                            .Include(a => a.Nftproject)
                            .AsSplitQuery()
                           where a.NftprojectId == project.Id && (a.State == state || state == "all") &&
                              a.MainnftId == null && a.Isroyaltytoken == false
                        orderby a.Selldate descending
                        select a).AsNoTracking().Skip((page - 1) * count).Take(count).ToList();
                }
                if (orderby == "id")
                {
                    nft = (from a in db.Nfts
                            .Include(a => a.Nftproject)
                            .AsSplitQuery()
                           where a.NftprojectId == project.Id && (a.State == state || state == "all") &&
                              a.MainnftId == null && a.Isroyaltytoken == false
                        orderby a.Id
                        select a).AsNoTracking().Skip((page - 1) * count).Take(count).ToList();
                }
                if (orderby == "id_desc")
                {
                    nft = (from a in db.Nfts
                            .Include(a => a.Nftproject)
                            .AsSplitQuery()
                           where a.NftprojectId == project.Id && (a.State == state || state == "all") &&
                              a.MainnftId == null && a.Isroyaltytoken == false
                        orderby a.Id descending
                        select a).AsNoTracking().Skip((page - 1) * count).Take(count).ToList();
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
                        AssetId = a.Assetid,
                        Assetname = a.Assetname,
                        InitialMintTxHash = a.Initialminttxhash,
                        PolicyId = a.Policyid,
                        Series = a.Series,
                        Id = a.Id,
                        Uid = a.Uid,
                        Selldate = a.Selldate,
                        Price = GlobalFunctions.GetPrice(db,_redis, a,1),
                        PriceSolana=GlobalFunctions.GetPrice(db,_redis,a,1,Coin.SOL),
                        PriceAptos= GlobalFunctions.GetPrice(db, _redis, a, 1, Coin.APT),
                        PaymentGatewayLinkForSpecificSale = paylink
                    });
                }

                db.Database.CloseConnection();
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200, nftl, apiparameter);
                return Ok(nftl);
            }


        }
    }
}
