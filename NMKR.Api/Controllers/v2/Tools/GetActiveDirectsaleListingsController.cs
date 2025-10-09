using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Asp.Versioning;
using NMKR.Shared.Classes;
using NMKR.Shared.Classes.Koios;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.Blockfrost;
using NMKR.Shared.Functions.Koios;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;

namespace NMKR.Api.Controllers.v2.Tools
{
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class GetActiveDirectsaleListingsController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        /// <summary>
        /// Returns all active listings in smartcontract from a given stakeaddress - Constructor
        /// </summary>
        /// <param name="redis"></param>
        public GetActiveDirectsaleListingsController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        [HttpGet("{stakeaddress}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] {"Tools"}
        )]
        public async Task<IActionResult> Get([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, string stakeaddress)
        {
           // string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            string apifunction = this.GetType().Name;
            string apiparameter = stakeaddress;

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
                if (cachedResult.Statuscode == 200)
                    return Ok(JsonConvert.DeserializeObject<GetActiveListingsClass[]>(cachedResult.ResultString));
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


            // Check if stakeaddress is valid

            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);
            var smartcontracts=await (from a in db.Smartcontracts
                                      where a.Type=="directsaleV2" && a.State=="active"
                                      select a).AsNoTracking().ToListAsync();


            List<GetActiveListingsClass> listings = new List<GetActiveListingsClass>();
            // Get all addresses for stakeadress
            var addresses=await BlockfrostFunctions.GetAllAddressesWithThisStakeAddressAsync(_redis,stakeaddress);


            // Get all transactions to smartcontracts
            KoiosGetTransactionsAddressesClass adr = new KoiosGetTransactionsAddressesClass()
                {Addresses =(from a in addresses
                             select a.Address).ToArray(), AfterBlockHeight = 0};
            var txs=await KoiosFunctions.GetAllTransactionsForSpecificAddressesAsync(adr);

            // Check all transactions if they are still on the smartcontract
            foreach (var tx in txs)
            {

               // var transaction = await KoiosFunctions.GetTransactionInformationAsync(tx.TxHash);
               var transaction = await GetTransactionInformation(db,tx.TxHash);
                if (transaction == null || !transaction.Any())
                    continue;

                var txinfo = transaction.First();
                foreach (var a in txinfo.Outputs)
                {
                    var f = smartcontracts.Find(x => x.Address == a.PaymentAddr.Bech32);
                    if (f == null)
                        continue;

                    // Check if the asset is still on the smartcontract

                    foreach (var b in a.AssetList)
                    {
                        if (await IsAssetStillInSmartcontracrt(f.Address, b.PolicyId, b.AssetName))
                        {
                            listings.Add(new GetActiveListingsClass()
                            {
                                PolicyId = b.PolicyId,
                                Fingerprint = b.Fingerprint,
                                AssetNameInHex = b.AssetName,
                                SmartcontractName = f.Smartcontractname,
                                SmartcontractAddress=f.Address,
                                TxHashAndId = txinfo.TxHash + "#0",
                                //Buylink = f.Buylink,
                            });
                        }
                    }
                }
            }

            CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 200, listings, apiparameter);
            return Ok(JsonConvert.SerializeObject(listings));

        }

        private async Task<KoiosTransactionClass[]> GetTransactionInformation(EasynftprojectsContext db, string txTxHash)
        {
            var txhash = await (from a in db.Txhashcaches
                where a.Txhash == txTxHash
                select a).AsNoTracking().FirstOrDefaultAsync();
            if (txhash != null)
            {
                return JsonConvert.DeserializeObject<KoiosTransactionClass[]>(txhash.Transactionobject);
            }
            var transaction = await KoiosFunctions.GetTransactionInformationAsync(txTxHash);
            if (transaction != null)
            {
                await db.Txhashcaches.AddAsync(new Txhashcache()
                {
                    Created = DateTime.Now, Txhash = txTxHash,
                    Transactionobject = JsonConvert.SerializeObject(transaction)
                });
                await db.SaveChangesAsync();
            }

            return transaction;
        }

        private async Task<bool> IsAssetStillInSmartcontracrt(string smartcontracrtaddress, string policyid, string assetnameinhex)
        {
            var nft = await KoiosFunctions.GetNftAddressAsync(policyid, assetnameinhex);
            if (nft == null || !nft.Any())
                return false;

            return nft.First().PaymentAddress == smartcontracrtaddress;
        }
    }
}
