using System;
using System.Linq;
using NMKR.Api.Controllers.v2.Paymenttransactions.Prepare.SmartContract;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.Koios;
using NMKR.Shared.Model;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.v2.Paymenttransactions.Prepare.Legacy
{
    public class PreparedPaymentTransactionsLegacyDirectsaleClass : PreparedPaymentTransactionsSmartcontractsClass
    {
        public PreparedPaymentTransactionsLegacyDirectsaleClass(EasynftprojectsContext db, IConnectionMultiplexer redis, PaymentTransactionTypes PaymentTransactionType) : base(db,redis, PaymentTransactionType)
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
            if (paymenttransaction.PaymentTransactionType != PaymentTransactionTypes.legacy_directsale)
                return result;

            if (paymenttransaction.AuctionParameters != null)
            {
                result.ErrorCode = 1009;
                result.ErrorMessage = "You must NOT submit the auction parameters";
                result.ResultState = ResultStates.Error;
                statuscode = 406;
                return result;
            }
            if (paymenttransaction.DirectSaleParameters == null)
            {
                result.ErrorCode = 1015;
                result.ErrorMessage = "You must submit the directsale parameters";
                result.ResultState = ResultStates.Error;
                statuscode = 406;
                return result;
            }

            if (paymenttransaction.PaymentgatewayParameters != null)
            {
                result.ErrorCode = 1013;
                result.ErrorMessage = "You must NOT submit the paymentgatway parameters";
                result.ResultState = ResultStates.Error;
                statuscode = 406;
                return result;
            }
            if (paymenttransaction.TransactionParameters == null || !paymenttransaction.TransactionParameters.Any())
            {
                result.ErrorCode = 1010;
                result.ErrorMessage = "You must submit the transaction parameters";
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


            if (paymenttransaction.DirectSaleParameters != null)
            {
                preparedpaymenttransaction.Lovelace = paymenttransaction.DirectSaleParameters.PriceInLovelace;
                preparedpaymenttransaction.Lockamount = 2000000;
                db.SaveChanges();
            }

            result = CreateLegacyDirectsale(paymenttransaction, preparedpaymenttransaction, result, out statuscode);
            if (result.ResultState == ResultStates.Error)
                return result;


            return result;
        }


        private ApiErrorResultClass CreateLegacyDirectsale(CreatePaymentTransactionClass paymenttransaction, Preparedpaymenttransaction preparedpaymenttransaction, ApiErrorResultClass result, out int statuscode)
        {
            statuscode = 0;

            var newaddress = ConsoleCommand.CreateNewPaymentAddress(GlobalFunctions.IsMainnet());
            if (newaddress.ErrorCode != 0)
            {
                statuscode = 500;
                result.ErrorCode = newaddress.ErrorCode;
                result.ErrorMessage = "Con not create a payment address";
                result.ResultState = ResultStates.Error;
                return result;
            }


            var salt = GlobalFunctions.GetGuid();
            var royalty = KoiosFunctions.GetRoyaltiesFromPolicyId(paymenttransaction.TransactionParameters.First().PolicyId);
            Legacydirectsale la = new()
            {
                Address = newaddress.Address,
                Created = DateTime.Now,
                Price = paymenttransaction.DirectSaleParameters.PriceInLovelace,
                State = "notactive",
                Salt = salt,
                Skey = Encryption.EncryptString(newaddress.privateskey, salt + GeneralConfigurationClass.Masterpassword),
                Vkey = Encryption.EncryptString(newaddress.privatevkey, salt + GeneralConfigurationClass.Masterpassword),
                Marketplacefeepercent = preparedpaymenttransaction.Nftproject.Smartcontractssettings.Percentage,
                Royaltyaddress = royalty?.Address,
                Royaltyfeespercent = royalty?.Percentage,
                NftprojectId = preparedpaymenttransaction.NftprojectId,
            };

            db.Legacydirectsales.Add(la);
            db.SaveChanges();

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

            // TODO: This must be changed to a array version
            LegacydirectsalesNft lan = new()
            {
                LegacydirectsaleId = la.Id,
                Policyid = paymenttransaction.TransactionParameters.First().PolicyId,
                Tokennamehex = paymenttransaction.TransactionParameters.First().Tokenname,
                Tokencount = paymenttransaction.TransactionParameters.First().Tokencount,
                Ipfshash = ""
            };
            db.LegacydirectsalesNfts.Add(lan);


            preparedpaymenttransaction.LegacydirectsalesId = la.Id;
            db.SaveChanges();

            return result;
        }


    }
}
