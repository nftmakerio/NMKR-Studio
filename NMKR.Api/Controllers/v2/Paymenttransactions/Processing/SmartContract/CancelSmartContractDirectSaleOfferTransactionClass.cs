using System;
using System.Linq;
using System.Threading.Tasks;
using NMKR.Api.Controllers.SharedClasses;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.v2.Paymenttransactions.Processing.SmartContract
{
    /// <summary>
    /// Cancels the Smart Contract Direct Sale Transaction
    /// </summary>
    public class CancelSmartContractDirectSaleOfferTransactionClass : ControllerBase, IProcessPaymentTransactionInterface
    {
        private readonly IConnectionMultiplexer _redis;

        public CancelSmartContractDirectSaleOfferTransactionClass(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }
        /// <summary>
        /// Cancels the Smart Contract Direct Sale Transaction
        /// </summary>
        /// <param name="db"></param>
        /// <param name="apikey"></param>
        /// <param name="result"></param>
        /// <param name="preparedtransaction"></param>
        /// <param name="postparameter1"></param>
        /// <returns></returns>
        public async Task<IActionResult> ProcessTransaction(EasynftprojectsContext db, string apikey, string remoteipaddress, ApiErrorResultClass result, Preparedpaymenttransaction preparedtransaction, object postparameter1)
        {
            BuyerClass postparameter = postparameter1 as BuyerClass;
            if (preparedtransaction.Transactiontype != nameof(PaymentTransactionTypes.smartcontract_directsale_offer))
            {
                result.ErrorCode = 1102;
                result.ErrorMessage = "Command does not fit to this transaction";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            if (postparameter == null)
            {
                result.ErrorCode = 1266;
                result.ErrorMessage = "You must submit the Buyerclass (POST Call)";
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

            if (preparedtransaction.Smartcontractstate != nameof(PaymentTransactionSubstates.waitingforsale) &&
                preparedtransaction.Smartcontractstate != nameof(PaymentTransactionSubstates.readytosignbybuyercancel))
            {
                result.ErrorCode = 1306;
                result.ErrorMessage = "Smartcontract is not in the state of sale - cancel not possible";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            if (!await StaticTransactionFunctions.CheckForCorrectWalletCancel(db, preparedtransaction, postparameter))
            {
                result.ErrorCode = 1399;
                result.ErrorMessage = "Wallet is not allowed to cancel this transaction";
                result.ResultState = ResultStates.Error;
                return StatusCode(403, result);
            }



            // Retrieve the TxHash und TxHash from the Transaction - if the Transaction is not verified already, we stop with an error
            string SmartContractTxIn = await StaticTransactionFunctions.GetSmartContractTxin(
                preparedtransaction.PreparedpaymenttransactionsSmartcontractsjsons.Last().Txid,
                preparedtransaction.Smartcontracts.Address);
            if (string.IsNullOrEmpty(SmartContractTxIn))
            {
                var last = preparedtransaction.PreparedpaymenttransactionsSmartcontractsjsons.Last();
                if (last.Templatetype == nameof(DatumTemplateTypes.cancel) &&
                    preparedtransaction.Smartcontractstate == nameof(PaymentTransactionSubstates.readytosignbybuyercancel) &&
                    (last.Submitted == null || last.Submitted < DateTime.Now.AddMinutes(-30)))
                {
                    db.PreparedpaymenttransactionsSmartcontractsjsons.Remove(last);
                    preparedtransaction.Smartcontractstate = nameof(PaymentTransactionSubstates.waitingforsale);
                    await db.SaveChangesAsync();
                    SmartContractTxIn = await StaticTransactionFunctions.GetSmartContractTxin(
                        preparedtransaction.PreparedpaymenttransactionsSmartcontractsjsons.Last().Txid,
                        preparedtransaction.Smartcontracts.Address);
                }
                else
                {
                    result.ErrorCode = 1207;
                    result.ErrorMessage =
                        "Smart Contract has pending transactions or is not ready at the moment. Please try again later.";
                    result.ResultState = ResultStates.Error;
                    return StatusCode(423, result);
                }
            }

            var txin = StaticTransactionFunctions.GetAllNeededTxin(_redis, postparameter.Buyer.Addresses,
          2000000, 0, null, postparameter.Buyer.CollateralTxIn, out string errormessage, out AllTxInAddressesClass alltxin);

            PreparedpaymenttransactionsSmartcontractsjson ptsj = new()
            {
                PreparedpaymenttransactionsId = preparedtransaction.Id,
                Json = StaticTransactionFunctions.FillJsonTemplateBuyerDirectsaleOffer(preparedtransaction),
                Redeemer = StaticTransactionFunctions.FillJsonTemplateCancelRedeemer(db, preparedtransaction, postparameter.Buyer.Pkh),
                Templatetype = "cancel",
            };
            ptsj.Hash = StaticTransactionFunctions.GetHash(ptsj.Json);


            string guid = GlobalFunctions.GetGuid();
            string matxrawfile = GeneralConfigurationClass.TempFilePath + guid + ".raw";
            string protocolParamsFile = GeneralConfigurationClass.TempFilePath + "protocol" + guid + ".params";
            string redeemerfile = GeneralConfigurationClass.TempFilePath + "redeemer" + guid + ".json";
            string scriptfile = GeneralConfigurationClass.TempFilePath + guid + preparedtransaction.Smartcontracts.Filename;
            string olddatumfile = GeneralConfigurationClass.TempFilePath + "olddatum" + guid + ".json";

            // Write Plutus Script
            if (!string.IsNullOrEmpty(preparedtransaction.Smartcontracts.Plutus))
                await System.IO.File.WriteAllTextAsync(scriptfile, preparedtransaction.Smartcontracts.Plutus);

            // Write Old Datum file - the last used JSON File
            var olddatum = preparedtransaction.PreparedpaymenttransactionsSmartcontractsjsons.LastOrDefault();
            if (olddatum != null)
                await System.IO.File.WriteAllTextAsync(olddatumfile, olddatum.Json);

            ConsoleCommand.GenerateProtocolParamsFile(protocolParamsFile, _redis, GlobalFunctions.IsMainnet(), out errormessage);
            BuildTransactionClass bt = new();

            await System.IO.File.WriteAllTextAsync(redeemerfile, ptsj.Redeemer);

            SmartContractAuctionsParameterClass scapc = new()
            {
                bidamount = postparameter.BuyerOffer, // The new offer for the bid in lovelace
                scriptfile = scriptfile, // The Scriptfile - for action it is the auction.plutus
                olddatumfile = olddatumfile, // The Old Datum hash from the further action
                utxoScript =
                    SmartContractTxIn, // The Tx-In of the Smart Contract - we have to get this from GetSmartContractsTxIn
                scripthash =
                    preparedtransaction.Smartcontracts.Address, // Scripthash is the address of the smart contract
                protocolParamsFile = protocolParamsFile,
                collateraltxin = postparameter.Buyer.CollateralTxIn, // The Collateral TX-In
                changeaddress = postparameter.Buyer.ChangeAddress, // Change Address for the rest of Lovelace
                matxrawfile = matxrawfile,
                utxopaymentaddress =
                    txin, // The needed TX-IN from the Bidder - here must be min. the adaamount what he is bidding
                scriptDatumHash = ptsj.Hash, // The new Datum hash from this action - so bid-n.json
                redeemerfile = redeemerfile,
                signerhash = preparedtransaction.Buyerpkh,
                tokencount = preparedtransaction.Tokencount,
                policyidAndTokenname = preparedtransaction.Policyid + "." + preparedtransaction.Tokenname,
            /*    smartcontractmemvalue = preparedtransaction.Smartcontracts.Memvalue,
                smartcontracttimevalue = preparedtransaction.Smartcontracts.Timevalue*/
            };
            scapc.receiver.Add(new()
            {
                address = preparedtransaction.Buyeraddresses,
                lovelace = preparedtransaction.Lovelace ?? 2000000,
                tokens = "",
            });

            var ok = ConsoleCommand.SmartContractsDirectSaleOffer(_redis, scapc, GlobalFunctions.IsMainnet(), ref bt);


            // Last check if the transaction is still in the right state - mabye we have to check this with a mysql function
            if (ok)
            {
                var prep1 = await (from a in db.Preparedpaymenttransactions
                                   where a.Id == preparedtransaction.Id
                                   select a).AsNoTracking().FirstOrDefaultAsync();
                if (prep1.Smartcontractstate != nameof(PaymentTransactionSubstates.waitingforsale))
                {
                    result.ErrorCode = 1307;
                    result.ErrorMessage = "Smartcontract is not any longer in the state of canceling";
                    result.ResultState = ResultStates.Error;
                    return StatusCode(406, result);
                }

                preparedtransaction.Smartcontractstate = nameof(PaymentTransactionSubstates.readytosignbybuyercancel);
                await db.SaveChangesAsync();
            }

            if (ok)
            {
                string raw = await System.IO.File.ReadAllTextAsync(matxrawfile);
                ptsj.Fee = bt.Fees;
                ptsj.Bidamount = postparameter.BuyerOffer;
                ptsj.Logfile = bt.LogFile;
                ptsj.Rawtx = raw;
                ptsj.Created = DateTime.Now;
                ptsj.Signinguid = "C" + guid;
                ptsj.Address = postparameter.Buyer.ChangeAddress;
                // We need to "fake" sign the cbor - because of an error in the serialisatzon lib. Nami can not sign it, without the signing space - so we sign it and nami replaces the signature
                await StaticTransactionFunctions.FakeSign(db, preparedtransaction);

                ptsj.Signedcbr = ConsoleCommand.SignTx(matxrawfile,
                    preparedtransaction.Nftproject.Smartcontractssettings.Fakesignskey);
                ptsj.Txid = bt.TxHash;

                // Save new Json to Database
                db.PreparedpaymenttransactionsSmartcontractsjsons.Add(ptsj);
                await db.SaveChangesAsync();
            }
            else
            {
                result.ErrorCode = 1118;
                result.ErrorMessage = "Can not create Cbor. Please contact support.";
                result.ResultState = ResultStates.Error;
                result.InnerErrorMessage = bt.LogFile;
                return StatusCode(500, result);
            }

            GlobalFunctions.DeleteFile(matxrawfile);
            GlobalFunctions.DeleteFile(protocolParamsFile);
            GlobalFunctions.DeleteFile(redeemerfile);
            GlobalFunctions.DeleteFile(scriptfile);
            GlobalFunctions.DeleteFile(olddatumfile);

            return Ok(StaticTransactionFunctions.GetTransactionState(db,_redis, preparedtransaction.Transactionuid, true,true));
        }

    }
}
