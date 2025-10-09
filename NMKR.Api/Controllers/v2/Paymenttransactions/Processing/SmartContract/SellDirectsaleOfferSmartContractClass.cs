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
    /// Creates a buy smart contract transaction
    /// </summary>
    public class SellDirectsaleOfferSmartContractClass : ControllerBase, IProcessPaymentTransactionInterface
    {
        private readonly IConnectionMultiplexer _redis;

        public SellDirectsaleOfferSmartContractClass(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }
        private readonly long changeLockAmount = 1500000;

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
            SellerClass postparameter = postparameter1 as SellerClass;

            if (preparedtransaction.Transactiontype != nameof(PaymentTransactionTypes.smartcontract_directsale_offer))
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

            if (postparameter == null)
            {
                result.ErrorCode = 1256;
                result.ErrorMessage = "Wrong Postdata";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }


            if (postparameter.Seller == null)
            {
                result.ErrorCode = 1203;
                result.ErrorMessage = "Seller Object in JSON missing";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            if (!postparameter.Seller.Addresses.Any())
            {
                result.ErrorCode = 1205;
                result.ErrorMessage = "Missing Seller Address(es)";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            if (postparameter.Seller.ChangeAddress == null)
            {
                result.ErrorCode = 1207;
                result.ErrorMessage = "Seller change address missing";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            string SmartContractTxIn = preparedtransaction.Txinforalreadylockedtransactions;
            // Retrieve the TxHash und TxHash from the Transaction - if the Transaction is not verified already, we stop with an error

            if (string.IsNullOrEmpty(preparedtransaction.Txinforalreadylockedtransactions))
            {
                SmartContractTxIn = await StaticTransactionFunctions.GetSmartContractTxin(
                    preparedtransaction.PreparedpaymenttransactionsSmartcontractsjsons.LastOrDefault()?.Txid,
                    preparedtransaction.Smartcontracts.Address);
            }

            if (string.IsNullOrEmpty(SmartContractTxIn))
            {
                var last = preparedtransaction.PreparedpaymenttransactionsSmartcontractsjsons.LastOrDefault();
                if (last == null)
                {
                    result.ErrorCode = 1407;
                    result.ErrorMessage =
                        "Lock Transaction can not be found";
                    result.ResultState = ResultStates.Error;
                    return StatusCode(423, result);
                }
                if (last.Templatetype == nameof(DatumTemplateTypes.sell) &&
                    (preparedtransaction.Smartcontractstate == nameof(PaymentTransactionSubstates.waitingforsale) ||
                    preparedtransaction.Smartcontractstate == nameof(PaymentTransactionSubstates.readytosignbyseller)) &&
                    (last.Created < DateTime.Now.AddMinutes(-30) || last.Signedandsubmitted == false))
                {
                    db.PreparedpaymenttransactionsSmartcontractsjsons.Remove(last);
                    await db.SaveChangesAsync();
                    SmartContractTxIn = await StaticTransactionFunctions.GetSmartContractTxin(
                        preparedtransaction.PreparedpaymenttransactionsSmartcontractsjsons.Last().Txid,
                        preparedtransaction.Smartcontracts.Address);
                    preparedtransaction.Smartcontractstate = nameof(PaymentTransactionSubstates.waitingforsale);
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

            // Just for testing
            postparameter.Seller.CollateralTxIn = "";

            var tokensearch = preparedtransaction.PreparedpaymenttransactionsNfts.FirstOrDefault();
            if (tokensearch == null)
            {
                result.ErrorCode = 2707;
                result.ErrorMessage =
                    "No Token specified to buy";
                result.ResultState = ResultStates.Error;
                return StatusCode(423, result);
            }
            // TODO: decimals and multiplier
            var txin = StaticTransactionFunctions.GetAllNeededTxin(_redis, postparameter.Seller.Addresses,
                changeLockAmount,tokensearch.Count ,tokensearch.Policyid+"."+tokensearch.Tokennamehex,
                postparameter.Seller.CollateralTxIn, out string errormessage, out AllTxInAddressesClass alltxin);

            if (txin == null)
            {
                result.ErrorCode = 3201;
                result.ErrorMessage = "Not enough ADA found or ADA currently locked";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            string guid = GlobalFunctions.GetGuid();
            string matxrawfile = GeneralConfigurationClass.TempFilePath + "matx" + guid + ".raw";
            string protocolParamsFile = GeneralConfigurationClass.TempFilePath + @"protocol" + guid + ".params";
            string redeemerfile = GeneralConfigurationClass.TempFilePath + "redeemer" + guid + ".json";
            string scriptfile = GeneralConfigurationClass.TempFilePath + guid + preparedtransaction.Smartcontracts.Filename;
            string olddatumfile = GeneralConfigurationClass.TempFilePath + "olddatum" + guid + ".json";
            string costsfile = GeneralConfigurationClass.TempFilePath + "costs" + guid + ".json";

            // Write Plutus Script
            if (!string.IsNullOrEmpty(preparedtransaction.Smartcontracts.Plutus))
                await System.IO.File.WriteAllTextAsync(scriptfile, preparedtransaction.Smartcontracts.Plutus);


            PreparedpaymenttransactionsSmartcontractsjson ptsj = new()
            {
                PreparedpaymenttransactionsId = preparedtransaction.Id,
                Json = preparedtransaction.PreparedpaymenttransactionsSmartcontractsjsons.FirstOrDefault(x => x.Templatetype == nameof(DatumTemplateTypes.lockada))?.Json,
                Redeemer = StaticTransactionFunctions.FillJsonTemplateRedeemer(db, preparedtransaction, GlobalFunctions.GetPkhFromAddress(postparameter.Seller.ChangeAddress)),
                Templatetype = nameof(DatumTemplateTypes.sell),
                Hash = "",
            };
            if (string.IsNullOrEmpty(ptsj.Json))
            {
                result.ErrorCode = 1707;
                result.ErrorMessage =
                    "Datum not found";
                result.ResultState = ResultStates.Error;
                return StatusCode(500, result);
            }
            await System.IO.File.WriteAllTextAsync(olddatumfile, ptsj.Json);


            ConsoleCommand.GenerateProtocolParamsFile(protocolParamsFile, _redis, GlobalFunctions.IsMainnet(), out errormessage);
            BuildTransactionClass bt = new();

            var qt = ConsoleCommand.GetQueryTip();
            var l =qt.Slot;
            if (l != null)
            {
                long slot = (long)l;

                await System.IO.File.WriteAllTextAsync(redeemerfile, ptsj.Redeemer);

                SmartContractAuctionsParameterClass scapc = new()
                {
                    bidamount = preparedtransaction.Lovelace ?? 0, // The new offer for the bid in lovelace
                    scriptfile = scriptfile, // The Scriptfile - for action it is the auction.plutus
                    olddatumfile = olddatumfile, // The Old Datum hash from the further action
                    costsfile = costsfile,
                    utxoScript =
                        SmartContractTxIn, // The Tx-In of the Smart Contract - we have to get this from GetSmartContractsTxIn
                    scripthash =
                        preparedtransaction.Smartcontracts.Address, // Scripthash is the address of the smart contract
                    protocolParamsFile = protocolParamsFile,
                    collateraltxin = postparameter.Seller.CollateralTxIn, // The Collateral TX-In
                    changeaddress = postparameter.Seller.ChangeAddress, // Change Address for the rest of Lovelace
                    matxrawfile = matxrawfile,
                    utxopaymentaddress =
                        txin, // The needed TX-IN from  seller
                    scriptDatumHash = ptsj.Hash, // The new Datum hash from this action - so bid-n.json
                    startslot = slot,
                    next10slots = slot + 150,
                    redeemerfile = redeemerfile,
                    signerhash = GlobalFunctions.GetPkhFromAddress(postparameter.Seller.ChangeAddress),
                    tokencount = preparedtransaction.Tokencount,
                    policyidAndTokenname = preparedtransaction.Policyid + "." + preparedtransaction.Tokenname,
                    lockamount = (long)(preparedtransaction.Lockamount ?? 0),
                    smartcontractmemvalue = preparedtransaction.Smartcontracts.Memvalue,
                    smartcontracttimevalue = preparedtransaction.Smartcontracts.Timevalue
                };

                scapc.receiver.AddRange(StaticTransactionFunctions.GetReceiversToSmartContractsPayoutsClass(preparedtransaction));

                scapc.receiver.Add(new()
                {
                    address = postparameter.Seller.ChangeAddress,
                    lovelace = preparedtransaction.Lovelace - scapc.receiver.Sum(x=>x.lovelace) ?? 0,
                    receivertype = ReceiverTypes.seller
                });

                var ok = ConsoleCommand.SmartContractsDirectSaleOffer(_redis, scapc, GlobalFunctions.IsMainnet(), ref bt);


                // Last check if the transaction is still in the right state - mabye we have to check this with a mysql function
                if (ok)
                {
                    var prep1 = await (from a in db.Preparedpaymenttransactions
                                       where a.Id == preparedtransaction.Id
                                       select a).AsNoTracking().FirstOrDefaultAsync();
                    if (prep1.Smartcontractstate != nameof(PaymentTransactionSubstates.waitingforsale) && prep1.Smartcontractstate != nameof(PaymentTransactionSubstates.readytosignbyseller))
                    {
                        result.ErrorCode = 1310;
                        result.ErrorMessage = "Smartcontract is not any longer in the state of buying";
                        result.ResultState = ResultStates.Error;
                        return StatusCode(406, result);
                    }

                    preparedtransaction.Smartcontractstate = nameof(PaymentTransactionSubstates.readytosignbyseller);
                    await db.SaveChangesAsync();


                    string raw = await System.IO.File.ReadAllTextAsync(matxrawfile);
                    ptsj.Fee = bt.Fees;
                    ptsj.Bidamount = preparedtransaction.Lovelace ?? 0;
                    ptsj.Logfile = bt.LogFile;
                    ptsj.Rawtx = raw;
                    ptsj.Created = DateTime.Now;
                    ptsj.Signinguid = "S" + guid;
                    ptsj.Address = postparameter.Seller.ChangeAddress;

                    ptsj.Signedcbr = raw;



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
            }

            GlobalFunctions.DeleteFile(matxrawfile);
            GlobalFunctions.DeleteFile(protocolParamsFile);
            GlobalFunctions.DeleteFile(redeemerfile);
            GlobalFunctions.DeleteFile(scriptfile);
            GlobalFunctions.DeleteFile(olddatumfile);

            return Ok(StaticTransactionFunctions.GetTransactionState(db, _redis, preparedtransaction.Transactionuid, false));
        }

    }
}
