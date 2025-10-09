using System;
using System.Linq;
using NMKR.Shared.Classes;
using NMKR.Shared.Model;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.v2.Paymenttransactions.Prepare.SmartContract
{
    public class PreparedPaymentTransactionsSmartcontractsAuctionClass : PreparedPaymentTransactionsSmartcontractsClass
    {
        public PreparedPaymentTransactionsSmartcontractsAuctionClass(EasynftprojectsContext db, IConnectionMultiplexer redis, PaymentTransactionTypes PaymentTransactionType) : base(db,redis, PaymentTransactionType)
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
            if (paymenttransaction.PaymentTransactionType != PaymentTransactionTypes.smartcontract_auction)
                return result;

            if (paymenttransaction.AuctionParameters == null)
            {
                result.ErrorCode = 1009;
                result.ErrorMessage = "You must submit the auction parameters";
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
            if (paymenttransaction.DirectSaleParameters != null)
            {
                result.ErrorCode = 1015;
                result.ErrorMessage = "You must not submit the directsale parameters";
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


            var smartcontractscript = (from a in db.Smartcontracts
                where a.Type == "auction" && a.State == "active"
                select a).FirstOrDefault();

            if (paymenttransaction.AuctionParameters != null)
            {
                preparedpaymenttransaction.Auctionduration = paymenttransaction.AuctionParameters.DurationInSeconds;
                preparedpaymenttransaction.Auctionminprice = paymenttransaction.AuctionParameters.MinBet;
                preparedpaymenttransaction.SmartcontractsId = smartcontractscript?.Id;
                preparedpaymenttransaction.Expires =
                    DateTime.Now.AddSeconds(paymenttransaction.AuctionParameters.DurationInSeconds);
                db.SaveChanges();
            }

            return result;
        }
    }
}
