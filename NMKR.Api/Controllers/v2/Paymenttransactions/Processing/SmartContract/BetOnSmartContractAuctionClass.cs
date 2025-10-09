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
    /// Bets on a smart contract auction
    /// </summary>
    public class BetOnSmartContractAuctionClass : ControllerBase, IProcessPaymentTransactionInterface
    {
        private readonly IConnectionMultiplexer _redis;

        public BetOnSmartContractAuctionClass(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }
        private readonly long changeLockAmount = 3000000;

        /// <summary>
        /// Bets on a smart contract auction
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
            BuyerClass postparameter = postparameter1 as BuyerClass;
            if (preparedtransaction.State != nameof(PaymentTransactionsStates.active))
            {
                result.ErrorCode = 1209;
                result.ErrorMessage = "Transaction is not in active state";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            if (preparedtransaction.Transactiontype != nameof(PaymentTransactionTypes.smartcontract_auction))
            {
                result.ErrorCode = 1102;
                result.ErrorMessage = "Command does not fit to this transaction";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            var lastbid = preparedtransaction.PreparedpaymenttransactionsSmartcontractsjsons.LastOrDefault();
            if (preparedtransaction.Smartcontractstate != nameof(PaymentTransactionSubstates.waitingforbid))
            {
                result.ErrorCode = 1315;
                result.ErrorMessage = "Smartcontract is not in the state of bidding";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            if (preparedtransaction.Expires < DateTime.Now)
            {
                result.ErrorCode = 1208;
                result.ErrorMessage = "Auction is ended. Bid is too late";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            if (preparedtransaction.Auctionminprice != null && postparameter.BuyerOffer < preparedtransaction.Auctionminprice)
            {
                result.ErrorCode = 1201;
                result.ErrorMessage = "Bid must be higher than min. price";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            if (lastbid != null && lastbid.Bidamount != null && lastbid.Bidamount >= postparameter.BuyerOffer)
            {
                result.ErrorCode = 1202;
                result.ErrorMessage = "There is a higher bid from an other bidder";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }
            if (postparameter==null)
            {
                result.ErrorCode = 1502;
                result.ErrorMessage = "No valid buyerclass provided";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            if (postparameter.BuyerOffer < 5000000)
            {
                result.ErrorCode = 1202;
                result.ErrorMessage = "The mimimum bid is 5 ADA";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }
            if (postparameter.Buyer == null)
            {
                result.ErrorCode = 1203;
                result.ErrorMessage = "Buyer Object in JSON missing";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            if (!postparameter.Buyer.Addresses.Any())
            {
                result.ErrorCode = 1205;
                result.ErrorMessage = "Missing Buyer Address(es)";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }
            if (postparameter.Buyer.ChangeAddress == null)
            {
                result.ErrorCode = 1207;
                result.ErrorMessage = "Buyer change address missing";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }



            // Retrieve the TxHash und TxHash from the Transaction - if the Transaction is not verified already, we stop with an error
            string SmartContractTxIn = await StaticTransactionFunctions.GetSmartContractTxin(
                preparedtransaction.PreparedpaymenttransactionsSmartcontractsjsons.Last().Txid,
                preparedtransaction.Smartcontracts.Address);
            if (string.IsNullOrEmpty(SmartContractTxIn))
            {
                var last = preparedtransaction.PreparedpaymenttransactionsSmartcontractsjsons.Last();
                if (last.Templatetype == nameof(DatumTemplateTypes.bet) &&
                    preparedtransaction.Smartcontractstate == nameof(PaymentTransactionSubstates.waitingforbid) &&
                    last.Created < DateTime.Now.AddMinutes(-30))
                {
                    db.PreparedpaymenttransactionsSmartcontractsjsons.Remove(last);
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

            // Just for testing
            postparameter.Buyer.CollateralTxIn = "";

            var txin = StaticTransactionFunctions.GetAllNeededTxin(_redis,postparameter.Buyer.Addresses,
                postparameter.BuyerOffer + (long)(preparedtransaction.Lockamount ?? 0) + changeLockAmount, 0, null,
                postparameter.Buyer.CollateralTxIn, out string errormessage, out AllTxInAddressesClass alltxin);

            PreparedpaymenttransactionsSmartcontractsjson ptsj = new()
            {
                PreparedpaymenttransactionsId = preparedtransaction.Id,
                Json = StaticTransactionFunctions.FillJsonTemplateSellerAuction(db, preparedtransaction, postparameter.BuyerOffer, GlobalFunctions.GetPkhFromAddress(postparameter.Buyer.ChangeAddress)),
                Redeemer = StaticTransactionFunctions.FillJsonTemplateRedeemer(db, preparedtransaction, GlobalFunctions.GetPkhFromAddress(postparameter.Buyer.ChangeAddress)),
                Templatetype = "bet",
            };
            ptsj.Hash = StaticTransactionFunctions.GetHash(ptsj.Json);


            string guid = GlobalFunctions.GetGuid();
            string matxrawfile = GeneralConfigurationClass.TempFilePath + "matx" + guid + ".raw";
            string protocolParamsFile = GeneralConfigurationClass.TempFilePath + "protocol" + guid + ".params";
            string redeemerfile = GeneralConfigurationClass.TempFilePath + "redeemer" + guid + ".json";
            string scriptfile = GeneralConfigurationClass.TempFilePath + "" + guid + preparedtransaction.Smartcontracts.Filename;
            string olddatumfile = GeneralConfigurationClass.TempFilePath + "olddatum" + guid + ".json";
            string newdatumfile = GeneralConfigurationClass.TempFilePath + "newdatum" + guid + ".json";

            // Write Plutus Script
            if (!string.IsNullOrEmpty(preparedtransaction.Smartcontracts.Plutus))
                await System.IO.File.WriteAllTextAsync(scriptfile, preparedtransaction.Smartcontracts.Plutus);

            // Write Old Datum file - the last used JSON File
            var olddatum = preparedtransaction.PreparedpaymenttransactionsSmartcontractsjsons.LastOrDefault();
            if (olddatum != null)
                await System.IO.File.WriteAllTextAsync(olddatumfile, olddatum.Json);

            var newdatum = ptsj;
            await System.IO.File.WriteAllTextAsync(newdatumfile, newdatum.Json);

            ConsoleCommand.GenerateProtocolParamsFile(protocolParamsFile,_redis, GlobalFunctions.IsMainnet(), out errormessage);
            BuildTransactionClass bt = new();

            var qt = ConsoleCommand.GetQueryTip();
            long slot = qt.Slot??0;

            await System.IO.File.WriteAllTextAsync(redeemerfile, ptsj.Redeemer);

            SmartContractAuctionsParameterClass scapc = new()
            {
                bidamount = postparameter.BuyerOffer,                       // The new offer for the bid in lovelace
                scriptfile = scriptfile,                                    // The Scriptfile - for action it is the auction.plutus
                olddatumfile = olddatumfile,                                // The Old Datum hash from the further action
                utxoScript = SmartContractTxIn,                             // The Tx-In of the Smart Contract - we have to get this from GetSmartContractsTxIn
                scripthash = preparedtransaction.Smartcontracts.Address,    // Scripthash is the address of the smart contract
                protocolParamsFile = protocolParamsFile,
                collateraltxin = postparameter.Buyer.CollateralTxIn,        // The Collateral TX-In
                changeaddress = postparameter.Buyer.ChangeAddress,          // Change Address for the rest of Lovelace
                matxrawfile = matxrawfile,
                utxopaymentaddress = txin,                                  // The needed TX-IN from the Bidder - here must be min. the adaamount what he is bidding
                scriptDatumHash = ptsj.Hash,                                // The new Datum hash from this action - so bid-n.json
                newdatumfile = newdatumfile,
                startslot = slot,
                next10slots = slot + 150,
                redeemerfile = redeemerfile,
                signerhash = postparameter.Buyer.Pkh,
                tokencount = preparedtransaction.Tokencount,
                policyidAndTokenname = preparedtransaction.Policyid + "." + preparedtransaction.Tokenname,
            };

            if (olddatum.Templatetype == nameof(DatumTemplateTypes.bet))
            {
                scapc.receiver.Add( new()
                { address = olddatum.Address, lovelace = olddatum.Bidamount??0, receivertype = ReceiverTypes.buyer});
            }


            string sendbackmessagefile = GeneralConfigurationClass.TempFilePath + "payment" + guid + ".metadata";

            //     if (!string.IsNullOrEmpty(sendbackmessage))
            {
                ConsoleCommand.CreateSendbackMessageMetadata(sendbackmessagefile, "There was a higher bid");
            }
            scapc.sendbackmessagefile = sendbackmessagefile;

            var ok = ConsoleCommand.SmartContractsAuctionTransactionBid(_redis,scapc, GlobalFunctions.IsMainnet(), ref bt);


            // Last check if the transaction is still in the right state - mabye we have to check this with a mysql function
            if (ok)
            {
                var prep1 = await (from a in db.Preparedpaymenttransactions
                                   where a.Id == preparedtransaction.Id
                                   select a).AsNoTracking().FirstOrDefaultAsync();
                if (prep1.Smartcontractstate != nameof(PaymentTransactionSubstates.waitingforbid))
                {
                    result.ErrorCode = 1316;
                    result.ErrorMessage = "Smartcontract is not any longer in the state of bidding";
                    result.ResultState = ResultStates.Error;
                    return StatusCode(406, result);
                }
                preparedtransaction.Smartcontractstate = nameof(PaymentTransactionSubstates.readytosignbybuyer);
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
                ptsj.Signinguid = "B" + guid;
                ptsj.Address = postparameter.Buyer.ChangeAddress;
                // We need to "fake" sign the cbor - because of an error in the serialisatzon lib. Nami can not sign it, without the signing space - so we sign it and nami replaces the signature
          //      await StaticTransactionFunctions.FakeSign(db, preparedtransaction);

//                ptsj.Signedcbr = ConsoleCommand.SignTx(matxrawfile, preparedtransaction.Nftproject.Smartcontractssettings.Fakesignskey);

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

            GlobalFunctions.DeleteFile(matxrawfile);
            GlobalFunctions.DeleteFile(protocolParamsFile);
            GlobalFunctions.DeleteFile(redeemerfile);
            GlobalFunctions.DeleteFile(scriptfile);
            GlobalFunctions.DeleteFile(olddatumfile);
            GlobalFunctions.DeleteFile(newdatumfile);

            return Ok(StaticTransactionFunctions.GetTransactionState(db, _redis, preparedtransaction.Transactionuid, true));
        }

    }
}
