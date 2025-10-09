using System;
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
    public class RemintAndBurnController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;
        public RemintAndBurnController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// ReMints a specific Nft and sends it to a burn address
        /// </summary>
        /// <remarks>
        /// When you call this API, you can update metadata of an already sold nft. The nft will be minted and send to a burning address
        /// </remarks>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <param Name="projectuid">The uid of your project</param>
        /// <param Name="nftid">The ID of the NFT</param>
        /// <response code="200">Remint is scheduled</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectuid etc.)</response>     
        /// <response code="404">NFT no more available (already sold)</response>            
        /// <response code="402">Too less ADA in your account. Fill up ADA first before try to mint and send</response>        
        /// <response code="409">There are pending transactions on the sender address (your account address). Please wait a second</response>
        /// <response code="406">The receiveraddress is not a valid cardano address or a valid ada handle</response>
        /// <response code="500">Internal server error - see the errormessage in the result</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status402PaymentRequired, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{projectuid}/{nftuid}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Mint" }
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string projectuid, string nftuid)
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
                return StatusCode(cachedResult.Statuscode,
                    JsonConvert.DeserializeObject<ApiErrorResultClass>(cachedResult.ResultString));


            // Check if the Apikey is valid
            var result = CheckCachedAccess.CheckApikeyOrToken(_redis, apifunction,
                 "", apikey, remoteIpAddress?.ToString());
            if (result.ResultState != ResultStates.Ok)
            {
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 401, result, apiparameter);
                return Unauthorized(result);
            }





            var project = await (from a in db.Nftprojects
                    .Include(a => a.Customer)
                    .Include(a => a.Customerwallet)
                    .Include(a => a.Settings)
                where a.Uid == projectuid
                select a).FirstOrDefaultAsync();

            if (project == null)
            {
                LogClass.LogMessage(db, "API-CALL: ERROR: Project not found " + projectuid);
                result.ErrorCode = 56;
                result.ErrorMessage = "Project not found. Please submit valid project UID";
                result.ResultState = ResultStates.Error;

                return StatusCode(406, result);
            }

            int nftprojectid = project.Id;
            await GlobalFunctions.UpdateLastActionProjectAsync(db, nftprojectid,_redis);

            if (!GlobalFunctions.CheckExpirationSlot(project))
            {
                result.ErrorCode = 205;
                result.ErrorMessage = "Policy is already locked. No further minting possible (5)";
                result.ResultState = ResultStates.Error;
                return StatusCode(404, result);
            }


            var nx = await (from a in db.Nfts
                    .Include(a => a.Nftproject)
                where a.Uid == nftuid && a.NftprojectId == nftprojectid
                select a).AsNoTracking().FirstOrDefaultAsync();

            if (nx == null)
            {
                LogClass.LogMessage(db, "API-CALL" + remoteIpAddress.ToString() +
                                        ": NFT not available (NFT ID wrong - specific) Project: " + nftprojectid +
                                        " - NftId: " + nftuid);
                result.ErrorCode = 10;
                result.ErrorMessage = "NFT not available (NFT UID wrong)";
                result.ResultState = ResultStates.Error;
                return NotFound(result);
            }

            if (nx.State != "sold")
            {
                LogClass.LogMessage(db, "API-CALL" + remoteIpAddress.ToString() +
                                        ": NFT not in sold state (NFT ID wrong - specific) Project: " + nftprojectid +
                                        " - NftId: " + nftuid);
                result.ErrorCode = 11;
                result.ErrorMessage = "NFT not in sold state";
                result.ResultState = ResultStates.Error;
                return StatusCode(406,result);
            }

            var nftreservations = await (from a in db.Nftreservations
                where a.NftId == nx.Id
                select a).AsNoTracking().FirstOrDefaultAsync();

            if (nftreservations != null)
            {
                LogClass.LogMessage(db, "API-CALL" + remoteIpAddress.ToString() +
                                        ": NFT already reserved for minting: " + nftprojectid +
                                        " - NftId: " + nftuid);
                result.ErrorCode = 409;
                result.ErrorMessage = "NFT already reserved for minting ";
                result.ResultState = ResultStates.Error;
                return StatusCode(409,result);
            }



            string guid = GlobalFunctions.GetGuid();

            if ((project.Customer.Newpurchasedmints < 1))
            {
                LogClass.LogMessage(db, "MintNfts - Too less Mints in your internal Account. Fill up first " +
                                        project.Projectname);
                result.ErrorCode = 54;
                result.ErrorMessage = "Too less Mints in your internal Account. Fill up first";
                result.ResultState = ResultStates.Error;
                await NftReservationClass.ReleaseAllNftsAsync(db, _redis, guid);
                return StatusCode(402, result);
            }

            // Create Reservation
            await db.Nftreservations.AddAsync(new()
            {
                Mintandsendcommand = true, NftId = nx.Id, Tc = 1, Reservationtoken = guid, Reservationtime = 60,
                Reservationdate = DateTime.Now, Multiplier = 1
            });
            await db.SaveChangesAsync();


            // Create Burn Address
            var be = await GlobalFunctions.CreateBurningAddressAsync(db, nftprojectid, DateTime.Now.AddHours(24),Blockchain.Cardano,true, false);


            var mas = new Mintandsend()
            {
                Created = DateTime.Now,
                CustomerId = project.CustomerId,
                NftprojectId = project.Id,
                Receiveraddress = be.Address,
                Reservationtoken = guid,
                State = "execute",
                Onlinenotification = false,
                Reservelovelace = 0,
                Usecustomerwallet = true, 
                Remintandburn = true,
                Coin = Coin.ADA.ToString(),
            };
            await db.Mintandsends.AddAsync(mas);
            await db.SaveChangesAsync();


            LogClass.LogMessage(db, "ReMintAndBurnFromApi - Send Result OK " + be.Address);

            result.ErrorCode = 0;
            result.ErrorMessage = "";
            result.ResultState = ResultStates.Ok;
            await db.Database.CloseConnectionAsync();
            return Ok();
        }
    }
}
