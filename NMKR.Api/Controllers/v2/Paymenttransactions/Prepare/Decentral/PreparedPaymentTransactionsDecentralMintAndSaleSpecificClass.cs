using System;
using System.Linq;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.v2.Paymenttransactions.Prepare.Decentral
{
    public class PreparedPaymentTransactionsDecentralMintAndSaleSpecificClass : PreparedPaymentTransactionsDecentralClass
    {
        public PreparedPaymentTransactionsDecentralMintAndSaleSpecificClass(EasynftprojectsContext db, IConnectionMultiplexer redis, PaymentTransactionTypes PaymentTransactionType) : base(db,redis, PaymentTransactionType)
        {
        }
        public override ApiErrorResultClass CheckParameter(CreatePaymentTransactionClass paymenttransaction, Nftproject project, ApiErrorResultClass result, out int statuscode)
        {
            result = base.CheckParameter(paymenttransaction,project,  result, out statuscode);
            if (statuscode != 0)
            {
                return result;
            }

            if (project.Disablespecificsales)
            {
                LogClass.LogMessage(db, "API-CALL: ERROR: Specific sales not enabled ");
                result.ErrorCode = 4502;
                result.ErrorMessage = "Specific sales are not enabled on this project";
                result.ResultState = ResultStates.Error;
                statuscode = 406;
                GlobalFunctions.LogMessage(db, $"API-Call: Decentral transaction error - {project.Id} - {result.ErrorMessage}", JsonConvert.SerializeObject(paymenttransaction));
                return result;
            }


            statuscode = 0;
            if (paymenttransaction.PaymentTransactionType != PaymentTransactionTypes.decentral_mintandsale_specific)
                return result;

            if (paymenttransaction.DecentralParameters.MintNfts == null)
            {
                result.ErrorCode = 1006;
                result.ErrorMessage = "You must specify NFTs when you will sell them specific";
                result.ResultState = ResultStates.Error;
                statuscode = 406;
                GlobalFunctions.LogMessage(db, $"API-Call: Decentral transaction error - {project.Id} - {result.ErrorMessage}", JsonConvert.SerializeObject(paymenttransaction));
                return result;
            }

            if (paymenttransaction.DecentralParameters.MintNfts.ReserveNfts == null ||
                !paymenttransaction.DecentralParameters.MintNfts.ReserveNfts.Any())
            {
                result.ErrorCode = 1004;
                result.ErrorMessage = "You must specify NFTs when you will sell them specific";
                result.ResultState = ResultStates.Error;
                statuscode = 406;
                GlobalFunctions.LogMessage(db, $"API-Call: Decentral transaction error - {project.Id} - {result.ErrorMessage}", JsonConvert.SerializeObject(paymenttransaction));
                return result;
            }

            foreach (var mintNftsReserveNft in paymenttransaction.DecentralParameters.MintNfts.ReserveNfts)
            {
                if (mintNftsReserveNft.Tokencount == 0)
                {
                    result.ErrorCode = 1007;
                    result.ErrorMessage = "Specify tokencount in the reservenfts";
                    result.ResultState = ResultStates.Error;
                    statuscode = 406;
                    GlobalFunctions.LogMessage(db, $"API-Call: Decentral transaction error - {project.Id} - {result.ErrorMessage}", JsonConvert.SerializeObject(paymenttransaction));
                    return result;
                }

                if (string.IsNullOrEmpty(mintNftsReserveNft.NftUid))
                {
                    result.ErrorCode = 1008;
                    result.ErrorMessage = "Specify nft uid in the reservenfts";
                    result.ResultState = ResultStates.Error;
                    statuscode = 406;
                    GlobalFunctions.LogMessage(db, $"API-Call: Decentral transaction error - {project.Id} - {result.ErrorMessage}", JsonConvert.SerializeObject(paymenttransaction));
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


            long totallovelace = 0;
            foreach (var ptcReserveSpecificNft in paymenttransaction.DecentralParameters.MintNfts.ReserveNfts)
            {
                var nftid = (from a in db.Nfts
                             where a.Uid == ptcReserveSpecificNft.NftUid && a.NftprojectId == project.Id
                             select a).FirstOrDefault();
                if (nftid == null)
                {
                    result.ErrorCode = 1002;
                    result.ErrorMessage = "NFT UID not found or not connected with this project";
                    result.ResultState = ResultStates.Error;
                    statuscode = 404;
                    GlobalFunctions.LogMessage(db, $"API-Call: Decentral transaction error - {project.Id} - {result.ErrorMessage}", JsonConvert.SerializeObject(paymenttransaction));
                    return result;
                }

                long lovelace = 0;
                if (ptcReserveSpecificNft.Lovelace == null || ptcReserveSpecificNft.Lovelace == 0)
                {
                    if (nftid.Price != null)
                        lovelace = (long)nftid.Price;
                    else
                    {
                        if (ptcReserveSpecificNft.Tokencount == 0)
                            ptcReserveSpecificNft.Tokencount = 1;


                        // Save price from pricelist to the transaction
                        var pricelist = (from a in db.Pricelists
                            where a.NftprojectId == project.Id && a.State == "active" &&
                                  (a.Validfrom == null || a.Validfrom <= DateTime.Now) &&
                                  (a.Validto == null || a.Validto >= DateTime.Now) && a.Countnftortoken ==
                                  ptcReserveSpecificNft.Tokencount
                            select a).FirstOrDefault();
                        if (pricelist != null)
                        {
                            lovelace = GlobalFunctions.GetPriceInEntities(redis, pricelist);

                            if (pricelist.Priceintoken is > 0 && !string.IsNullOrEmpty(pricelist.Tokenpolicyid))
                            {
                                var multiplier = GlobalFunctions.GetFtTokensMultiplier(pricelist.Tokenpolicyid, pricelist.Assetnamehex??pricelist.Tokenassetid.ToHex());
                                PreparedpaymenttransactionsTokenprice tp = new()
                                {
                                    Policyid = pricelist.Tokenpolicyid,
                                    Assetname = pricelist.Tokenassetid,
                                    Assetnamehex = pricelist.Assetnamehex??pricelist.Tokenassetid.ToHex(),
                                    PreparedpaymenttransactionId = preparedpaymenttransaction.Id,
                                    Totalcount = (long)pricelist.Priceintoken,
                                    Multiplier = multiplier.Multiplier,
                                    Tokencount = Convert.ToInt64(pricelist.Priceintoken / multiplier.Multiplier),
                                    Decimals = multiplier.Decimals,
                                };
                                db.PreparedpaymenttransactionsTokenprices.Add(tp);
                                db.SaveChanges();
                            }

                        }
                        else
                        {
                            result.ErrorCode = 1008;
                            result.ErrorMessage = "No price found";
                            result.ResultState = ResultStates.Error;
                            statuscode = 404;
                            return result;
                        }
                    }
                }
                else
                {
                    lovelace = (long)ptcReserveSpecificNft.Lovelace;
                }

                var reserve = new PreparedpaymenttransactionsNft()
                {
                    Count = ptcReserveSpecificNft.Tokencount??1,// * Math.Max(1,project.Multiplier),
                    Tokennamehex = GlobalFunctions.ToHexString(project + nftid.Name),
                    Policyid = nftid.Policyid,
                    Tokenname = project.Tokennameprefix + nftid.Name,
                    NftId = nftid.Id,
                    PreparedpaymenttransactionsId = preparedpaymenttransaction.Id,
                    Lovelace = lovelace,
                    Nftuid = ptcReserveSpecificNft.NftUid
                };
                totallovelace += lovelace;

                db.Add(reserve);
                db.SaveChanges();
            }


            // Set Promotion Multiplier
            preparedpaymenttransaction.Promotionmultiplier = paymenttransaction.DecentralParameters.MintNfts.ReserveNfts.Length;


            preparedpaymenttransaction.Lovelace = totallovelace;
            db.SaveChanges();


            return result;
        }
    }
}
