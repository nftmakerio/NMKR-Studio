using System;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.Metadata;
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
    public class MintRoyaltyTokenController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public MintRoyaltyTokenController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Mints the royalty token
        /// </summary>
        /// <remarks>
        /// When you call this API, the royalty token for this project will be minted and send to a burning address. You have to specify the address for the royalties and the percentage of royalties. You need mint credits in your account. Only one royalty token can be minted for each project
        /// </remarks>
        /// <param Name="authorization">The apikey you have created on NMKR Studio - the apikey mus have the PurchaseSpecificNft permission</param>
        /// <param Name="projectuid">The uid of your project</param>
        /// <param Name="royaltyaddress">The address where the royalties should go to</param>
        /// <param Name="percentage">How high should the royalty fee be (in percent)?</param>
        /// <response code="200">The royaltytoken was created successfully</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectuid etc.)</response>     
        /// <response code="402">Too less ADA in your account. Fill up ADA first before try to mint and send</response>        
        /// <response code="409">There are pending transactions on the sender address (your account address). Please wait a second</response>
        /// <response code="406">The royaltyaddress is not a valid cardano address or a valid ada handle</response>
        /// <response code="406">The project already has a royalty token</response>
        /// <response code="500">Internal server error - see the errormessage in the result</response>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status402PaymentRequired, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpGet("{projectuid}/{royaltyaddress}/{percentage:double}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Mint" }
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string projectuid,
            string royaltyaddress, double percentage)
        {
          //  string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            if (Request.Method.Equals("HEAD"))
                return null;

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;

            await GlobalFunctions.LogMessageAsync(db, "Mint royalty address", projectuid);



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
                where a.Uid == projectuid && a.State == "active"
                                 select a).FirstOrDefaultAsync();

            if (project == null)
            {
                LogClass.LogMessage(db, "API-CALL: ERROR: Project not found " + projectuid);
                result.ErrorCode = 56;
                result.ErrorMessage = "Internal error. Please contact support";
                result.ResultState = ResultStates.Error;

                return StatusCode(500, result);
            }

            if (project.Policyexpire != null && project.Policyexpire < DateTime.Now)
            {
                LogClass.LogMessage(db, "API-CALL: ERROR: Project not found " + projectuid);
                result.ErrorCode = 4102;
                result.ErrorMessage = "Policy on this project is already locked. Minting not longer possible";
                result.ResultState = ResultStates.Error;

                return StatusCode(406, result);
            }

            int nftprojectid = project.Id;
            await GlobalFunctions.UpdateLastActionProjectAsync(db, nftprojectid,_redis);

        /*    if (project.Hasroyality)
            {
                result.ErrorCode = 4401;
                result.ErrorMessage = "The project already has a royalty token";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }*/


        /*    var nx = await (from a in db.Nfts
                    .Include(a => a.Nftproject)
                where a.NftprojectId == nftprojectid && a.Isroyaltytoken == true
                select a).AsNoTracking().FirstOrDefaultAsync();

            if (nx != null)
            {
                result.ErrorCode = 4402;
                result.ErrorMessage = "The project already has a royalty token";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }*/


            var b = ConsoleCommand.CheckIfAddressIsValid(db, royaltyaddress, GlobalFunctions.IsMainnet(),
                out string outaddress, out Blockchain blockchain, true);
            if (!b)
            {
                result.ErrorCode = 4403;
                result.ErrorMessage =
                    "The address for the royalties is not a valid cardano address or no valid adahandle";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }


            if (project.Customer.Newpurchasedmints < 1)
            {
                result.ErrorCode = 4404;
                result.ErrorMessage =
                    "You don't have enough mint credits for this transaction. Please buy mint credits first.";
                result.ResultState = ResultStates.Error;
                return StatusCode(402, result);
            }

            var be = await GlobalFunctions.CreateBurningAddressAsync(db, nftprojectid, DateTime.Now.AddMinutes(60),Blockchain.Cardano);
           
            if (be == null || string.IsNullOrEmpty(be.Address))
            {
                result.ErrorCode = 4405;
                result.ErrorMessage = "Error while creating burning endpoint. Please contact support";
                result.ResultState = ResultStates.Error;
                return StatusCode(500, result);
            }
            await GlobalFunctions.LogMessageAsync(db, "Mint royalty address - Burnin address", be.Address);
            string burningEndpoint = be.Address;

            NMKR.Shared.Model.Nft n = new()
            {
                NftprojectId = nftprojectid,
                Name = "",
                State = "reserved",
                Reservedcount = project.Maxsupply,
                Soldcount = 0,
                Errorcount = 0,
                Burncount = 0,
                Metadataoverride = ConsoleCommand.CreateRoyaltyTokenJson(royaltyaddress, percentage),
                Checkpolicyid = false,
                Uploadedtonftstorage = true,
                Isroyaltytoken = true,
                MetadatatemplateId = 1,
                Filename = "",
                Filesize = 0,
                Ipfshash = "",
                Minted = false,
                Uid = Guid.NewGuid().ToString()
            };

            await db.Nfts.AddAsync(n);
            await db.SaveChangesAsync();


            var nft = await (from a in db.Nfts
                    .Include(a => a.Nftproject)
                    .ThenInclude(a => a.Customer)
                    .AsSplitQuery()
                    .Include(a => a.Instockpremintedaddress)
                    .AsSplitQuery()
                    .Include(a => a.Nftproject)
                    .ThenInclude(a => a.Settings)
                    .AsSplitQuery()
                    .Include(a => a.InverseMainnft)
                    .ThenInclude(a => a.Metadata)
                    .AsSplitQuery()
                    .Include(a => a.Metadata)
                    .AsSplitQuery()
                where a.Id == n.Id
                select a).FirstOrDefaultAsync();

            if (nft == null)
            {
                result.ErrorCode = 4406;
                result.ErrorMessage = "Error while creating royalty token. Please contact support";
                result.ResultState = ResultStates.Error;
                return StatusCode(500, result);
            }


            var paywallet = await GlobalFunctions.GetFirstNmkrPaywalletAndBlockAsync(db, 0);
           
            if (paywallet == null)
            {
                result.ErrorCode = 4406;
                result.ErrorMessage = "All pay wallets are busy in the moment. Please try again in a few seconds";
                result.ResultState = ResultStates.Error;
                return StatusCode(500, result);
            }
            await GlobalFunctions.LogMessageAsync(db, "Mint royalty address - Paywallet address", paywallet.Address);
            GetMetadataClass gmc = new(nft.Id, "", true);
            MintManuallyClass mmc1 = new()
            {
                BurnResult = true,
                Metadata = (await gmc.MetadataResultAsync()).Metadata,
                PolicyId = nft.Nftproject.Policyid,
                Prefix = "",
                ReceiverAddress = burningEndpoint,
                Tokenname = "",
                Projectid = nft.NftprojectId,
                SenderAddress = paywallet.Address,
                SenderSKey = Encryption.DecryptString(paywallet.Privateskey, GeneralConfigurationClass.Masterpassword + paywallet.Salt),
                SenderVKey = Encryption.DecryptString(paywallet.Privatevkey, GeneralConfigurationClass.Masterpassword + paywallet.Salt)

            };

            var s = ConsoleCommand.MintManually(db,_redis, project, mmc1,  GlobalFunctions.IsMainnet(), 0, "", out var buildTransaction);


            if (s != "OK")
            {
                await GlobalFunctions.UnlockPaywalletAsync(db, paywallet);
                db.Nfts.Remove(nft);
                await db.SaveChangesAsync();

                await GlobalFunctions.LogMessageAsync(db, "Error while creating royalty token",
                    buildTransaction.LogFile);

                result.ErrorCode = 4407;
                result.ErrorMessage = "Error while creating royalty token. Please contact support";
                result.InnerErrorMessage = s;
                result.ResultState = ResultStates.Error;
                return StatusCode(500, result);
            }
            else
            {
                project.Hasroyality = true;
                project.Royalityaddress = royaltyaddress;
                project.Royalitypercent = (float) percentage;
                project.Royaltiycreated = DateTime.Now;

                nft.State = "sold";
                nft.Selldate = DateTime.Now;
                nft.Minted = true;
                nft.Receiveraddress = burningEndpoint;
                nft.Soldcount = 1;
                nft.Reservedcount = 0;
                nft.Errorcount = 0;

                await GlobalFunctions.ExecuteSqlWithFallbackAsync(db, $"update adminmintandsendaddresses set addressblocked=1,blockcounter=0,lasttxhash='{buildTransaction.TxHash}', lasttxdate=NOW() where id='{paywallet.Id}'", 0);
                await GlobalFunctions.SaveTransactionAsync(db,_redis, buildTransaction, nft.Nftproject.CustomerId, nft.NftprojectId, nameof(TransactionTypes.mintfromcustomeraddress), null, nft.Id, 1, Coin.ADA);
                await GlobalFunctions.ReduceMintCouponsAsync(db, project.CustomerId, 0.5f);


                await db.SaveChangesAsync();

                return Ok(n.Metadataoverride);
            }
        }
    }
}
