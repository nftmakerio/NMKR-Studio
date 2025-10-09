using CardanoSharp.Wallet.CIPs.CIP2.ChangeCreationStrategies;
using CardanoSharp.Wallet.CIPs.CIP2;
using NMKR.Shared.Classes.Cardano_Sharp;
using NMKR.Shared.Classes.CustodialWallets;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StackExchange.Redis;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading.Tasks;
using System;
using System.Linq;
using Asp.Versioning;
using Microsoft.EntityFrameworkCore;

namespace NMKR.Api.Controllers.v2.CustodialWallets
{
    [Route("v2/[controller]")]
    [ApiController]
    [ApiVersion("2")]
    public class SendAllAssetsController : ControllerBase
    {
        private readonly IConnectionMultiplexer _redis;

        public SendAllAssetsController(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }
        /// <summary>
        /// Send all ADA and all Tokens from a managed wallet to a receiver address
        /// </summary>
        /// <param Name="authorization">The apikey you have created on NMKR Studio</param>
        /// <response code="200">Returns the MakeTransactionResult Class</response>
        /// <response code="401">The access was denied. (Wrong or expired APIKEY, wrong projectid etc.)</response>     
        /// <response code="500">Internal server error - see the errormessage in the result</response>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(MakeTransactionResultClass))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ApiErrorResultClass))]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResultClass))]
        [HttpPost("{customerid}")]
        [MapToApiVersion("2")]
        [SwaggerOperation(
            Tags = new[] { "Managed Wallets" }
        )]
        public async Task<IActionResult> Post([OpenApiHeaderIgnore][FromHeader(Name = "authorization")] string apikey, int customerid,
             [FromBody] SendAllAssetsTransactionClass transaction)
        {
            // string apikey = Request.Headers["authorization"];
            if (!string.IsNullOrEmpty(apikey))
                apikey = apikey.Replace("Bearer ", "");

            var remoteIpAddress = HttpContext.Connection.RemoteIpAddress;

            string apifunction = this.GetType().Name;
            string apiparameter = customerid.ToString() + transaction.Senderaddress + transaction.Walletpassword + JsonConvert.SerializeObject(transaction);

            // Check if there is a cached (error) result
            var cachedResult = CheckCachedAccess.CheckCachedResult(_redis, apifunction, apikey, apiparameter);
            if (cachedResult != null)
            {
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

            if (string.IsNullOrEmpty(transaction.Walletpassword))
            {
                result.ErrorCode = 4439;
                result.ErrorMessage = "Walletpassword is empty";
                result.ResultState = ResultStates.Error;
                return StatusCode(500, result);
            }

            if (transaction.Walletpassword.Length < 6)
            {
                result.ErrorCode = 4440;
                result.ErrorMessage = "Walletpassword is too short. Minimum length is 6";
                result.ResultState = ResultStates.Error;
                return StatusCode(500, result);
            }

            if (transaction.Walletpassword.Length > 64)
            {
                result.ErrorCode = 4441;
                result.ErrorMessage = "Walletpassword is too long. Maximum length is 64";
                result.ResultState = ResultStates.Error;
                return StatusCode(500, result);
            }
            await using var db = new EasynftprojectsContext(GlobalFunctions.optionsBuilder.Options);

            var custodialwallet = await (from a in db.Custodialwallets
                                         where a.Address == transaction.Senderaddress
                                               && a.CustomerId == customerid
                                         select a).AsNoTracking().FirstOrDefaultAsync();

            if (custodialwallet == null)
            {
                result.ErrorCode = 30001;
                result.ErrorMessage = "Wallet not found";
                result.ResultState = ResultStates.Error;
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 404, result, apiparameter);
                return NotFound(result);
            }

            if (custodialwallet.State != "active")
            {
                result.ErrorCode = 30002;
                result.ErrorMessage = "Wallet is not in active state";
                result.ResultState = ResultStates.Error;
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 404, result, apiparameter);
                return StatusCode(403, result);
            }

            // Check for Pincode
            string salt = transaction.Walletpassword;
            string password = salt + GeneralConfigurationClass.Masterpassword;
            string skey = Encryption.DecryptString(custodialwallet.Skey, password);
            string vkey = Encryption.DecryptString(custodialwallet.Vkey, password);
            if (string.IsNullOrEmpty(skey))
            {
                result.ErrorCode = 30003;
                result.ErrorMessage = "Walletpassword is not correct";
                result.ResultState = ResultStates.Error;
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 404, result, apiparameter);
                return StatusCode(401, result);
            }

            if (string.IsNullOrEmpty(transaction.ReceiverAddress))
            {
                result.ErrorCode = 30004;
                result.ErrorMessage = "Receiveraddress must not be empty";
                result.ResultState = ResultStates.Error;
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 404, result, apiparameter);
                return StatusCode(406, result);
            }

            if (!ConsoleCommand.IsValidCardanoAddress(transaction.ReceiverAddress,GlobalFunctions.IsMainnet()))
            {
                result.ErrorCode = 30005;
                result.ErrorMessage = "Receiveraddress is not a valid Cardano address";
                result.ResultState = ResultStates.Error;
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 404, result, apiparameter);
                return StatusCode(406, result);
            }


            var utxo = await ConsoleCommand.GetNewUtxoAsync(transaction.Senderaddress);


            var cs = new CoinSelectionService(new LargestFirstStrategy(), new BasicChangeSelectionStrategy());
            MakeTransactionResultClass res = new() { Executed = DateTime.Now };

            try
            {
                CreateManagedWalletTransactionClass transactionx=transaction.ToCreateManagedWalletTransactionClass(utxo);

                var outputs = transactionx.ToTransactionOutput();
                var utxos = utxo.ToCardanosharpUtxos();
                var csCoinSelection = cs.GetCoinSelection(outputs, utxos,
                    transaction.Senderaddress);

                BuildTransactionClass bt = new BuildTransactionClass();

                await GlobalFunctions.LogMessageAsync(db, $"CustodialWallet {transaction.Senderaddress} Maketransaction",
                    JsonConvert.SerializeObject(transaction)); //+Environment.NewLine+ walletpassword);
                await GlobalFunctions.LogMessageAsync(db, $"CustodialWallet {transaction.Senderaddress} Utxo",
                    JsonConvert.SerializeObject(utxo));

                string ok = CardanoSharpFunctions.SendTransaction(db, _redis, csCoinSelection, transactionx, transaction.Senderaddress, skey, vkey,
                    GlobalFunctions.IsMainnet(), ref bt);
                if (ok != "OK")
                {
                    res.ErrorMessage = ok == "ERROR" ? "Submit failed" : ok;
                    res.State = MakeTransactionResults.error;
                }
                else
                {
                    res.State = MakeTransactionResults.success;
                    res.TxHash = bt.TxHash;
                    res.Fee = bt.Fees;
                }
            }
            catch (Exception e)
            {
                result.ErrorCode = 30006;
                result.ErrorMessage = e.Message;
                result.ResultState = ResultStates.Error;
                CheckCachedAccess.SetCachedResult(_redis, apifunction, apikey, 404, result, apiparameter);
                return StatusCode(409, result);
            }

            return Ok(res);
        }
    }
}
