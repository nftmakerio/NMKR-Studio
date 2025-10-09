using System.Linq;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.v2.Paymenttransactions.Prepare.PaymentGatewayTransaction
{
    public abstract class PreparedPaymentTransactionsPaymentGatewayClass : PreparedPaymentTransactionsBaseClass
    {
        protected PreparedPaymentTransactionsPaymentGatewayClass(EasynftprojectsContext db, IConnectionMultiplexer redis, PaymentTransactionTypes PaymentTransactionType) : base(db,redis, PaymentTransactionType)
        {
        }

        /// <summary>
        /// Checks the Parameters from the incoming JSON
        /// </summary>
        /// <param name="paymenttransaction"></param>
        /// <param name="result"></param>
        /// <param name="statuscode"></param>
        /// <returns></returns>
        public override ApiErrorResultClass CheckParameter(CreatePaymentTransactionClass paymenttransaction, Nftproject project, ApiErrorResultClass result, out int statuscode)
        {
            statuscode = 0;
            if (paymenttransaction.PaymentTransactionType != PaymentTransactionTypes.paymentgateway_nft_random && paymenttransaction.PaymentTransactionType != PaymentTransactionTypes.paymentgateway_nft_specific)
                return result;

            if (paymenttransaction.PaymentgatewayParameters == null)
            {
                result.ErrorCode = 1009;
                result.ErrorMessage = "You must submit the paymentgateway parameters";
                result.ResultState = ResultStates.Error;
                statuscode = 406;
                return result;
            }
            if (paymenttransaction.DecentralParameters != null)
            {
                result.ErrorCode = 1009;
                result.ErrorMessage = "You must NOT submit the decentral parameters";
                result.ResultState = ResultStates.Error;
                statuscode = 406;
                return result;
            }
            if (paymenttransaction.AuctionParameters != null)
            {
                result.ErrorCode = 1009;
                result.ErrorMessage = "You must NOT submit the auction parameters";
                result.ResultState = ResultStates.Error;
                statuscode = 406;
                return result;
            }
            if (paymenttransaction.DirectSaleParameters != null)
            {
                result.ErrorCode = 1009;
                result.ErrorMessage = "You must NOT submit the directsale parameters";
                result.ResultState = ResultStates.Error;
                statuscode = 406;
                return result;
            }
            if (!GlobalFunctions.CheckExpirationSlot(project))
            {
                result.ErrorCode = 205;
                result.ErrorMessage = "Policy is already locked. No further minting possible (8(";
                result.ResultState = ResultStates.Error;
                statuscode = 404;
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

            preparedpaymenttransaction.Paymentgatewaystate = nameof(PaymentGatewayStates.prepared);
            preparedpaymenttransaction.Lovelace =
                GlobalFunctions.GetPriceFromProjectId(db,redis, preparedpaymenttransaction.NftprojectId, paymenttransaction.PaymentgatewayParameters?.MintNfts?.CountNfts ?? 1);

            if (paymenttransaction.PaymentgatewayParameters != null)
                preparedpaymenttransaction.Optionalreceiveraddress =
                    paymenttransaction.PaymentgatewayParameters.OptionalRecevierAddress;


            var rpptx = (from a in db.Preparedpaymenttransactions
                where a.Transactionuid == paymenttransaction.ReferencedPaymenttransactionUid
                select a).FirstOrDefault();

            if (rpptx != null)
            {
                preparedpaymenttransaction.Optionalreceiveraddress = rpptx.Optionalreceiveraddress;
            }


            db.SaveChanges();

            return result;
        }

    }
}
