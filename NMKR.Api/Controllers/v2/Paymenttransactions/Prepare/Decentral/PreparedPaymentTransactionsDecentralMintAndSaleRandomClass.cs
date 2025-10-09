using System;
using System.Linq;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.v2.Paymenttransactions.Prepare.Decentral
{
    public class PreparedPaymentTransactionsDecentralMintAndSaleRandomClass : PreparedPaymentTransactionsDecentralClass
    {
        public PreparedPaymentTransactionsDecentralMintAndSaleRandomClass(EasynftprojectsContext db,IConnectionMultiplexer redis,
            PaymentTransactionTypes PaymentTransactionType) : base(db,redis, PaymentTransactionType)
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

            if (project.Disablerandomsales)
            {
                LogClass.LogMessage(db, "API-CALL: ERROR: Random sales not enabled ");
                result.ErrorCode = 4501;
                result.ErrorMessage = "Random sales are not enabled on this project";
                result.ResultState = ResultStates.Error;
                statuscode = 406;
                GlobalFunctions.LogMessage(db, $"API-Call: Decentral transaction error - {project.Id} - {result.ErrorMessage}", JsonConvert.SerializeObject(paymenttransaction));
                return result;
            }

            statuscode = 0;
            if (paymenttransaction.PaymentTransactionType != PaymentTransactionTypes.decentral_mintandsale_random)
                return result;

            if (paymenttransaction.DecentralParameters.MintNfts == null)
            {
                result.ErrorCode = 1009;
                result.ErrorMessage = "You must submit the mintNfts parameters in the decentral node";
                result.ResultState = ResultStates.Error;
                statuscode = 406;
                GlobalFunctions.LogMessage(db, $"API-Call: Decentral transaction error - {project.Id} - {result.ErrorMessage}", JsonConvert.SerializeObject(paymenttransaction));
                return result;
            }

            if (paymenttransaction.DecentralParameters.MintNfts.ReserveNfts != null &&
                paymenttransaction.DecentralParameters.MintNfts.ReserveNfts.Any())
            {
                result.ErrorCode = 1003;
                result.ErrorMessage = "You can not specify NFTs when you will mint them randomly";
                result.ResultState = ResultStates.Error;
                statuscode = 406;
                GlobalFunctions.LogMessage(db, $"API-Call: Decentral transaction error - {project.Id} - {result.ErrorMessage}", JsonConvert.SerializeObject(paymenttransaction));
                return result;
            }

            if (paymenttransaction.DecentralParameters.MintNfts.CountNfts == 0)
            {
                result.ErrorCode = 1006;
                result.ErrorMessage = "You have to sell at least one nft";
                result.ResultState = ResultStates.Error;
                statuscode = 406;
                GlobalFunctions.LogMessage(db, $"API-Call: Decentral transaction error - {project.Id} - {result.ErrorMessage}", JsonConvert.SerializeObject(paymenttransaction));
                return result;
            }

            if (paymenttransaction.DecentralParameters.MintNfts.CountNfts > 20 && project.Maxsupply==1)
            {
                result.ErrorCode = 1007;
                result.ErrorMessage = "Maximum Count of NFTS is 20";
                result.ResultState = ResultStates.Error;
                statuscode = 406;
                GlobalFunctions.LogMessage(db, $"API-Call: Decentral transaction error - {project.Id} - {result.ErrorMessage}", JsonConvert.SerializeObject(paymenttransaction));
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


            long countnft = 1;
            if (paymenttransaction.DecentralParameters.MintNfts is {CountNfts: { }})
                countnft = (long)paymenttransaction.DecentralParameters.MintNfts.CountNfts;

            // Save price from pricelist to the transaction
            var pricelist = (from a in db.Pricelists
                             where a.NftprojectId == project.Id && a.State == "active" &&
                                   (a.Validfrom == null || a.Validfrom <= DateTime.Now) &&
                                   (a.Validto == null || a.Validto >= DateTime.Now) && a.Countnftortoken ==
                                   countnft
                             select a).AsNoTracking().FirstOrDefault();
            if (pricelist != null)
            {
                preparedpaymenttransaction.Lovelace = GlobalFunctions.GetPriceInEntities(redis, pricelist);
                preparedpaymenttransaction.Countnft = countnft;
                db.SaveChanges();
            }
            else
            {
                result.ErrorCode = 1008;
                result.ErrorMessage = "No price found";
                result.ResultState = ResultStates.Error;
                statuscode = 404;
                return result;
            }

            // Set Promotion Multiplier
            if (project.Maxsupply == 1)
                preparedpaymenttransaction.Promotionmultiplier = (int) countnft;
            else preparedpaymenttransaction.Promotionmultiplier = 1;
            db.SaveChanges();


            if (pricelist.Priceintoken is > 0 && !string.IsNullOrEmpty(pricelist.Tokenpolicyid))
            {
                var multiplier = GlobalFunctions.GetFtTokensMultiplier(pricelist.Tokenpolicyid, pricelist.Assetnamehex?? pricelist.Tokenassetid.ToHex());
                PreparedpaymenttransactionsTokenprice tp = new()
                {
                    Policyid = pricelist.Tokenpolicyid,
                    Assetname = pricelist.Tokenassetid,
                    Assetnamehex=pricelist.Assetnamehex ?? pricelist.Tokenassetid.ToHex(),
                    PreparedpaymenttransactionId = preparedpaymenttransaction.Id,
                    Totalcount = (long) pricelist.Priceintoken,
                    Multiplier = multiplier.Multiplier,
                    Tokencount = Convert.ToInt64(pricelist.Priceintoken/multiplier.Multiplier),
                    Decimals = multiplier.Decimals
                };
                db.Add(tp);
                db.SaveChanges();
            }

            return result;
        }
    }
}
