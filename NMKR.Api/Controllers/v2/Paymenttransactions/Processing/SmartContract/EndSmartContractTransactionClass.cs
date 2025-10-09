using System;
using System.Linq;
using System.Threading.Tasks;
using NMKR.Api.Controllers.SharedClasses;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.Extensions;
using NMKR.Shared.Model;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.v2.Paymenttransactions.Processing.SmartContract
{
    /// <summary>
    /// End a smart contract transaction
    /// </summary>
    public class EndSmartContractTransactionClass : ControllerBase, IProcessPaymentTransactionInterface
    {
        private readonly IConnectionMultiplexer _redis;

        public EndSmartContractTransactionClass(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }
        /// <summary>
        /// End a smart contract transaction
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
            if (preparedtransaction.Expires == null || preparedtransaction.Expires > DateTime.Now)
            {
                result.ErrorCode = 1319;
                result.ErrorMessage = "Transaction can not ended yet. Too early. Expiration (UTC): " + preparedtransaction.Expires;
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
            string SmartContractTxIn = await StaticTransactionFunctions.GetSmartContractTxin(
                preparedtransaction.PreparedpaymenttransactionsSmartcontractsjsons.Last().Txid,
                preparedtransaction.Smartcontracts.Address);
            if (string.IsNullOrEmpty(SmartContractTxIn))
            {
                result.ErrorCode = 1207;
                result.ErrorMessage =
                    "Smart Contract has pending transactions or is not ready at the moment. Please try again later.";
                result.ResultState = ResultStates.Error;
                return StatusCode(423, result);
            }


            var txin = StaticTransactionFunctions.GetAllNeededTxin(_redis,
                new[] { preparedtransaction.Nftproject.Smartcontractssettings.Address }, 2000000, 0, null,
                preparedtransaction.Nftproject.Smartcontractssettings.Collateral, out string errormessage);

            PreparedpaymenttransactionsSmartcontractsjson ptsj = new()
            {
                PreparedpaymenttransactionsId = preparedtransaction.Id,
                Json = StaticTransactionFunctions.FillJsonTemplate(db, preparedtransaction, "close")
                    .FillJsonMarketplace(db, preparedtransaction.Nftproject.Smartcontractssettings),
                Redeemer = StaticTransactionFunctions.FillJsonRedeemer(db, preparedtransaction, "close")
                    .FillJsonMarketplace(db, preparedtransaction.Nftproject.Smartcontractssettings),
                Templatetype = nameof(DatumTemplateTypes.close),
            };
            ptsj.Hash = StaticTransactionFunctions.GetHash(ptsj.Json);


            string guid = GlobalFunctions.GetGuid();
            string matxrawfile = GeneralConfigurationClass.TempFilePath + "matx" + guid + ".raw";
            string matxsignedfile = GeneralConfigurationClass.TempFilePath + "matx" + guid + ".signed";
            string protocolParamsFile = GeneralConfigurationClass.TempFilePath + "protocol" + guid + ".params";
            string redeemerfile = GeneralConfigurationClass.TempFilePath + "redeemer" + guid + ".json";
            string scriptfile = GeneralConfigurationClass.TempFilePath + "" + guid + preparedtransaction.Smartcontracts.Filename;
            string olddatumfile = GeneralConfigurationClass.TempFilePath + "olddatum" + guid + ".json";
            string newdatumfile = GeneralConfigurationClass.TempFilePath + "newdatum" + guid + ".json";
            string marketplaceskeyfile = GeneralConfigurationClass.TempFilePath + "marketplace" + guid + ".skey";

            // Write Plutus Script
            if (!string.IsNullOrEmpty(preparedtransaction.Smartcontracts.Plutus))
                await System.IO.File.WriteAllTextAsync(scriptfile, preparedtransaction.Smartcontracts.Plutus);

            // Write Old Datum file - the last used JSON File
            var olddatum = preparedtransaction.PreparedpaymenttransactionsSmartcontractsjsons.LastOrDefault();
            if (olddatum != null)
                await System.IO.File.WriteAllTextAsync(olddatumfile, olddatum.Json);

            var newdatum = ptsj;
            if (newdatum != null)
                await System.IO.File.WriteAllTextAsync(newdatumfile, newdatum.Json);

            ConsoleCommand.GenerateProtocolParamsFile(protocolParamsFile, _redis,GlobalFunctions.IsMainnet(), out errormessage);
            BuildTransactionClass bt = new();

            var qt = ConsoleCommand.GetQueryTip();
            long slot = qt.Slot ?? 0;

            await System.IO.File.WriteAllTextAsync(redeemerfile, ptsj.Redeemer);


            SmartContractAuctionsParameterClass scapc = new()
            {
                bidamount = 2000000, // The new offer for the bid in lovelace
                scriptfile = scriptfile, // The Scriptfile - for action it is the auction.plutus
                olddatumfile = olddatumfile, // The Old Datum hash from the further action
                utxoScript =
                    SmartContractTxIn, // The Tx-In of the Smart Contract - we have to get this from GetSmartContractsTxIn
                scripthash =
                    preparedtransaction.Smartcontracts.Address, // Scripthash is the address of the smart contract
                protocolParamsFile = protocolParamsFile,
                collateraltxin =
                    preparedtransaction.Nftproject.Smartcontractssettings.Collateral, // The Collateral TX-In
                changeaddress =
                    preparedtransaction.Nftproject.Smartcontractssettings
                        .Address, // Change Address for the rest of Lovelace
                matxrawfile = matxrawfile,
                utxopaymentaddress =
                    txin, // The needed TX-IN from the Bidder - here must be min. the adaamount what he is bidding
                scriptDatumHash = "", // The new Datum hash from this action - so bid-n.json
                newdatumfile = "",
                startslot = slot,
                next10slots = slot + 150,
                redeemerfile = redeemerfile,
                signerhash = preparedtransaction.Nftproject.Smartcontractssettings.Pkh,
                tokencount = preparedtransaction.Tokencount,
                policyidAndTokenname = preparedtransaction.Policyid + "." + preparedtransaction.Tokenname,

            };

            // If there was one or more bids, close it with fees for the marketplace and royalty
            if (olddatum.Templatetype == nameof(DatumTemplateTypes.bet))
            {
                foreach (var smartcontractOutput in preparedtransaction.PreparedpaymenttransactionsSmartcontractOutputs)
                {
                    scapc.receiver.Add(new()
                    {
                        address = smartcontractOutput.Address,
                        lovelace = smartcontractOutput.Lovelace,
                        receivertype = smartcontractOutput.Type.ToEnum<ReceiverTypes>()
                    });
                }
            }
            

            // If there was not bid - just close is without any fees
            if (olddatum.Templatetype == nameof(DatumTemplateTypes.locknft))
            {
                scapc.receiver.Add(new()
                {
                    address = preparedtransaction.Selleraddress,
                    lovelace = 2000000,
                    tokens = preparedtransaction.Tokencount + " " + preparedtransaction.Policyid + "." +
                             preparedtransaction.Tokenname,
                    receivertype = ReceiverTypes.seller
                });
            }

            var ok = ConsoleCommand.SmartContractsAuctionTransactionClose(_redis, scapc, GlobalFunctions.IsMainnet(),
                ref bt);

            if (ok)
            {
                string payskey = Encryption.DecryptString(preparedtransaction.Nftproject.Smartcontractssettings.Skey,
                    preparedtransaction.Nftproject.Smartcontractssettings.Salt + GeneralConfigurationClass.Masterpassword);
                await System.IO.File.WriteAllTextAsync(marketplaceskeyfile, payskey);
                ok = ConsoleCommand.SignTransaction(new[] {marketplaceskeyfile}, "", matxrawfile, matxsignedfile,
                    GlobalFunctions.IsMainnet(), ref bt);
            }

            if (ok)
            {
                var ok2 = ConsoleCommand.SubmitTransaction(matxsignedfile, GlobalFunctions.IsMainnet(), ref bt);
                string raw = await System.IO.File.ReadAllTextAsync(matxrawfile);
                ptsj.Fee = bt.Fees;
                ptsj.Logfile = bt.LogFile;
                ptsj.Rawtx = raw;
                ptsj.Created = DateTime.Now;
                ptsj.Txid = bt.TxHash;

                // Save new Json to Database
                db.PreparedpaymenttransactionsSmartcontractsjsons.Add(ptsj);
                await db.SaveChangesAsync();
                ok = ok2.Success;
            }

            if (ok)
            {
                ok = ConsoleCommand.GetTxId(matxsignedfile, ref bt);
            }

            if (ok)
            {
                preparedtransaction.Logfile = bt.LogFile;
                preparedtransaction.State = nameof(PaymentTransactionsStates.finished);
                await db.SaveChangesAsync();
            }

            if (!ok)
            {
                result.ErrorCode = 1118;
                result.ErrorMessage = "Can not close the smart contract";
                result.InnerErrorMessage = bt.LogFile;
                result.ResultState = ResultStates.Error;
                return StatusCode(500, result);
            }


            GlobalFunctions.DeleteFile(marketplaceskeyfile);
            GlobalFunctions.DeleteFile(matxrawfile);
            GlobalFunctions.DeleteFile(matxsignedfile);
            GlobalFunctions.DeleteFile(protocolParamsFile);
            GlobalFunctions.DeleteFile(redeemerfile);
            GlobalFunctions.DeleteFile(scriptfile);
            GlobalFunctions.DeleteFile(olddatumfile);
            GlobalFunctions.DeleteFile(newdatumfile);


            return Ok(StaticTransactionFunctions.GetTransactionState(db, _redis, preparedtransaction.Transactionuid, true));
        }
    }
}
