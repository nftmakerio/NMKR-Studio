using System;
using System.Linq;
using NMKR.Api.Controllers.SharedClasses;
using NMKR.Api.Controllers.v2.SmartContracts;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.Blockfrost;
using NMKR.Shared.Model;
using NMKR.Shared.SmartContracts;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.v2.Paymenttransactions.Prepare.SmartContract
{
    public class PreparedPaymentTransactionsSmartcontractsDirectsaleOffersClass : PreparedPaymentTransactionsSmartcontractsClass
    {
        public PreparedPaymentTransactionsSmartcontractsDirectsaleOffersClass(EasynftprojectsContext db, IConnectionMultiplexer redis, PaymentTransactionTypes PaymentTransactionType) : base(db,redis, PaymentTransactionType)
        {
        }

        public override ApiErrorResultClass CheckParameter(CreatePaymentTransactionClass paymenttransaction, Nftproject project,
            ApiErrorResultClass result, out int statuscode)
        {
            result = base.CheckParameter(paymenttransaction, project, result, out statuscode);
            if (statuscode != 0)
            {
                return result;
            }

            statuscode = 0;
            if (paymenttransaction.PaymentTransactionType != PaymentTransactionTypes.smartcontract_directsale_offer)
                return result;


            if (paymenttransaction.DirectSaleOfferParameters == null)
            {
                result.ErrorCode = 1009;
                result.ErrorMessage = "You must submit the directsale offer parameters";
                result.ResultState = ResultStates.Error;
                statuscode = 406;
                return result;
            }
            if (paymenttransaction.DirectSaleParameters != null)
            {
                result.ErrorCode = 1016;
                result.ErrorMessage = "You must not submit the directsale parameters";
                result.ResultState = ResultStates.Error;
                statuscode = 406;
                return result;
            }
            if (paymenttransaction.AuctionParameters != null)
            {
                result.ErrorCode = 1014;
                result.ErrorMessage = "You must not submit the auction parameters";
                result.ResultState = ResultStates.Error;
                statuscode = 406;
                return result;
            }

            return result;
        }


        public override ApiErrorResultClass SaveTransaction(CreatePaymentTransactionClass paymenttransaction,
            ApiErrorResultClass result, Nftproject project, out Preparedpaymenttransaction preparedpaymenttransaction,
            out int statuscode)
        {
            result = base.SaveTransaction(paymenttransaction, result, project, out preparedpaymenttransaction,
                out statuscode);
            if (result.ResultState == ResultStates.Error)
                return result;
            bool checkTransactionparmeters = !(paymenttransaction.DirectSaleOfferParameters != null &&
                                               !string.IsNullOrEmpty(paymenttransaction.DirectSaleOfferParameters.TxHashForAlreadyLockedinAssets));

            if (checkTransactionparmeters)
            {
                if (paymenttransaction.DirectSaleOfferParameters != null &&
                    paymenttransaction.DirectSaleOfferParameters.OfferInLovelace == 0)
                {
                    result.ErrorCode = 1159;
                    result.ErrorMessage = "Missing Offer in lovelace for the NFT.";
                    result.ResultState = ResultStates.Error;
                    statuscode = 406;
                    return result;
                }


                var smartcontractscript = (from a in db.Smartcontracts
                    where a.Type == "directsaleoffer" && a.State == "active"
                    select a).FirstOrDefault();

                    if (paymenttransaction.DirectSaleOfferParameters != null)
                    {
                        preparedpaymenttransaction.SmartcontractsId = smartcontractscript?.Id;
                        preparedpaymenttransaction.Lovelace = paymenttransaction.DirectSaleOfferParameters.OfferInLovelace;

                        db.SaveChanges();
                    }

                // Save Outputs
                var nmkrSmartcontractFee = StaticTransactionFunctions.GetNmkrSmartcontractFeeLovelace(preparedpaymenttransaction, project);
                var nmkrMarketplaceFee = StaticTransactionFunctions.GetMarketplaceFeeLovelace(preparedpaymenttransaction, project);
                long royaltyamount = StaticTransactionFunctions.GetRoyaltyLovelace(preparedpaymenttransaction.Policyid, preparedpaymenttransaction.Lovelace ?? 0, out string royaltyaddress);
                long refereramount = StaticTransactionFunctions.GetRefererLovelace(db, preparedpaymenttransaction.NftprojectId, preparedpaymenttransaction.Lovelace ?? 0, out string refereraddress);

                if (nmkrSmartcontractFee!= null)
                {
                    db.PreparedpaymenttransactionsSmartcontractOutputs.Add(nmkrSmartcontractFee);
                    db.SaveChanges();
                }

                if (nmkrMarketplaceFee != null)
                {
                    db.PreparedpaymenttransactionsSmartcontractOutputs.Add(nmkrMarketplaceFee);
                    db.SaveChanges();
                }


                if (royaltyamount != 0 && !string.IsNullOrEmpty(royaltyaddress))
                {
                    db.PreparedpaymenttransactionsSmartcontractOutputs.Add(
                        new PreparedpaymenttransactionsSmartcontractOutput()
                        {
                            Address = royaltyaddress,
                            Pkh = GlobalFunctions.GetPkhFromAddress(royaltyaddress),
                            Lovelace = royaltyamount,
                            PreparedpaymenttransactionsId = preparedpaymenttransaction.Id,
                            Type = "royalties"
                        });
                    db.SaveChanges();
                }
                if (refereramount != 0 && !string.IsNullOrEmpty(refereraddress))
                {
                    db.PreparedpaymenttransactionsSmartcontractOutputs.Add(
                        new PreparedpaymenttransactionsSmartcontractOutput()
                        {
                            Address = refereraddress,
                            Pkh = GlobalFunctions.GetPkhFromAddress(refereraddress),
                            Lovelace = refereramount,
                            PreparedpaymenttransactionsId = preparedpaymenttransaction.Id,
                            Type = "referer"
                        });
                    db.SaveChanges();
                }


            }
            else
            {
                if (!paymenttransaction.DirectSaleOfferParameters.TxHashForAlreadyLockedinAssets.Contains("#"))
                {
                    result.ErrorCode = 1129;
                    result.ErrorMessage = "Missing TxHash in TxHash - separated with #";
                    result.ResultState = ResultStates.Error;
                    statuscode = 406;
                    return result;
                }

                string txhash = paymenttransaction.DirectSaleOfferParameters.TxHashForAlreadyLockedinAssets.Split('#').First();
                long txid = Convert.ToInt64(paymenttransaction.DirectSaleOfferParameters.TxHashForAlreadyLockedinAssets
                    .Split('#').Last());

                var bl = BlockfrostFunctions.GetTransactionUtxoFromBlockfrost(txhash);
                if (bl == null || !bl.Outputs.Any())
                {
                    result.ErrorCode = 1128;
                    result.ErrorMessage = "TxHash not found";
                    result.ResultState = ResultStates.Error;
                    statuscode = 406;
                    return result;
                }

                var asset = bl.Outputs.FirstOrDefault(x => x.OutputIndex == txid && x.DataHash != null);
                if (asset == null)
                {
                    result.ErrorCode = 1129;
                    result.ErrorMessage = "TxHash not found";
                    result.ResultState = ResultStates.Error;
                    statuscode = 406;
                    return result;
                }

                var smartcontract = (from a in db.Smartcontracts
                                     where a.Address == asset.Address
                                     select a).FirstOrDefault();

                if (smartcontract == null)
                {
                    result.ErrorCode = 1134;
                    result.ErrorMessage = "Smartcontract not supported";
                    result.ResultState = ResultStates.Error;
                    statuscode = 406;
                    return result;
                }

                preparedpaymenttransaction.SmartcontractsId = smartcontract.Id;
                preparedpaymenttransaction.Smartcontractstate = nameof(PaymentTransactionSubstates.waitingforsale);
                preparedpaymenttransaction.Txinforalreadylockedtransactions =
                    paymenttransaction.DirectSaleOfferParameters.TxHashForAlreadyLockedinAssets;
                db.SaveChanges();

                var getdatum = new GetDatumInformationForSmartcontractDirectsaleTransactionController(null);

                var datumx = getdatum.GetSmartcontractsOutputs(result,
                    paymenttransaction.DirectSaleOfferParameters.TxHashForAlreadyLockedinAssets);

                if (datumx.ApiError.ResultState == ResultStates.Error)
                {
                    result.ErrorCode = 1523;
                    result.ErrorMessage = "Datum from TXHash can not retrieved";
                    result.ResultState = ResultStates.Error;
                    statuscode = 406;
                    return result;
                }

                var datum = datumx.SuccessResultObject as SmartcontractDirectsaleDatumInformationClass;

                if (datum == null)
                {
                    result.ErrorCode = 1525;
                    result.ErrorMessage = "Datum from TXHash can not retrieved";
                    result.ResultState = ResultStates.Error;
                    statuscode = 406;
                    return result;
                }

                foreach (var smartcontractDirectsaleReceiverClass in datum.Receivers)
                {
                    db.PreparedpaymenttransactionsSmartcontractOutputs.Add(
                        new PreparedpaymenttransactionsSmartcontractOutput()
                        {
                            Address = smartcontractDirectsaleReceiverClass.Address,
                            Pkh = smartcontractDirectsaleReceiverClass.Pkh,
                            Lovelace = smartcontractDirectsaleReceiverClass.AmountInLovelace,
                            PreparedpaymenttransactionsId = preparedpaymenttransaction.Id,
                        });
                }

                preparedpaymenttransaction.Lovelace = datum.Receivers.Sum(x => x.AmountInLovelace);
                preparedpaymenttransaction.State = nameof(PaymentTransactionsStates.active);
                preparedpaymenttransaction.Lockamount = asset.Amount.Where(x => x.Unit == "lovelace").Sum(x => x.Quantity);

                preparedpaymenttransaction.Buyeraddress = datum.Receivers.MinBy(x => x.AmountInLovelace)?.Address;
              //  preparedpaymenttransaction.Buyeraddresses = preparedpaymenttransaction.Buyeraddresses;
                preparedpaymenttransaction.Buyerpkh = datum.Receivers.MinBy(x => x.AmountInLovelace)?.Pkh;
                preparedpaymenttransaction.Txhash =
                    paymenttransaction.DirectSaleOfferParameters.TxHashForAlreadyLockedinAssets;
                var jsontemplate = new PreparedpaymenttransactionsSmartcontractsjson
                {
                    PreparedpaymenttransactionsId = preparedpaymenttransaction.Id,
                    Confirmed = true,
                    Created = DateTime.Now,
                    Txid = paymenttransaction.DirectSaleOfferParameters.TxHashForAlreadyLockedinAssets,
                    Fee = 0,
                    //  Json = CSLServiceFunctions.PlutusDataCborToJson(datum.DatumCbor),
                    Json = BlockfrostFunctions.GetDatumFromDatumHash(asset.DataHash),
                    Address = preparedpaymenttransaction.Buyeraddresses,
                    Templatetype = nameof(DatumTemplateTypes.locknft),
                };

                if (string.IsNullOrEmpty(jsontemplate.Json))
                {
                    result.ErrorCode = 1526;
                    result.ErrorMessage = "Json can not created from Datum";
                    result.ResultState = ResultStates.Error;
                    statuscode = 406;
                    return result;
                }
                jsontemplate.Hash = "";
                db.PreparedpaymenttransactionsSmartcontractsjsons.Add(jsontemplate);

                db.SaveChanges();



                // Add NFTS
                foreach (var token in asset.Amount.Where(x => x.Unit != "lovelace"))
                {
                    db.PreparedpaymenttransactionsNfts.Add(new PreparedpaymenttransactionsNft()
                    {
                        Count = token.Quantity ?? 1,
                        Policyid = token.Unit.Substring(0, 56),
                        Tokennamehex = token.Unit.Substring(56),
                        Tokenname = token.Unit.Substring(56).FromHex(),
                        PreparedpaymenttransactionsId = preparedpaymenttransaction.Id,
                        Lovelace = preparedpaymenttransaction.Lockamount ?? 2000000,
                    });
                }

                db.SaveChanges();
            }


            return result;
        }
    }
}
