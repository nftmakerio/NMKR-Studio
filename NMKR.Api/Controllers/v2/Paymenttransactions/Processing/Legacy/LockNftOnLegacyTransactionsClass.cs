using System;
using System.Linq;
using System.Threading.Tasks;
using NMKR.Api.Controllers.SharedClasses;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.v2.Paymenttransactions.Processing.Legacy
{
    /// <summary>
    /// Locks an nft on a legacy transaction (auction or directsale)
    /// </summary>
    public class LockNftOnLegacyTransactionsClass : ControllerBase, IProcessPaymentTransactionInterface
    {
        private readonly IConnectionMultiplexer _redis;

        public LockNftOnLegacyTransactionsClass(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }
        /// <summary>
        /// Locks an nft on a legacy transaction (auction or directsale)
        /// </summary>
        /// <param name="db"></param>
        /// <param name="apikey"></param>
        /// <param name="remoteipaddress"></param>
        /// <param name="result"></param>
        /// <param name="preparedtransaction"></param>
        /// <param name="postparameter1"></param>
        /// <returns></returns>
        public async Task<IActionResult> ProcessTransaction(EasynftprojectsContext db, string apikey, string remoteipaddress, ApiErrorResultClass result,
            Preparedpaymenttransaction preparedtransaction, object postparameter1)
        {
            var seller = postparameter1 as SellerClass;

            if (preparedtransaction.State == nameof(PaymentTransactionsStates.active) && preparedtransaction.Smartcontractstate == nameof(PaymentTransactionSubstates.readytosignbyseller))
            {
                return Ok(StaticTransactionFunctions.GetTransactionState(db, _redis, preparedtransaction.Transactionuid, false));
            }

            if (preparedtransaction.State != nameof(PaymentTransactionsStates.prepared) && preparedtransaction.Smartcontractstate != nameof(PaymentTransactionSubstates.waitingforlocknft))
            {
                result.ErrorCode = 1117;
                result.ErrorMessage = "State of this Transaction does not fit to this command";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            string tokenname = preparedtransaction.Policyid.ToLower() + "." + preparedtransaction.Tokenname.ToLower();

            BuildTransactionClass bt = new();
            string guid = GlobalFunctions.GetGuid();
            string matxrawfile = GeneralConfigurationClass.TempFilePath + "matx" + guid + ".raw";
            string protocolParamsFile = GeneralConfigurationClass.TempFilePath + "protocol" + guid + ".params";

            preparedtransaction.Sellerpkh = seller.Seller.Pkh;
            preparedtransaction.Selleraddresses = String.Join("/", seller.Seller.Addresses.Select(x=>x.Address).ToArray()); // This are all addresses from the seller - if he have more than one - to find the right tx in for the token
            preparedtransaction.Selleraddress =
                seller.Seller.Addresses.Select(x => x.Address).FirstOrDefault(); // This is the first Seller Address to send the ada when the smart contract is over
            preparedtransaction.Changeaddress = seller.Seller.ChangeAddress;


            string legacyaddress = "";
            if (preparedtransaction.Transactiontype == nameof(PaymentTransactionTypes.legacy_auction))
            {
                preparedtransaction.Legacyauctions.Selleraddress = seller.Seller.ChangeAddress;
                legacyaddress = preparedtransaction.Legacyauctions.Address;
            }
            if (preparedtransaction.Transactiontype == nameof(PaymentTransactionTypes.legacy_directsale))
            {
                preparedtransaction.Legacydirectsales.Selleraddress = seller.Seller.ChangeAddress;
                legacyaddress = preparedtransaction.Legacydirectsales.Address;
            }

            await db.SaveChangesAsync();


            var utxofinal = StaticTransactionFunctions.GetAllNeededTxin(_redis,preparedtransaction.Selleraddresses.Split("/"),
                4500000, (long)(preparedtransaction.Tokencount ?? 1), tokenname, seller.Seller.CollateralTxIn, out string errormessage);
            if (utxofinal == null)
            {
                result.ErrorCode = 1105;
                result.ErrorMessage = errormessage;
                result.ResultState = ResultStates.Error;
                return StatusCode(404, result);
            }

            ConsoleCommand.GenerateProtocolParamsFile(protocolParamsFile, _redis,GlobalFunctions.IsMainnet(), out errormessage);
            var ok = ConsoleCommand.LegacyTransactionLockNft(_redis,utxofinal, preparedtransaction.Changeaddress, (long)preparedtransaction.Tokencount,
                tokenname, legacyaddress,
                protocolParamsFile, matxrawfile, GlobalFunctions.IsMainnet(), ref bt);

            if (ok)
            {
                string raw = await System.IO.File.ReadAllTextAsync(matxrawfile);
                preparedtransaction.Estimatedfees = bt.Fees;

                PreparedpaymenttransactionsSmartcontractsjson ptsj = new()
                {
                    PreparedpaymenttransactionsId = preparedtransaction.Id,
                    Json = "",
                    Templatetype = nameof(DatumTemplateTypes.locknft),
                    Hash = "",
                    Rawtx = raw,
                    Created = DateTime.Now,
                    Fee = bt.Fees,
                    Logfile = bt.LogFile,
                    Signinguid = "Q" + guid,
                    Signedcbr = ConsoleCommand.SignTx(matxrawfile,
                        preparedtransaction.Nftproject.Smartcontractssettings.Fakesignskey),
                };
                db.PreparedpaymenttransactionsSmartcontractsjsons.Add(ptsj);
                await db.SaveChangesAsync();

                preparedtransaction.Smartcontractstate = nameof(PaymentTransactionSubstates.readytosignbyseller);
                preparedtransaction.State = nameof(PaymentTransactionsStates.active);
                await db.SaveChangesAsync();
            }
            else
            {
                result.ErrorCode = 1118;
                result.ErrorMessage = "Can not create Cbor. Please contact support.";
                result.ResultState = ResultStates.Error;
                return StatusCode(500, result);
            }

            GlobalFunctions.DeleteFile(matxrawfile);
            GlobalFunctions.DeleteFile(protocolParamsFile);

            return Ok(StaticTransactionFunctions.GetTransactionState(db, _redis, preparedtransaction.Transactionuid, false));
        }

    }
}
