using System;
using System.Linq;
using System.Threading.Tasks;
using NMKR.Api.Controllers.SharedClasses;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.v2.Paymenttransactions.Processing.SmartContract
{
    /// <summary>
    /// Creates a buy smart contract transaction
    /// </summary>
    public class GetBuyOutSmartcontractAddress : ControllerBase, IProcessPaymentTransactionInterface
    {
        private readonly IConnectionMultiplexer _redis;

        public GetBuyOutSmartcontractAddress(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        /// <summary>
        /// Creates a buy smart contract transaction
        /// </summary>
        /// <param name="db"></param>
        /// <param name="apikey"></param>
        /// <param name="result"></param>
        /// <param name="preparedtransaction"></param>
        /// <param name="postparameter1"></param>
        /// <returns></returns>
        public async Task<IActionResult> ProcessTransaction(EasynftprojectsContext db, string apikey, string remoteipaddress, ApiErrorResultClass result,
            Preparedpaymenttransaction preparedtransaction, object postparameter1)
        {

            if (preparedtransaction.Transactiontype != nameof(PaymentTransactionTypes.smartcontract_directsale))
            {
                result.ErrorCode = 1102;
                result.ErrorMessage = "Command does not fit to this transaction";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            if (preparedtransaction.State != nameof(PaymentTransactionsStates.active))
            {
                result.ErrorCode = 1209;
                result.ErrorMessage = "Transaction is not in active state";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            // Retrieve the TxHash und TxHash from the Transaction - if the Transaction is not verified already, we stop with an error

            if (preparedtransaction.Smartcontractstate!=nameof(PaymentTransactionSubstates.waitingforsale))
            {
                result.ErrorCode = 1209;
                result.ErrorMessage = "Smartcontract state is not in waitingforsale state";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }





            var cn = ConsoleCommand.CreateNewPaymentAddress(GlobalFunctions.IsMainnet());
            if (cn.ErrorCode != 0)
            {
                result.ErrorCode = cn.ErrorCode;
                result.ErrorMessage = cn.ErrorMessage;
                result.ResultState = ResultStates.Error;
                return StatusCode(500, result);
            }

            CryptographyProcessor cp = new();
            string salt = cp.CreateSalt(30);
            string password = salt + GeneralConfigurationClass.Masterpassword;

            Buyoutsmartcontractaddress bsca = new Buyoutsmartcontractaddress()
            {
                Transactionid = preparedtransaction.Transactionuid,
                Expiredate = DateTime.Now.AddMinutes(30),
                Lovelace = (preparedtransaction.Lovelace??0) + (preparedtransaction.Lockamount??0),
                Lockamount = preparedtransaction.Lockamount ?? 0,
                Additionalamount = 2000000,
                Smartcontracttxhash = preparedtransaction.Txinforalreadylockedtransactions ?? preparedtransaction.PreparedpaymenttransactionsSmartcontractsjsons.Last(x => x.Templatetype=="locknft").Txid+"#0",
                State = "active",
                Address = cn.Address,
                Skey = Encryption.EncryptString(cn.privateskey, password),
                Vkey = Encryption.EncryptString(cn.privatevkey, password),
                CustomerId = preparedtransaction.Nftproject.CustomerId,
                Salt = salt,
            };
            await db.Buyoutsmartcontractaddresses.AddAsync(bsca);
            await db.SaveChangesAsync();

            foreach (var smartcontractDirectsaleReceiverClass in preparedtransaction.PreparedpaymenttransactionsSmartcontractOutputs)
            {
                BuyoutsmartcontractaddressesReceiver bscar = new BuyoutsmartcontractaddressesReceiver()
                {
                    Lovelace = smartcontractDirectsaleReceiverClass.Lovelace,
                    Receiveraddress = smartcontractDirectsaleReceiverClass.Address,
                    BuyoutsmartcontractaddressesId = bsca.Id,
                    Pkh = smartcontractDirectsaleReceiverClass.Pkh,
                };
                await db.BuyoutsmartcontractaddressesReceivers.AddAsync(bscar);
                await db.SaveChangesAsync();
            }
           
            preparedtransaction.BuyoutaddressesId=bsca.Id;
            await db.SaveChangesAsync();


            return Ok(StaticTransactionFunctions.GetTransactionState(db,_redis, preparedtransaction.Transactionuid, false));
        }

    }
}
