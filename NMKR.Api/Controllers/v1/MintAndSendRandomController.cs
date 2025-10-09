using NMKR.Shared.Classes;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using StackExchange.Redis;

namespace NMKR.Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [ApiVersion("1")]
    public class MintAndSendRandomController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public MintAndSendRandomController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }
        /// <summary>
        /// Mints random Nfts and sends it to an Address
        /// </summary>
        /// <remarks>
        /// When you call this API, random NFTs will be selected, minted and send to an ada address. You will need ADA in your Account for the transaction and minting costs.
        /// </remarks>
        /// <param Name="apikey">The apikey you have created on NMKR Studio</param>
        /// <param Name="nftprojectid">The id of your project</param>
        /// <param Name="countnft">The count of the nft you want to mint and send</param>
        /// <param Name="receiveraddress">The cardano address or the adahandle you want to send the nft</param>
        /// <response code="200">Returns the Nft Class</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="404">No more nft available</response>            
        /// <response code="402">Too less ADA in your account. Fill up ADA first before try to mint and send</response>        
        /// <response code="409">There are pending transactions on the sender address (your account address). Please wait a second</response>
        /// <response code="406">The receiveraddress is not a valid cardano address or a valid adahandle</response>
        /// <response code="500">Internal server error - see the errormessage in the result</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MintAndSendResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status402PaymentRequired, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{apikey}/{nftprojectid:int}/{countnft:int}/{receiveraddress}")]
        [MapToApiVersion("1")]
        public async Task<IActionResult> Get(string apikey, int nftprojectid, int countnft, string receiveraddress)
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

                var project = await (from a in db.Nftprojects
                        .Include(a=>a.Customer)
                        .AsSplitQuery()
                        .Include(a => a.Customerwallet)
                        .AsSplitQuery()
                        .Include(a => a.Settings)
                        .AsSplitQuery()
                                     where a.Id == nftprojectid
                    select a).FirstOrDefaultAsync();

                if (project == null)
                {
                    LogClass.LogMessage(db,"API-CALL: ERROR: Project not found " + nftprojectid);
                    result.ErrorCode = 56;
                    result.ErrorMessage = "Internal error. Please contact support";
                    result.ResultState = ResultStates.Error;

                    return StatusCode(500, result);
                }
              

                string guid = GlobalFunctions.GetGuid();


                var selectedreservations = await NftReservationClass.ReserveRandomNft(db,_redis,guid,nftprojectid,countnft,project.Expiretime,true, false, Coin.ADA);


                if (selectedreservations.Count < countnft)
                {
                    result.ErrorCode = 10;
                    result.ErrorMessage = "No more NFT available";
                    result.ResultState = ResultStates.Error;
                    await GlobalFunctions.LogMessageAsync(db, "Api: " + result.ErrorMessage + $" - {result.ErrorCode} - Project: {project.Id}",
                        "MintAndSendRandom");
                    return NotFound(result);
                }


              
                if ((project.Customer.Newpurchasedmints < selectedreservations.Count))
                {
                    LogClass.LogMessage(db, "MintNfts - Too less Mints in your internal Account. Fill up first " +
                                            project.Projectname);
                    result.ErrorCode = 54;
                    result.ErrorMessage = "Too less Mints in your internal Account. Fill up first";
                    result.ResultState = ResultStates.Error;
                    await NftReservationClass.ReleaseAllNftsAsync(db, _redis, guid);
                    return StatusCode(402, result);
                }
                var b = ConsoleCommand.CheckIfAddressIsValid(db,receiveraddress, GlobalFunctions.IsMainnet(), out string outaddress, out Blockchain blockchain, true);
                if (!b)
                {
                    LogClass.LogMessage(db,"MintNfts -The receiveraddress is not a valid cardano address or not a valid adahandle" +
                                        project.Projectname + " " + receiveraddress);
                    result.ErrorCode = 59;
                    result.ErrorMessage = "The receiveraddress is not a cardanoaddress";
                    result.ResultState = ResultStates.Error;
                    await NftReservationClass.ReleaseAllNftsAsync(db,_redis, guid);
                    return StatusCode(406, result);
                }

                receiveraddress = outaddress;


                await db.Mintandsends.AddAsync(new()
                {
                    Created = DateTime.Now, CustomerId = project.CustomerId, Coin = blockchain==Blockchain.Cardano? Coin.ADA.ToString() : Coin.SOL.ToString(),
                    NftprojectId = project.Id, Receiveraddress = receiveraddress,
                    Reservationtoken = guid, State = "execute", Onlinenotification = false, Reservelovelace = 0,
                });
                await db.SaveChangesAsync();

              
                LogClass.LogMessage(db,"MintAndSendFromApi - Send Result OK " + receiveraddress);

                List<NFT> n1 = new();
                foreach (var sn in selectedreservations)
                {
                    var nx1 = (from a in db.Nfts
                        where a.Id == sn.NftId
                        select a).FirstOrDefault();

                    if (nx1 == null)
                        continue;

                    n1.Add(new()
                    {
                        IpfsLink = "ipfs://" + nx1.Ipfshash,
                        Name = nx1.Name,
                        GatewayLink = GeneralConfigurationClass.IPFSGateway + nx1.Ipfshash,
                        State = nx1.State,
                        Minted = nx1.Minted,
                        Id = sn.NftId,
                        Uid=nx1.Uid,
                    });
                }


                MintAndSendResultClass masrc = new() {SendedNft = n1.ToArray()};
                result.ErrorCode = 0;
                result.ErrorMessage = "";
                result.ResultState = ResultStates.Ok;
                await db.Database.CloseConnectionAsync();
                return Ok(masrc);
            }
        }

    }
}
