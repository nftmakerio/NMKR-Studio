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
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.v2.Paymenttransactions.Prepare.SmartContract
{
    public class PreparedPaymentTransactionsSmartcontractsDirectsaleClass : PreparedPaymentTransactionsSmartcontractsClass
    {
        public PreparedPaymentTransactionsSmartcontractsDirectsaleClass(EasynftprojectsContext db, IConnectionMultiplexer redis, PaymentTransactionTypes PaymentTransactionType) : base(db, redis, PaymentTransactionType)
        {
        }

        public override ApiErrorResultClass CheckParameter(CreatePaymentTransactionClass paymenttransaction, Nftproject project,
            ApiErrorResultClass result, out int statuscode)
        {
            result = base.CheckParameter(paymenttransaction,project, result, out statuscode);
            if (statuscode != 0)
            {
                return result;
            }

            statuscode = 0;
            if (paymenttransaction.PaymentTransactionType != PaymentTransactionTypes.smartcontract_directsale)
                return result;


            if (paymenttransaction.DirectSaleParameters == null)
            {
                result.ErrorCode = 1009;
                result.ErrorMessage = "You must submit the directsale parameters";
                result.ResultState = ResultStates.Error;
                statuscode = 406;
                return result;
            }
            if (paymenttransaction.DirectSaleOfferParameters != null)
            {
                result.ErrorCode = 1016;
                result.ErrorMessage = "You must not submit the directsale offer parameters";
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

            if (!string.IsNullOrEmpty(paymenttransaction.DirectSaleParameters.SmartContractName))
            {
                var sm=(from a in db.Smartcontracts
                        where a.Smartcontractname==paymenttransaction.DirectSaleParameters.SmartContractName
                        select a).FirstOrDefault();
                if (sm == null)
                {
                    result.ErrorCode = 1514;
                    result.ErrorMessage = "Smartcontract is not valid. Leave field blank for automatic or specifiy a correct one";
                    result.ResultState = ResultStates.Error;
                    statuscode = 406;
                    return result;
                }
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
            bool checkTransactionparmeters = !(paymenttransaction.DirectSaleParameters != null &&
                                               !string.IsNullOrEmpty(paymenttransaction.DirectSaleParameters.TxHashForAlreadyLockedinAssets));

            if (checkTransactionparmeters)
            {
                if (paymenttransaction.DirectSaleParameters != null &&
                    paymenttransaction.DirectSaleParameters.PriceInLovelace == 0)
                {
                    result.ErrorCode = 1159;
                    result.ErrorMessage = "Missing Price in lovelace for your NFT.";
                    result.ResultState = ResultStates.Error;
                    statuscode = 406;
                    return result;
                }


                if (paymenttransaction.DirectSaleParameters != null && !string.IsNullOrEmpty(paymenttransaction.DirectSaleParameters.SmartContractName))
                {
                    var sm = (from a in db.Smartcontracts
                        where a.Smartcontractname == paymenttransaction.DirectSaleParameters.SmartContractName && a.State=="active" && (a.Type == "directsale" || a.Type == "directsaleV2")
                              select a).FirstOrDefault();

                    if (sm == null)
                    {
                        result.ErrorCode = 1175;
                        result.ErrorMessage = "Smartcontractname is not correct";
                        result.ResultState = ResultStates.Error;
                        statuscode = 406;
                        return result;
                    }

                    if (paymenttransaction.DirectSaleParameters != null)
                    {
                        preparedpaymenttransaction.SmartcontractsId = sm.Id;
                        preparedpaymenttransaction.Lovelace = paymenttransaction.DirectSaleParameters.PriceInLovelace;

                        db.SaveChanges();
                    }
                }
                else
                {
                    var smartcontractscript = (from a in db.Smartcontracts
                        where (a.Type == "directsale" || a.Type=="directsaleV2")  && a.State == "active"
                        orderby a.Id descending 
                        select a).FirstOrDefault();

                    if (smartcontractscript == null)
                    {
                        result.ErrorCode = 1169;
                        result.ErrorMessage = "No active smartcontract found";
                        result.ResultState = ResultStates.Error;
                        statuscode = 406;
                        return result;
                    }

                    if (paymenttransaction.DirectSaleParameters != null)
                    {
                        preparedpaymenttransaction.SmartcontractsId = smartcontractscript?.Id;
                        preparedpaymenttransaction.Lovelace = paymenttransaction.DirectSaleParameters.PriceInLovelace;

                        db.SaveChanges();
                    }
                }

                // Save Outputs
                var nmkrSmartcontractFee = StaticTransactionFunctions.GetNmkrSmartcontractFeeLovelace(preparedpaymenttransaction, project);
                var nmkrMarketplaceFee = StaticTransactionFunctions.GetMarketplaceFeeLovelace(preparedpaymenttransaction, project);
                long royaltyamount = StaticTransactionFunctions.GetRoyaltyLovelace(preparedpaymenttransaction.Policyid, preparedpaymenttransaction.Lovelace??0, out string royaltyaddress);
                long refereramount = StaticTransactionFunctions.GetRefererLovelace(db, preparedpaymenttransaction.NftprojectId, preparedpaymenttransaction.Lovelace??0, out string refereraddress);

                if (nmkrSmartcontractFee != null)
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
                if (!paymenttransaction.DirectSaleParameters.TxHashForAlreadyLockedinAssets.Contains("#"))
                {
                    result.ErrorCode = 1129;
                    result.ErrorMessage = "Missing TxHash in TxHash - separated with #";
                    result.ResultState = ResultStates.Error;
                    statuscode = 406;
                    return result;
                }

                string txhash = paymenttransaction.DirectSaleParameters.TxHashForAlreadyLockedinAssets.Split('#').First();
                long txid = Convert.ToInt64(paymenttransaction.DirectSaleParameters.TxHashForAlreadyLockedinAssets
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

                var asset = bl.Outputs.FirstOrDefault(x=>x.OutputIndex == txid && x.DataHash!=null);
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
                    paymenttransaction.DirectSaleParameters.TxHashForAlreadyLockedinAssets;
                db.SaveChanges();

                var getdatum = new GetDatumInformationForSmartcontractDirectsaleTransactionController(null);

                var datumx = getdatum.GetSmartcontractsOutputs(result,
                    paymenttransaction.DirectSaleParameters.TxHashForAlreadyLockedinAssets);

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
                    var a = new PreparedpaymenttransactionsSmartcontractOutput()
                    {
                        Address = smartcontractDirectsaleReceiverClass.Address,
                        Pkh = smartcontractDirectsaleReceiverClass.Pkh,
                        Lovelace = smartcontractDirectsaleReceiverClass.AmountInLovelace,
                        PreparedpaymenttransactionsId = preparedpaymenttransaction.Id,
                    };
                    db.PreparedpaymenttransactionsSmartcontractOutputs.Add(a);
                    db.SaveChanges();
                   
                    if (smartcontractDirectsaleReceiverClass.Tokens == null) continue;

                    foreach (var token in smartcontractDirectsaleReceiverClass.Tokens)
                    {
                        db.PreparedpaymenttransactionsSmartcontractOutputsAssets.Add(
                            new PreparedpaymenttransactionsSmartcontractOutputsAsset()
                            {
                                Amount = token.TotalCount,
                                Policyid = token.PolicyId,
                                Tokennameinhex = token.AssetNameInHex,
                                PreparedpaymenttransactionsSmartcontractOutputsId = a.Id
                            });
                        preparedpaymenttransaction.Policyid=token.PolicyId;
                        preparedpaymenttransaction.Tokenname=token.AssetNameInHex;
                        preparedpaymenttransaction.Tokencount=token.TotalCount;
                        db.SaveChanges();
                    }
                }

                preparedpaymenttransaction.Lockamount = asset.Amount.Where(x => x.Unit == "lovelace").Sum(x => x.Quantity);
                preparedpaymenttransaction.Lovelace = datum.Receivers.Sum(x => x.AmountInLovelace) - preparedpaymenttransaction.Lockamount;
                preparedpaymenttransaction.State = nameof(PaymentTransactionsStates.active);
                

                preparedpaymenttransaction.Selleraddress = datum.Receivers.MaxBy(x => x.AmountInLovelace)?.Address;
              //  preparedpaymenttransaction.Selleraddresses = preparedpaymenttransaction.Selleraddresses;
                preparedpaymenttransaction.Sellerpkh= datum.Receivers.MaxBy(x => x.AmountInLovelace)?.Pkh;
                preparedpaymenttransaction.Txhash =
                    paymenttransaction.DirectSaleParameters.TxHashForAlreadyLockedinAssets;
                var jsontemplate = new PreparedpaymenttransactionsSmartcontractsjson
                {
                    PreparedpaymenttransactionsId = preparedpaymenttransaction.Id,
                    Confirmed = true,
                    Created = DateTime.Now,
                    Txid = paymenttransaction.DirectSaleParameters.TxHashForAlreadyLockedinAssets,
                    Fee = 0,
                 Json= smartcontract.Id==2?GetOldDatumFromFurtherTx(db, paymenttransaction.DirectSaleParameters.TxHashForAlreadyLockedinAssets) :  StaticTransactionFunctions.FillJsonTemplateSellerDirectsale(db,
                     preparedpaymenttransaction),
                Address = preparedpaymenttransaction.Selleraddresses,
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
                foreach (var token in asset.Amount.Where(x=>x.Unit!="lovelace"))
                {
                    db.PreparedpaymenttransactionsNfts.Add(new PreparedpaymenttransactionsNft()
                    {
                        Count = token.Quantity??1, Policyid = token.Unit.Substring(0,56), Tokennamehex = token.Unit.Substring(56),
                        Tokenname = token.Unit.Substring(56).FromHex(),
                        PreparedpaymenttransactionsId = preparedpaymenttransaction.Id,
                        Lovelace = preparedpaymenttransaction.Lockamount ?? 2000000,
                    });
                }
                
                db.SaveChanges();


            }


            return result;
        }

        private string GetOldDatumFromFurtherTx(EasynftprojectsContext db, string txHashForAlreadyLockedinAssets)
        { 
            var txhash = txHashForAlreadyLockedinAssets.Split("#").First();
           var tx = (from a in db.PreparedpaymenttransactionsSmartcontractsjsons
                     where a.Txid==txhash && a.Templatetype=="locknft" orderby a.Id descending
                     select a).AsNoTracking().FirstOrDefault();

           if (tx!=null)
               return tx.Json;

           return "";
        }
    }
}
