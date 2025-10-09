using System;
using System.Linq;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.v2.Paymenttransactions.Prepare
{
    public abstract class PreparedPaymentTransactionsBaseClass
    {
        public PaymentTransactionTypes PaymentTransactionType { get; set; }
        public EasynftprojectsContext db { get; set; }
        public IConnectionMultiplexer redis { get; set; }
        protected PreparedPaymentTransactionsBaseClass(EasynftprojectsContext dB,IConnectionMultiplexer Redis, PaymentTransactionTypes paymentTransactionType)
        {
            PaymentTransactionType = paymentTransactionType;
            db = dB;
            redis=Redis;
        }

        /// <summary>
        /// Checks the Parameters from the incoming JSON
        /// </summary>
        /// <param name="paymenttransaction"></param>
        /// <param name="result"></param>
        /// <param name="statuscode"></param>
        /// <returns></returns>
        public abstract ApiErrorResultClass CheckParameter(CreatePaymentTransactionClass paymenttransaction, Nftproject project,
            ApiErrorResultClass result, out int statuscode);

        /// <summary>
        /// Save the Base Transaction
        /// </summary>
        /// <param name="paymenttransaction"></param>
        /// <param name="result"></param>
        /// <param name="project"></param>
        /// <param name="preparedpaymenttransaction"></param>
        /// <param name="statuscode"></param>
        /// <returns></returns>
        public virtual ApiErrorResultClass SaveTransaction(CreatePaymentTransactionClass paymenttransaction, ApiErrorResultClass result, Nftproject project, out Preparedpaymenttransaction preparedpaymenttransaction, out int statuscode)
        {
            Preparedpaymenttransaction? rpptx = null;
            if (!string.IsNullOrEmpty(paymenttransaction.ReferencedPaymenttransactionUid))
            {
                 rpptx = (from a in db.Preparedpaymenttransactions
                    where a.Transactionuid == paymenttransaction.ReferencedPaymenttransactionUid
                    select a).FirstOrDefault();
            }

            statuscode = 0;
            preparedpaymenttransaction = new()
            {
                NftprojectId = project.Id,
                State = nameof(PaymentTransactionsStates.prepared),
                Created = DateTime.Now,
                Transactionuid = "T" + GlobalFunctions.GetGuid(),
                Transactiontype = paymenttransaction.PaymentTransactionType.ToString(),
                Countnft = paymenttransaction.PaymentgatewayParameters?.MintNfts?.CountNfts ?? 1,
                Customeripaddress = paymenttransaction.CustomerIpAddress,
                Referer = paymenttransaction.Referer,
                Optionalreceiveraddress = rpptx?.Optionalreceiveraddress
            };

            db.Preparedpaymenttransactions.Add(preparedpaymenttransaction);
            db.SaveChanges();

            if (!string.IsNullOrEmpty(paymenttransaction.ReferencedPaymenttransactionUid))
            {
                if (rpptx != null)
                {
                    rpptx.ReferencedprepearedtransactionId=preparedpaymenttransaction.Id;
                    db.SaveChanges();
                }
                else
                {
                    result.ErrorCode = 5201;
                    result.ErrorMessage = "Referenced payment transaction uid is not valid";
                    result.ResultState = ResultStates.Error;
                    statuscode = 406;
                    return result;
                }
            }



            // Save Custom Properties
            if (paymenttransaction.CustomProperties != null)
            {
                foreach (var ptcCustomProperty in paymenttransaction.CustomProperties)
                {
                    var custom = new PreparedpaymenttransactionsCustomproperty()
                    {
                        PreparedpaymenttransactionsId = preparedpaymenttransaction.Id,
                        Key = ptcCustomProperty.Key,
                        Value = ptcCustomProperty.Value
                    };
                    db.Add(custom);
                    db.SaveChanges();
                }
            }

            // Save Notifications
            if (paymenttransaction.PaymentTransactionNotifications != null)
            {
                foreach (var notificationsClass in paymenttransaction.PaymentTransactionNotifications)
                {
                    var notification = new PreparedpaymenttransactionsNotification()
                    {
                        PreparedpaymenttransactionsId = preparedpaymenttransaction.Id,
                        Notificationtype = notificationsClass.NotificationType.ToString(),
                        Notificationendpoint = notificationsClass.NotificationEndpoint??"",
                        Secret = notificationsClass.HMACSecret??""
                    };
                    db.Add(notification);
                    db.SaveChanges();
                }
            }


            return result;
        }
    }
}
