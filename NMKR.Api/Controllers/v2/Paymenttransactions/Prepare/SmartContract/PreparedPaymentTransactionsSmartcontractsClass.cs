using System.Linq;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.v2.Paymenttransactions.Prepare.SmartContract
{
    public class PreparedPaymentTransactionsSmartcontractsClass : PreparedPaymentTransactionsBaseClass
    {
        public PreparedPaymentTransactionsSmartcontractsClass(EasynftprojectsContext db, IConnectionMultiplexer redis, PaymentTransactionTypes PaymentTransactionType) : base(db, redis, PaymentTransactionType)
        {
        }


        public override ApiErrorResultClass CheckParameter(CreatePaymentTransactionClass paymenttransaction, Nftproject project,
            ApiErrorResultClass result, out int statuscode)
        {
            statuscode = 0;
            bool checkTransactionparmeters = !(paymenttransaction.DirectSaleParameters != null &&
                                               !string.IsNullOrEmpty(paymenttransaction.DirectSaleParameters.TxHashForAlreadyLockedinAssets));


            if (checkTransactionparmeters)
            {
                if (paymenttransaction.TransactionParameters == null || !paymenttransaction.TransactionParameters.Any())
                {
                    result.ErrorCode = 1010;
                    result.ErrorMessage = "You must submit the transaction parameters";
                    result.ResultState = ResultStates.Error;
                    statuscode = 406;
                    return result;
                }

                if (paymenttransaction.TransactionParameters.Length > 1)
                {
                    result.ErrorCode = 1999;
                    result.ErrorMessage = "More than one Token is currently not supported. Coming soon";
                    result.ResultState = ResultStates.Error;
                    statuscode = 406;
                    return result;
                }
                if (string.IsNullOrEmpty(paymenttransaction.TransactionParameters.First().Tokenname))
                {
                    result.ErrorCode = 1011;
                    result.ErrorMessage = "You must submit the tokenname";
                    result.ResultState = ResultStates.Error;
                    statuscode = 406;
                    return result;
                }
                if (string.IsNullOrEmpty(paymenttransaction.TransactionParameters.First().PolicyId))
                {
                    result.ErrorCode = 1011;
                    result.ErrorMessage = "You must submit the policyid";
                    result.ResultState = ResultStates.Error;
                    statuscode = 406;
                    return result;
                }

                if (paymenttransaction.TransactionParameters.First().Tokencount == 0)
                {
                    result.ErrorCode = 1011;
                    result.ErrorMessage = "You must submit the token count";
                    result.ResultState = ResultStates.Error;
                    statuscode = 406;
                    return result;
                }
            }
            else
            {
                if (paymenttransaction.TransactionParameters != null)
                {
                    result.ErrorCode = 1113;
                    result.ErrorMessage = "If you submit a txhash, you must not submit the transaction parameters";
                    result.ResultState = ResultStates.Error;
                    statuscode = 406;
                    return result;
                }

              
            }

            if (paymenttransaction.PaymentgatewayParameters != null)
            {
                result.ErrorCode = 1013;
                result.ErrorMessage = "You must not submit the paymentgateway parameters";
                result.ResultState = ResultStates.Error;
                statuscode = 406;
                return result;
            }
            

            if (paymenttransaction.PaymentTransactionNotifications != null &&
                paymenttransaction.PaymentTransactionNotifications.Any())
            {
                foreach (var notification in paymenttransaction.PaymentTransactionNotifications)
                {
                    if (notification.NotificationType == PaymentTransactionNotificationTypes.email && !notification.NotificationEndpoint.IsValidEmail())
                    {
                        result.ErrorCode = 1012;
                        result.ErrorMessage = "E-Mail Address in notification is not valid";
                        result.InnerErrorMessage = notification.NotificationEndpoint;
                        result.ResultState = ResultStates.Error;
                        statuscode = 406;
                        return result;
                    }
                    if (notification.NotificationType == PaymentTransactionNotificationTypes.webhook && !notification.NotificationEndpoint.IsValidUrl())
                    {
                        result.ErrorCode = 1012;
                        result.ErrorMessage = "URL in notification is not valid";
                        result.InnerErrorMessage = notification.NotificationEndpoint;
                        result.ResultState = ResultStates.Error;
                        statuscode = 406;
                        return result;
                    }
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
                foreach (var transactionParameter in paymenttransaction.TransactionParameters)
                {
                    PreparedpaymenttransactionsNft pptn = new()
                    {
                        Count = transactionParameter.Tokencount,
                        Policyid = transactionParameter.PolicyId,
                        Tokenname = transactionParameter.Tokenname,
                        Tokennamehex = GlobalFunctions.ToHexString(transactionParameter.Tokenname),
                        PreparedpaymenttransactionsId = preparedpaymenttransaction.Id
                    };
                    db.Add(pptn);
                }

                // This can be removed if i changed all to the array
                preparedpaymenttransaction.Tokencount = paymenttransaction.TransactionParameters.First().Tokencount;
                preparedpaymenttransaction.Policyid = paymenttransaction.TransactionParameters.First().PolicyId;
                preparedpaymenttransaction.Tokenname =
                    GlobalFunctions.ToHexString(paymenttransaction.TransactionParameters.First().Tokenname);


                preparedpaymenttransaction.Overridemarketplaceaddress =
                    paymenttransaction.DirectSaleParameters?.OverrideMarkteplaceFeeAddress;
                preparedpaymenttransaction.Overridemarketplacefee =
                    (float?)paymenttransaction.DirectSaleParameters?.OverrideMarketplaceFee;



               

                if (preparedpaymenttransaction.Transactiontype ==
                    nameof(PaymentTransactionTypes.smartcontract_directsale) ||
                    preparedpaymenttransaction.Transactiontype == nameof(PaymentTransactionTypes.smartcontract_auction))
                {
                    preparedpaymenttransaction.Lockamount = 2000000; // TODO: Calculate from the amount of nft
                    preparedpaymenttransaction.Smartcontractstate =
                        nameof(PaymentTransactionSubstates.waitingforlocknft);
                }

                if (preparedpaymenttransaction.Transactiontype ==
                    nameof(PaymentTransactionTypes.smartcontract_directsale_offer))
                {
                    preparedpaymenttransaction.Lockamount = 0;
                    preparedpaymenttransaction.Smartcontractstate =
                        nameof(PaymentTransactionSubstates.waitingforlockada);
                }

                db.SaveChanges();
            }
          
            return result;
        }
    }
}
