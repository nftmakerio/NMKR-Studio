using System;
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

namespace NMKR.Api.Controllers.v2.Mint
{
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
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
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="projectuid">The uid of your project</param>
        /// <param Name="countnft">The count of the nft you want to mint and send</param>
        /// <param Name="receiveraddress">The cardano address or the adahandle you want to send the nft</param>
        /// <response code="200">Returns the Nft Class</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectuid etc.)</response>     
        /// <response code="404">No more nft available</response>            
        /// <response code="402">Too less ADA in your account. Fill up ADA first before try to mint and send</response>        
        /// <response code="409">There are pending transactions on the sender address (your account address). Please wait a second</response>
        /// <response code="406">The receiveraddress is not a valid cardano address or a valid ada handle</response>
        /// <response code="500">Internal server error - see the errormessage in the result</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MintAndSendResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status402PaymentRequired, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{projectuid}/{countnft:int}/{receiveraddress}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Mint" }
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string projectuid, int countnft, string receiveraddress, [FromQuery] Blockchain blockchain = Blockchain.Cardano)
        {
          //  string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            if (Request.Method.Equals("HEAD"))
                return null;

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;



            string apifunction = this.GetType().Name;
            string apiparameter = "";

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


               

            var project = await (from a in db.Nftprojects
                    .Include(a=>a.Customer)
                    .Include(a => a.Customerwallet)
                    .Include(a => a.Settings)
                where a.Uid == projectuid
                select a).FirstOrDefaultAsync();

                
            if (project == null)
            {
                LogClass.LogMessage(db,"API-CALL: ERROR: Project not found " + projectuid);
                result.ErrorCode = 56;
                result.ErrorMessage = "Project not found. Please submit valid project UID";
                result.ResultState = ResultStates.Error;

                return StatusCode(406, result);
            }
            await GlobalFunctions.UpdateLastActionProjectAsync(db, project.Id,_redis);


            if (!GlobalFunctions.CheckExpirationSlot(project) && blockchain== Blockchain.Cardano)
            {
                result.ErrorCode = 205;
                result.ErrorMessage = "Policy is already locked. No further minting possible (3)";
                result.ResultState = ResultStates.Error;
                return StatusCode(404, result);
            }

            string guid = GlobalFunctions.GetGuid();

            var selectedreservations = await NftReservationClass.ReserveRandomNft(db,_redis, guid, project.Id, countnft, project.Expiretime, true, false, Coin.ADA);

            if (selectedreservations.Count < countnft)
            {
                result.ErrorCode = 10;
                result.ErrorMessage = "No more NFT available";
                result.ResultState = ResultStates.Error;
                await GlobalFunctions.LogMessageAsync(db, "Api: " + result.ErrorMessage + $" - {result.ErrorCode} - Project: {project.Id}",
                    "MintAndSendRandom");
                return NotFound(result);
            }

              
            if ((project.Customer.Newpurchasedmints < selectedreservations.Count*project.Settings.Mintandsendcoupons))
            {
                LogClass.LogMessage(db, "MintNfts - Too less Mints in your internal Account. Fill up first " +
                                        project.Projectname);
                result.ErrorCode = 54;
                result.ErrorMessage = "Too less Mints in your internal Account. Fill up first";
                result.ResultState = ResultStates.Error;
                await NftReservationClass.ReleaseAllNftsAsync(db, _redis, guid);
                return StatusCode(402, result);
            }

            var b = ConsoleCommand.CheckIfAddressIsValid(db,receiveraddress, GlobalFunctions.IsMainnet(), out string outaddress, out Blockchain blockchainx, true);
            if (!b)
            {
                LogClass.LogMessage(db,"MintNfts -The receiveraddress is not a valid address " +
                                       project.Projectname + " " + receiveraddress);
                result.ErrorCode = 59;
                result.ErrorMessage = "The receiveraddress is not a valid address";
                result.ResultState = ResultStates.Error;
                await NftReservationClass.ReleaseAllNftsAsync(db,_redis, guid);
                return StatusCode(406, result);
            }
            if (blockchainx != blockchain)
            {
                LogClass.LogMessage(db, "MintAndSendSpecific -The receiveraddress is not valid " +
                                        project.Projectname + " " + receiveraddress);
                result.ErrorCode = 59;
                result.ErrorMessage = "The receiveraddress is not a valid for the selected blockchain";
                result.ResultState = ResultStates.Error;
                await NftReservationClass.ReleaseAllNftsAsync(db, _redis, guid);
                return StatusCode(406, result);
            }

            receiveraddress = outaddress;

            var blocked=await GlobalFunctions.CheckForBlockedAddresses(db, receiveraddress);
            if (blocked)
            {
                LogClass.LogMessage(db, "MintAndSendSpecific -The receiveraddress is blocked " +
                                        project.Projectname + " " + receiveraddress);
                result.ErrorCode = 59;
                result.ErrorMessage = "The receiveraddress is blocked";
                result.ResultState = ResultStates.Error;
                await NftReservationClass.ReleaseAllNftsAsync(db, _redis, guid);
                return StatusCode(406, result);
            }


            if (!project.Enabledcoins.Contains(blockchain.ToCoin().ToString()))
            {
                result.ErrorCode = 545;
                result.ErrorMessage = $"This project is not enabled for the {blockchain.ToString()} Blockchain";
                result.ResultState = ResultStates.Error;
                await NftReservationClass.ReleaseAllNftsAsync(db, _redis, guid);
                return StatusCode(406, result);
            }


            var mas = new Mintandsend()
            {
                Created = DateTime.Now,
                CustomerId = project.CustomerId,
                NftprojectId = project.Id,
                Receiveraddress = receiveraddress,
                Reservationtoken = guid,
                State = "execute",
                Onlinenotification = false,
                Reservelovelace = 0,
                Coin = blockchain.ToCoin().ToString(),
            };
            await db.Mintandsends.AddAsync(mas);
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


            MintAndSendResultClass masrc = new() {SendedNft = n1.ToArray(), MintAndSendId = mas.Id};
            result.ErrorCode = 0;
            result.ErrorMessage = "";
            result.ResultState = ResultStates.Ok;
            await db.Database.CloseConnectionAsync();
            return Ok(masrc);
        }

    }
}
