using System.Linq;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.v2.Paymenttransactions.Prepare.Decentral
{
    public class PreparedPaymentTransactionsDecentralClass : PreparedPaymentTransactionsBaseClass
    {
        protected PreparedPaymentTransactionsDecentralClass(EasynftprojectsContext db, IConnectionMultiplexer redis, PaymentTransactionTypes PaymentTransactionType) : base(db,redis, PaymentTransactionType)
        {
        }

        public override ApiErrorResultClass CheckParameter(CreatePaymentTransactionClass paymenttransaction, Nftproject project, ApiErrorResultClass result, out int statuscode)
        {
            statuscode = 0;
            if (paymenttransaction.PaymentTransactionType != PaymentTransactionTypes.decentral_mintandsend_specific && paymenttransaction.PaymentTransactionType != PaymentTransactionTypes.decentral_mintandsend_random
                && paymenttransaction.PaymentTransactionType != PaymentTransactionTypes.decentral_mintandsale_random && paymenttransaction.PaymentTransactionType != PaymentTransactionTypes.decentral_mintandsale_specific)
                return result;

            if (paymenttransaction.DecentralParameters == null)
            {
                result.ErrorCode = 1009;
                result.ErrorMessage = "You must submit the decentral parameters";
                result.ResultState = ResultStates.Error;
                statuscode = 406;
                GlobalFunctions.LogMessage(db, $"API-Call: Decentral transaction error - {project.Id} - {result.ErrorMessage}", JsonConvert.SerializeObject(paymenttransaction));
                return result;
            }
            if (paymenttransaction.PaymentgatewayParameters != null)
            {
                result.ErrorCode = 1010;
                result.ErrorMessage = "You must NOT submit the paymentgateway parameters";
                result.ResultState = ResultStates.Error;
                statuscode = 406;
                GlobalFunctions.LogMessage(db, $"API-Call: Decentral transaction error - {project.Id} - {result.ErrorMessage}", JsonConvert.SerializeObject(paymenttransaction));
                return result;
            }

            if (project.Enabledecentralpayments == false)
            {
                result.ErrorCode = 1011;
                result.ErrorMessage = "Multisig payments are not enabled on this project";
                result.ResultState = ResultStates.Error;
                statuscode = 406;
                GlobalFunctions.LogMessage(db, $"API-Call: Decentral transaction error - {project.Id} - {result.ErrorMessage}", JsonConvert.SerializeObject(paymenttransaction));
                return result;
            }

            if (paymenttransaction.DecentralParameters.CreateRoyaltyTokenIfNotExists != null)
            {
                if (string.IsNullOrEmpty(paymenttransaction.DecentralParameters.CreateRoyaltyTokenIfNotExists.Address))
                {
                    result.ErrorCode = 1012;
                    result.ErrorMessage = "If you want to create a royalty token, provide the royalty address";
                    result.ResultState = ResultStates.Error;
                    statuscode = 406;
                    GlobalFunctions.LogMessage(db, $"API-Call: Decentral transaction error - {project.Id} - {result.ErrorMessage}", JsonConvert.SerializeObject(paymenttransaction));
                    return result;
                }
                if (paymenttransaction.DecentralParameters.CreateRoyaltyTokenIfNotExists.Percentage<1f || paymenttransaction.DecentralParameters.CreateRoyaltyTokenIfNotExists.Percentage > 100f)
                {
                    result.ErrorCode = 1013;
                    result.ErrorMessage = "Royalty token percentage out of range";
                    result.ResultState = ResultStates.Error;
                    GlobalFunctions.LogMessage(db, $"API-Call: Decentral transaction error - {project.Id} - {result.ErrorMessage}", JsonConvert.SerializeObject(paymenttransaction));
                    statuscode = 406;
                    return result;
                }
            }
            if (!GlobalFunctions.CheckExpirationSlot(project))
            {
                result.ErrorCode = 205;
                result.ErrorMessage = "Policy is already locked. No further minting possible (6)";
                result.ResultState = ResultStates.Error;
                statuscode = 404;
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

            if (result.ResultState == ResultStates.Error)
                return result;

            preparedpaymenttransaction.Paymentgatewaystate = nameof(PaymentGatewayStates.prepared);
            preparedpaymenttransaction.Smartcontractstate = nameof(PaymentTransactionSubstates.waitingforsale);

            if (project.CustomerwalletId != null)
            {
                var wallet = (from a in db.Customerwallets
                    where a.Id == project.CustomerwalletId && a.State=="active"
                    select a).FirstOrDefault();
                if (wallet != null)
                    preparedpaymenttransaction.Selleraddress = wallet.Walletaddress;
            }
            

            // Set Promotion Id
            preparedpaymenttransaction.PromotionId = project.DefaultpromotionId;
            

            // Create the roalty token, if not already exists
            if (paymenttransaction.DecentralParameters.CreateRoyaltyTokenIfNotExists != null && project.Hasroyality == false)
            { 
                    preparedpaymenttransaction.Createroyaltytokenaddress =
                        paymenttransaction.DecentralParameters.CreateRoyaltyTokenIfNotExists.Address;
                    preparedpaymenttransaction.Createroyaltytokenpercentage =
                        paymenttransaction.DecentralParameters.CreateRoyaltyTokenIfNotExists.Percentage;
            }
            preparedpaymenttransaction.Optionalreceiveraddress = paymenttransaction.DecentralParameters.OptionalRecevierAddress;
            db.SaveChanges();


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
