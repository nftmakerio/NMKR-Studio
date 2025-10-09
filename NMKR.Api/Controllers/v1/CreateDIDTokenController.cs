using System;
using System.Linq;
using System.Threading.Tasks;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.Metadata;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Asp.Versioning;

namespace NMKR.Api.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("api/[controller]")]
    [ApiController]
    [ApiVersion("1")]
    public class CreateDIDTokenController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public CreateDIDTokenController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }
        /// <summary>
        /// Creates a digital Identity
        /// </summary>
        /// <param Name="didJson">The UploadNft Class as Body Content</param>
        /// <response code="200">Returns the UploadNftResult Class</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="404">No Image Content was provided. Send a file either as Base64 or as Link or IPFS Hash</response>            
        /// <response code="406">See the errormessage in the resultset for further information</response>
        /// <response code="409">There is a conflict with the provided images. Send a file either as Base64 or as Link or IPFS Hash</response>
        /// <response code="500">Internal server error - see the errormessage in the resultset</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UploadNftResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status406NotAcceptable, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpPost("{secret}/{policyid}")]
        [MapToApiVersion("1")]
        public async Task<IActionResult> Post(string secret,string policyId, [FromBody] string didJson)
        {
            if (Request.Method.Equals("HEAD"))
                return null;

            ApiErrorResultClass result = new();
            if (string.IsNullOrEmpty(didJson))
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode =3001;
                result.ErrorMessage = "Invalid DID Data submitted";
                return StatusCode(406, result);
            }
            if (string.IsNullOrEmpty(policyId))
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 3002;
                result.ErrorMessage = "Invalid PolicyId submitted";
                return StatusCode(406, result);
            }
            if (string.IsNullOrEmpty(secret))
            {
                result.ResultState = ResultStates.Error;
                result.ErrorCode = 3004;
                result.ErrorMessage = "Invalid secret submitted";
                return StatusCode(401, result);
            }
            await using (var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options))
            {
                var secrets = await (from a in db.Getaccesstokensusers
                    where a.Secret == secret && a.State == "active"
                    select a).FirstOrDefaultAsync();

                if (secrets == null)
                {
                    result.ResultState = ResultStates.Error;
                    result.ErrorCode = 3004;
                    result.ErrorMessage = "Invalid secret submitted";
                    return StatusCode(401, result);
                }


                var did = await (from a in db.Digitalidentities
                        .Include(a=>a.Nftproject)
                        .ThenInclude(a=>a.Customer)
                        .AsSplitQuery()
                    where a.Policyid == policyId && a.State == nameof(DidStateTypes.active)
                    select a).FirstOrDefaultAsync();

                if (did == null)
                {
                    result.ResultState = ResultStates.Error;
                    result.ErrorCode = 3003;
                    result.ErrorMessage = "Invalid PolicyId submitted or DID already created";
                    return StatusCode(406, result);
                }

                if (did.Created < DateTime.Now.AddHours(-3))
                {
                    result.ResultState = ResultStates.Error;
                    result.ErrorCode = 3005;
                    result.ErrorMessage = "DID Request expired";

                    did.State = nameof(DidStateTypes.expired);
                    await db.SaveChangesAsync();
                    
                    return StatusCode(406, result);
                }

                if (did.Nftproject.Customer.Newpurchasedmints < 1)
                {
                    result.ResultState = ResultStates.Error;
                    result.ErrorCode = 3006;
                    result.ErrorMessage = "Customer has too less mints/ada in his account";

                    did.State = nameof(DidStateTypes.canceled);
                    await db.SaveChangesAsync();

                    return StatusCode(406, result);

                }

                did.Didjsonresult=didJson;
                did.Didresultreceived=DateTime.Now;
                did.State = nameof(DidStateTypes.didresultreceived);
                await db.SaveChangesAsync();

                // Mint Token
             //   ConsoleCommand.MintAndSend()

             var res=await MintAndSendDidToken(db, did, didJson);

             did.Resultmessage = res;

             if (res != "OK")
             {
                 result.ResultState = ResultStates.Error;
                 result.ErrorCode = 3009;
                 result.ErrorMessage = res;

                 did.State = nameof(DidStateTypes.error);

                 await db.SaveChangesAsync();

                 return StatusCode(500, result);
             }

             did.State = nameof(DidStateTypes.tokencreated);
             await db.SaveChangesAsync();
            }

            return Ok();
        }

        private async Task<string> MintAndSendDidToken(EasynftprojectsContext db, Digitalidentity did, string metadata)
        {
            var be = await GlobalFunctions.CreateBurningAddressAsync(db, did.NftprojectId, DateTime.Now.AddMinutes(60), Blockchain.Cardano);

            if (be == null || string.IsNullOrEmpty(be.Address))
            {
                return "Could not create burning address";
            }

            string burningEndpoint = be.Address;

            Nft n = new()
            {
                NftprojectId = did.NftprojectId,
                Name = "Digital Identity",
                State = "reserved",
                Reservedcount = did.Nftproject.Maxsupply,
                Soldcount = 0,
                Errorcount = 0,
                Burncount = 0,
                Metadataoverride = metadata,
                Checkpolicyid = false,
                Uploadedtonftstorage = true,
                Isroyaltytoken = true,
                MetadatatemplateId = 1,
                Filename = "",
                Ipfshash = "",
                Filesize = 0,
                Minted = false, 
                Uid = Guid.NewGuid().ToString()
            };

            await db.Nfts.AddAsync(n);
            await db.SaveChangesAsync();


            var nft = await(from a in db.Nfts
               .Include(a => a.Nftproject)
               .ThenInclude(a => a.Customer)
               .AsSplitQuery()
               .Include(a => a.Nftproject)
               .ThenInclude(a => a.Settings)
               .AsSplitQuery()
                            where a.Id == n.Id
                            select a).FirstOrDefaultAsync();

            if (nft == null)
            {
                return "Token could not be created";
            }
            var paywallet = await GlobalFunctions.GetNmkrPaywalletAndBlockAsync(db,0,"CreateDIDTokenController",null);

            if (paywallet == null)
            {
                return "All pay wallets are busy in the moment. Please try again in a few seconds";
            }

            GetMetadataClass gmc = new(nft.Id, "",true);
            var check = nft.Name.Replace(" ", "");
            MintManuallyClass mmc1 = new()
            {
                BurnResult = true,
                Metadata = (await gmc.MetadataResultAsync()).Metadata,
                PolicyId = nft.Nftproject.Policyid,
                Prefix = "",
                ReceiverAddress = burningEndpoint,
                Tokenname = check,
                Projectid = nft.NftprojectId,
                SenderAddress = paywallet.Address,
                SenderSKey = Encryption.DecryptString(paywallet.Privateskey, GeneralConfigurationClass.Masterpassword + paywallet.Salt),
                SenderVKey = Encryption.DecryptString(paywallet.Privatevkey, GeneralConfigurationClass.Masterpassword + paywallet.Salt)
            };

            var s = ConsoleCommand.MintManually(db,_redis,nft.Nftproject, mmc1,  GlobalFunctions.IsMainnet(), 0, "", out var buildTransaction);
            await GlobalFunctions.LogMessageAsync(db, "MintAndSendCollectionToken " + paywallet.Address + " - " + s, buildTransaction.LogFile);


            if (s != "OK")
            {
                await GlobalFunctions.UnlockPaywalletAsync(db, paywallet);
                db.Nfts.Remove(nft);
                await db.SaveChangesAsync();
                return s;
            }
            else
            {
                nft.State = "sold";
                nft.Selldate = DateTime.Now;
                nft.Minted = true;
                nft.Receiveraddress = burningEndpoint;
                nft.Soldcount = 1;
                nft.Reservedcount = 0;
                nft.Errorcount = 0;
                await db.SaveChangesAsync();

                await GlobalFunctions.ExecuteSqlWithFallbackAsync(db, $"update adminmintandsendaddresses set addressblocked=1,blockcounter=0,lasttxhash='{buildTransaction.TxHash}', lasttxdate=NOW() where id='{paywallet.Id}'", 0);
                await GlobalFunctions.SaveTransactionAsync(db,_redis, buildTransaction, nft.Nftproject.CustomerId, nft.NftprojectId, nameof(TransactionTypes.mintfromcustomeraddress), null, nft.Id, 1, Coin.ADA);
                await GlobalFunctions.ReduceMintCouponsAsync(db, nft.Nftproject.CustomerId, 0.5f);

            }

            return "OK";
        }
    }
}
