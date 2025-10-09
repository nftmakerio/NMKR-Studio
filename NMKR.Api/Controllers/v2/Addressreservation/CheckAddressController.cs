using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Blockchains;
using NMKR.Shared.Blockchains.APTOS;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.Extensions;
using NMKR.Shared.Functions.Solana;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;

namespace NMKR.Api.Controllers.v2.Addressreservation
{
    /// <summary>
    /// 
    /// </summary>
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class CheckAddressController : ControllerBase
    {

        private readonly IConnectionMultiplexer _redis;

        public CheckAddressController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }


        /// <summary>
        /// Checks an address for state changes (project uid)
        /// </summary>
        /// <remarks>
        /// You can call this api to check if a user has paid to this particular address or if the address has expired. The reserved/sold NFTs will only filled after the amount was fully paid. This is for security reasons. In the reserved state, only the nft ids and tokenamount are submitted 
        ///
        /// IMPORTANT:
        /// This function uses an internal cache. All results will be cached for 10 seconds. You do not need to call this function more than once in 10 seconds, because the results will be the same.
        /// </remarks>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="projectuid">The uid of your project (not the id)</param>
        /// <param Name="address">The address you want to check</param>
        /// <response code="200">Returns the Apiresultclass with the information about the address incl. the assigned NFTs</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="404">The address was not found in our database or not assiged to this project</response>            
        //  [HttpHead("{apikey}/{nftprojectid:int}/{address}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CheckAddressResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{projectuid}/{address}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Address reservation (sale)" }
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string projectuid, string address)
        {
          //  string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            if (Request.Method.Equals("HEAD"))
                return null;

            if (string.IsNullOrEmpty(address) || address == "undefined")
                return StatusCode(404);


            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = projectuid + address;

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok(JsonConvert.DeserializeObject<CheckAddressResultClass>(cachedResult.ResultString));
                return StatusCode(cachedResult.Statuscode, JsonConvert.DeserializeObject<ApiErrorResultClass>(cachedResult.ResultString));
            }

            // Check if the Apikey is valid
            var result = CheckCachedAccess.CheckApikeyOrToken(_redis, apifunction,
                projectuid, apikey, remoteIpAddress?.ToString());
            if (result.ResultState != ResultStates.Ok)
            {
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                return Unauthorized(result);
            }

            return await Check(result, new(apikey, apifunction, apiparameter), projectuid, address);
        }

        internal async Task<IActionResult> Check(ApiErrorResultClass result, CachedApiCallValues apivalues, string projectuid, string address)
        {
            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            {
                var project = await (from a in db.Nftprojects
                               where a.Uid == projectuid
                               select a).FirstOrDefaultAsync();

                if (project == null)
                {
                    result.ErrorCode = 50;
                    result.ErrorMessage = "Projectuid not found";
                    result.ResultState = ResultStates.Error;
                    CheckCachedAccess.SetCachedResult(_redis, apivalues, 404, result);
                    await db.Database.CloseConnectionAsync();
                    return NotFound(result);
                }

                int nftprojectid = project.Id;

                await GlobalFunctions.UpdateLastActionProjectAsync(db, nftprojectid,_redis);
               
                var adr = await (from a in db.Nftaddresses
                        .Include(a => a.Nfttonftaddresses)
                        .ThenInclude(a => a.Nft)
                    where a.Address == address && a.NftprojectId == nftprojectid
                    select a).FirstOrDefaultAsync();


                if (adr == null)
                {
                    result.ErrorCode = 50;
                    result.ErrorMessage = "Address not known";
                    result.ResultState = ResultStates.Error;
                    CheckCachedAccess.SetCachedResult(_redis, apivalues, 404, result);
                    await db.Database.CloseConnectionAsync();
                    return NotFound(result);
                }

                CheckAddressResultClass carc = new()
                {
                    PayDateTime = adr.Paydate,
                    ExpiresDateTime = adr.State != "paid" ? adr.Expires : null,
                    Lovelace = adr.Lovelace ?? 0,
                    SenderAddress = adr.Senderaddress,
                    State = adr.State,
                    Transaction = adr.Txid,
                    HasToPay = adr.Price ?? 0,
                    AdditionalPriceInTokens = GlobalFunctions.GetAdditionalTokens(adr),
                    RejectReason = adr.Rejectreason,
                    RejectParameter = adr.Rejectparameter,
                    StakeReward = adr.Stakereward,
                    TokenReward = adr.Tokenreward,
                    Discount = adr.Discount,
                    CustomProperty = adr.Customproperty,
                    CountNftsOrTokens = adr.Countnft ?? 1,
                    ReservationType=adr.Reservationtype,
                    Coin= adr.Coin.ToEnum<Coin>(),
                };

                if (carc.State == "active")
                {
                    ulong amount = 0;
                    switch (adr.Coin.ToEnum<Coin>())
                    {
                        case Coin.ADA:
                        {
                            var utxo = await ConsoleCommand.GetNewUtxoAsync(address);
                            amount = (ulong)utxo.LovelaceSummary;
                            carc.ReceivedCardanoLovelace = adr.Lovelace ?? 0;
                                break;
                        }
                        case Coin.SOL:
                        {
                            amount = await SolanaFunctions.GetWalletBalanceAsync(address);
                            carc.ReceivedSolanaLamports = adr.Lovelace ?? 0;
                                break;
                        }
                        case Coin.APT:
                        {
                            IBlockchainFunctions aptos = new AptosBlockchainFunctions();
                            amount = await aptos.GetWalletBalanceAsync(address);
                            carc.ReceivedAptosOctas = adr.Lovelace ?? 0;
                                break;
                        }
                    }

                    if (amount > 0)
                    {
                        carc.State = "payment_received";
                        string sql = "update nftaddresses set state='payment_received', expires=date_add(expires,interval 90 minute) where id=" + adr.Id + " and state='active'";
                        await db.Database.ExecuteSqlRawAsync(sql);
                    }
                } 

                List<NFT> n1 = new();
                foreach (var sn in adr.Nfttonftaddresses)
                {
                    if (adr.State == "paid" || carc.State=="payment_received")
                    {
                        n1.Add(new()
                        {
                            IpfsLink = "ipfs://" + sn.Nft.Ipfshash,
                            Name = string.IsNullOrEmpty(project.Tokennameprefix)
                                ? sn.Nft.Name
                                : project.Tokennameprefix + sn.Nft.Name,
                            GatewayLink = GeneralConfigurationClass.IPFSGateway + sn.Nft.Ipfshash,
                            Displayname = sn.Nft.Displayname,
                            Detaildata = sn.Nft.Detaildata,
                          
                            State = sn.Nft.State,
                            Minted = sn.Nft.Minted,
                            Id = sn.Id,
                            AssetId = string.IsNullOrEmpty(sn.Nft.Assetid)
                                ? GlobalFunctions.GetAssetId(project.Policyid, project.Tokennameprefix, sn.Nft.Name)
                                : sn.Nft.Assetid,
                            Assetname = string.IsNullOrEmpty(sn.Nft.Assetname)
                                ? GlobalFunctions.GetAssetId("", project.Tokennameprefix, sn.Nft.Name)
                                : sn.Nft.Assetname,
                            Fingerprint = sn.Nft.Fingerprint ?? Bech32Engine.GetFingerprint(sn.Nft.Policyid, sn.Nft.Assetname.ToHex()),
                            InitialMintTxHash = sn.Nft.Initialminttxhash,
                            PolicyId = sn.Nft.Policyid,
                            Series = sn.Nft.Series,
                            Tokenamount = sn.Tokencount,
                            Uid = sn.Nft.Uid,
                        });
                    }
                    else
                    {
                        n1.Add(new()
                        {
                            Id = sn.Id,
                            Uid = sn.Nft.Uid,
                            Tokenamount = sn.Tokencount,
                        });
                    }
                }

                carc.ReservedNft = n1.ToArray();

                result.ResultState = ResultStates.Ok;
                result.ErrorCode = 0;
                result.ErrorMessage = "";
                await db.Database.CloseConnectionAsync();
                CheckCachedAccess.SetCachedResult(_redis, apivalues, 200, carc);
                return Ok(carc);
            }
        }

        /// <summary>
        /// Checks an address for state changes (project uid)
        /// </summary>
        /// <remarks>
        /// You can call this api to check if a user has paid to this particular address or if the address has expired. The reserved/sold NFTs will only filled after the amount was fully paid. This is for security reasons. In the reserved state, only the nft ids and tokenamount are submitted 
        ///
        /// IMPORTANT:
        /// This function uses an internal cache. All results will be cached for 10 seconds. You do not need to call this function more than once in 10 seconds, because the results will be the same.
        /// </remarks>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="projectuid">The uid of your project (not the id)</param>
        /// <param Name="address">The address you want to check</param>
        /// <response code="200">Returns the Apiresultclass with the information about the address incl. the assigned NFTs</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="404">The address was not found in our database or not assiged to this project</response>            
        //  [HttpHead("{apikey}/{nftprojectid:int}/{address}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CheckAddressResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{address}")]
        [MapToApiVersion("2")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [SwaggerOperation(
            Tags = new[] { "Address reservation (sale)" }
        )]
        public async Task<IActionResult> Get(string address)
        {
            if (Request.Method.Equals("HEAD"))
                return null;

            if (string.IsNullOrEmpty(address) || address == "undefined")
                return StatusCode(404);


            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = address;

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, "", apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok(JsonConvert.DeserializeObject<CheckAddressResultClass>(cachedResult.ResultString));
                return StatusCode(cachedResult.Statuscode, JsonConvert.DeserializeObject<ApiErrorResultClass>(cachedResult.ResultString));
            }

            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            
            ApiErrorResultClass result = new ApiErrorResultClass();
            var apivalues = new CachedApiCallValues("", apifunction, apiparameter);

            var nftaddress = await (from a in db.Nftaddresses
                    .Include(a=>a.Nftproject).AsSplitQuery()
                where a.Address == address
                select a).AsNoTracking().FirstOrDefaultAsync();

            if (nftaddress == null)
            {
                var buyouaddress = await (from a in db.Buyoutsmartcontractaddresses
                    where a.Address == address
                    select a).AsNoTracking().FirstOrDefaultAsync();
                if (buyouaddress != null)
                {
                    await db.Database.CloseConnectionAsync();
                    return await CheckBuyout(result, apivalues, buyouaddress);
                }
            }

            if (nftaddress != null) return await Check(result, apivalues, nftaddress.Nftproject.Uid, address);

            result.ErrorCode = 50;
            result.ErrorMessage = "Address not known";
            result.ResultState = ResultStates.Error;
            CheckCachedAccess.SetCachedResult(_redis, apivalues, 404, result);
            await db.Database.CloseConnectionAsync();
            return NotFound(result);

        }

        internal async Task<IActionResult> CheckBuyout(ApiErrorResultClass result, CachedApiCallValues apivalues, Buyoutsmartcontractaddress adr)
        {
            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            {
                CheckAddressResultClass carc = new()
                {
                    ExpiresDateTime = adr.State != "paid" ? adr.Expiredate : null,
                    Lovelace = adr.Lovelace,
                    SenderAddress = adr.Receiveraddress,
                    State = adr.State,
                    Transaction = adr.Outgoingtxhash,
                    HasToPay = adr.Lovelace,
                    CountNftsOrTokens = 1,
                };

                if (carc.State == "active")
                {
                    var utxo = await ConsoleCommand.GetNewUtxoAsync(adr.Address);
                    if (utxo.LovelaceSummary > 0)
                    {
                        carc.State = "payment_received";
                        string sql = "update nftaddresses set state='payment_received', expires=date_add(expires,interval 90 minute) where id=" + adr.Id + " and state='active'";
                        await db.Database.ExecuteSqlRawAsync(sql);
                    }
                }

                result.ResultState = ResultStates.Ok;
                result.ErrorCode = 0;
                result.ErrorMessage = "";
                await db.Database.CloseConnectionAsync();
                CheckCachedAccess.SetCachedResult(_redis, apivalues, 200, carc);
                return Ok(carc);
            }
        }
    }
}
