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

namespace NMKR.Api.Controllers.v2.Paymenttransactions.Processing.Legacy
{
    public class BuyDirectsaleLegacyClass : ControllerBase, IProcessPaymentTransactionInterface
    {
        private readonly IConnectionMultiplexer _redis;
       
        public BuyDirectsaleLegacyClass(IConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        private readonly long changeLockAmount = 3000000;
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

            if (preparedtransaction.Transactiontype != nameof(PaymentTransactionTypes.legacy_directsale))
            {
                result.ErrorCode = 1102;
                result.ErrorMessage = "Command does not fit to this transaction";
                result.ResultState = ResultStates.Error;
                return StatusCode(406, result);
            }

            /*  if (preparedtransaction.Smartcontractstate != nameof(PaymentTransactionSubstates.waitingforsale))
              {
                  result.ErrorCode = 1311;
                  result.ErrorMessage = "Transaction is not in the state of buying";
                  result.ResultState = ResultStates.Error;
                  return StatusCode(406, result);
              }*/

            if (preparedtransaction.Lovelace != null && postparameter.BuyerOffer != preparedtransaction.Lovelace)
            {
                result.ErrorCode = 1201;
                result.ErrorMessage = "Price and Amount is not equal";
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

            if (postparameter.Buyer.Pkh == null)
            {
                result.ErrorCode = 1206;
                result.ErrorMessage = "Buyer private key hash missing";
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


            var last = preparedtransaction.PreparedpaymenttransactionsSmartcontractsjsons.Last();
            if (last.Templatetype == nameof(DatumTemplateTypes.buy))
            {
                if (last.Created < DateTime.Now.AddMinutes(-1))
                {
                    db.PreparedpaymenttransactionsSmartcontractsjsons.Remove(last);
                    preparedtransaction.Smartcontractstate = nameof(PaymentTransactionSubstates.waitingforsale);
                    await db.SaveChangesAsync();
                }
                else
                {
                    result.ErrorCode = 1207;
                    result.ErrorMessage =
                        "Sale has pending transactions or is not ready at the moment. Please try again later.";
                    result.ResultState = ResultStates.Error;
                    return StatusCode(406, result);
                }
            }



            var txin = StaticTransactionFunctions.GetAllNeededTxin(_redis,postparameter.Buyer.Addresses,
                postparameter.BuyerOffer + (long)(preparedtransaction.Lockamount ?? 0) + changeLockAmount, 0, null,
                postparameter.Buyer.CollateralTxIn, out string errormessage, out AllTxInAddressesClass alltxin);


            string guid = GlobalFunctions.GetGuid();
            string matxrawfile = GeneralConfigurationClass.TempFilePath + "matx" + guid + ".raw";
            string protocolParamsFile = GeneralConfigurationClass.TempFilePath + "protocol" + guid + ".params";

            ConsoleCommand.GenerateProtocolParamsFile(protocolParamsFile, _redis,GlobalFunctions.IsMainnet(), out errormessage);
            BuildTransactionClass bt = new();

            var qt = ConsoleCommand.GetQueryTip();
            long slot = qt.Slot??0;

            SmartContractAuctionsParameterClass scapc = new()
            {
                bidamount = postparameter.BuyerOffer + preparedtransaction.Lockamount??0, // The new offer for the bid in lovelace
                legacyaddress = preparedtransaction.Legacydirectsales.Address,
                protocolParamsFile = protocolParamsFile,
                collateraltxin = postparameter.Buyer.CollateralTxIn, // The Collateral TX-In
                changeaddress = postparameter.Buyer.ChangeAddress, // Change Address for the rest of Lovelace
                matxrawfile = matxrawfile,
                utxopaymentaddress =
                    txin, // The needed TX-IN from the Bidder - here must be min. the adaamount what he is bidding
                startslot = slot,
                next10slots = slot + 150,
                signerhash = postparameter.Buyer.Pkh,
                tokencount = preparedtransaction.Tokencount,
                policyidAndTokenname = preparedtransaction.Policyid + "." + preparedtransaction.Tokenname,
            };

            var ok = ConsoleCommand.LegacyDirectsaleTransactionSale(_redis,scapc, GlobalFunctions.IsMainnet(), ref bt);

            // Last check if the transaction is still in the right state - mabye we have to check this with a mysql function
            if (ok)
            {
                var prep1 = await (from a in db.Preparedpaymenttransactions
                                   where a.Id == preparedtransaction.Id
                                   select a).AsNoTracking().FirstOrDefaultAsync();
                if (prep1.Smartcontractstate != nameof(PaymentTransactionSubstates.waitingforsale))
                {
                    result.ErrorCode = 1312;
                    result.ErrorMessage = "Directsale is not any longer in the state of buying";
                    result.ResultState = ResultStates.Error;
                    return StatusCode(406, result);
                }

                await db.SaveChangesAsync();
            }

            if (ok)
            {
                string raw = await System.IO.File.ReadAllTextAsync(matxrawfile);

                // We need to "fake" sign the cbor - because of an error in the serialisatzon lib. Nami can not sign it, without the signing space - so we sign it and nami replaces the signature
                await StaticTransactionFunctions.FakeSign(db, preparedtransaction);


                PreparedpaymenttransactionsSmartcontractsjson ptsj = new()
                {
                    PreparedpaymenttransactionsId = preparedtransaction.Id,
                    Templatetype = nameof(DatumTemplateTypes.buy),
                    Fee = bt.Fees,
                    Bidamount = postparameter.BuyerOffer + preparedtransaction.Lockamount,
                    Logfile = bt.LogFile,
                    Rawtx = raw,
                    Created = DateTime.Now,
                    Signinguid = "D" + guid,
                    Address = postparameter.Buyer.ChangeAddress,
                    Signedcbr = ConsoleCommand.SignTx(matxrawfile,
                        preparedtransaction.Nftproject.Smartcontractssettings.Fakesignskey),
                    Hash = "",
                    Redeemer = "",
                    Json = "",
                    Txid = bt.TxHash
                };

                // Save new Json to Database
                db.PreparedpaymenttransactionsSmartcontractsjsons.Add(ptsj);
                preparedtransaction.Smartcontractstate = nameof(PaymentTransactionSubstates.readytosignbybuyer);
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

            return Ok(StaticTransactionFunctions.GetTransactionState(db, _redis, preparedtransaction.Transactionuid, false));
        }
    }
}
