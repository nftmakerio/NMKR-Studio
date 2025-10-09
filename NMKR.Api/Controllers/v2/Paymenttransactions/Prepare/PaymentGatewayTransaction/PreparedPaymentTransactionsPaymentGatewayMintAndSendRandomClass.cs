using System.Linq;
using NMKR.Shared.Classes;
using NMKR.Shared.Model;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.v2.Paymenttransactions.Prepare.PaymentGatewayTransaction
{
    public class PreparedPaymentTransactionsPaymentGatewayMintAndSendRandomClass : PreparedPaymentTransactionsPaymentGatewayClass
    {
        public PreparedPaymentTransactionsPaymentGatewayMintAndSendRandomClass(EasynftprojectsContext db, IConnectionMultiplexer redis, PaymentTransactionTypes PaymentTransactionType) : base(db,redis, PaymentTransactionType)
        {
        }

        public override ApiErrorResultClass CheckParameter(CreatePaymentTransactionClass paymenttransaction, Nftproject project, ApiErrorResultClass result, out int statuscode)
        {
            result = base.CheckParameter(paymenttransaction,project, result, out statuscode);
            if (statuscode != 0)
            {
                return result;
            }

            statuscode = 0;
            if (paymenttransaction.PaymentTransactionType != PaymentTransactionTypes.paymentgateway_mintandsend_random)
                return result;

            if (paymenttransaction.PaymentgatewayParameters.MintNfts == null)
            {
                result.ErrorCode = 1009;
                result.ErrorMessage = "You must submit the mintnfts parameters in the paymentgateway node";
                result.ResultState = ResultStates.Error;
                statuscode = 406;
                return result;
            }

            if (paymenttransaction.PaymentgatewayParameters.MintNfts.ReserveNfts != null &&
                paymenttransaction.PaymentgatewayParameters.MintNfts.ReserveNfts.Any())
            {
                result.ErrorCode = 1003;
                result.ErrorMessage = "You can not specify NFTs when you will mint them randomly";
                result.ResultState = ResultStates.Error;
                statuscode = 406;
                return result;
            }

            if (paymenttransaction.PaymentgatewayParameters.MintNfts.CountNfts == 0)
            {
                result.ErrorCode = 1006;
                result.ErrorMessage = "You have to mint at least one nft";
                result.ResultState = ResultStates.Error;
                statuscode = 406;
                return result;
            }

            if (paymenttransaction.PaymentgatewayParameters.MintNfts.CountNfts > 20 && project.Maxsupply == 1)
            {
                result.ErrorCode = 1007;
                result.ErrorMessage = "Maximum Count of NFTS is 20";
                result.ResultState = ResultStates.Error;
                statuscode = 406;
                return result;
            }

            return result;
        }
    }
}
