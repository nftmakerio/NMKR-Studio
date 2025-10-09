using System.Linq;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.v2.Paymenttransactions.Prepare.Decentral
{
    public class PreparedPaymentTransactionsDecentralMintAndSendRandomClass : PreparedPaymentTransactionsDecentralClass
    {
        public PreparedPaymentTransactionsDecentralMintAndSendRandomClass(EasynftprojectsContext db, IConnectionMultiplexer redis,
            PaymentTransactionTypes PaymentTransactionType) : base(db,redis, PaymentTransactionType)
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
            if (paymenttransaction.PaymentTransactionType != PaymentTransactionTypes.decentral_mintandsend_random)
                return result;

            if (paymenttransaction.DecentralParameters.MintNfts == null)
            {
                result.ErrorCode = 1009;
                result.ErrorMessage = "You must submit the mintNfts parameters in the decentral node";
                result.ResultState = ResultStates.Error;
                statuscode = 406;
                return result;
            }

            if (paymenttransaction.DecentralParameters.MintNfts.ReserveNfts != null &&
                paymenttransaction.DecentralParameters.MintNfts.ReserveNfts.Any())
            {
                result.ErrorCode = 1003;
                result.ErrorMessage = "You can not specify NFTs when you will mint them randomly";
                result.ResultState = ResultStates.Error;
                statuscode = 406;
                return result;
            }

            if (paymenttransaction.DecentralParameters.MintNfts.CountNfts == 0)
            {
                result.ErrorCode = 1006;
                result.ErrorMessage = "You have to sell at least one nft";
                result.ResultState = ResultStates.Error;
                statuscode = 406;
                return result;
            }

            if (paymenttransaction.DecentralParameters.MintNfts.CountNfts > 20 && project.Maxsupply == 1)
            {
                result.ErrorCode = 1007;
                result.ErrorMessage = "Maximum Count of NFTS is 20";
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

            long countnft = 1;
            if (paymenttransaction.DecentralParameters.MintNfts != null &&
                paymenttransaction.DecentralParameters.MintNfts.CountNfts != null)
                countnft = (long)paymenttransaction.DecentralParameters.MintNfts.CountNfts;


            // Set Promotion Multiplier
            if (project.Maxsupply == 1)
                preparedpaymenttransaction.Promotionmultiplier = (int)countnft;
            else preparedpaymenttransaction.Promotionmultiplier = 1;



            var costs =GlobalFunctions.GetMintingcosts2(project.Id, countnft, preparedpaymenttransaction.Lovelace??0).Costs;
            preparedpaymenttransaction.Countnft = countnft;
            preparedpaymenttransaction.Lovelace = costs;
            db.SaveChanges();

            return result;
        }
    }
}
