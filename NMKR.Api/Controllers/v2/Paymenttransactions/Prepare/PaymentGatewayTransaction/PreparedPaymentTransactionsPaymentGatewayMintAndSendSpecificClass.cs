using System.Linq;
using NMKR.Shared.Classes;
using NMKR.Shared.Model;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.v2.Paymenttransactions.Prepare.PaymentGatewayTransaction
{
    /// <summary>
    /// Checks all parameters for the specific mint and send
    /// </summary>
    public class PreparedPaymentTransactionsPaymentGatewayMintAndSendSpecificClass : PreparedPaymentTransactionsPaymentGatewayClass
    {
        /// <summary>
        /// Checks all parameters for the specific mint and send
        /// </summary>
        /// <param name="db"></param>
        /// <param name="PaymentTransactionType"></param>
        public PreparedPaymentTransactionsPaymentGatewayMintAndSendSpecificClass(EasynftprojectsContext db, IConnectionMultiplexer redis, PaymentTransactionTypes PaymentTransactionType) : base(db,redis, PaymentTransactionType)
        {
        }

        /// <summary>
        /// Checks all parameters for the specific mint and send
        /// </summary>
        /// <param name="paymenttransaction"></param>
        /// <param name="result"></param>
        /// <param name="statuscode"></param>
        /// <returns></returns>
        public override ApiErrorResultClass CheckParameter(CreatePaymentTransactionClass paymenttransaction, Nftproject project, ApiErrorResultClass result, out int statuscode)
        {
            result = base.CheckParameter(paymenttransaction,project, result, out statuscode);
            if (statuscode != 0)
            {
                return result;
            }

            statuscode = 0;
            if (paymenttransaction.PaymentTransactionType != PaymentTransactionTypes.paymentgateway_mintandsend_specific)
                return result;

            if (paymenttransaction.PaymentgatewayParameters.MintNfts == null)
            {
                result.ErrorCode = 1006;
                result.ErrorMessage = "You must specify NFTs when you will mint them specific";
                result.ResultState = ResultStates.Error;
                statuscode = 406;
                return result;
            }

            if (paymenttransaction.PaymentgatewayParameters.MintNfts.ReserveNfts == null ||
                !paymenttransaction.PaymentgatewayParameters.MintNfts.ReserveNfts.Any())
            {
                result.ErrorCode = 1004;
                result.ErrorMessage = "You must specify NFTs when you will mint them specific";
                result.ResultState = ResultStates.Error;
                statuscode = 406;
                return result;
            }

            return result;
        }

        /// <summary>
        /// Saves the specific mint and send transaction parameters
        /// </summary>
        /// <param name="paymenttransaction"></param>
        /// <param name="result"></param>
        /// <param name="project"></param>
        /// <param name="preparedpaymenttransaction"></param>
        /// <param name="statuscode"></param>
        /// <returns></returns>
        public override ApiErrorResultClass SaveTransaction(CreatePaymentTransactionClass paymenttransaction,
            ApiErrorResultClass result, Nftproject project, out Preparedpaymenttransaction preparedpaymenttransaction,
            out int statuscode)
        {
            result = base.SaveTransaction(paymenttransaction, result, project, out preparedpaymenttransaction,
                out statuscode);

            if (result.ResultState == ResultStates.Error)
                return result;

            if (paymenttransaction.PaymentgatewayParameters.MintNfts.ReserveNfts != null)
            {
                foreach (var ptcReserveSpecificNft in paymenttransaction.PaymentgatewayParameters.MintNfts.ReserveNfts)
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
                        return result;
                    }

                    var reserve = new PreparedpaymenttransactionsNft()
                    {
                        Count = ptcReserveSpecificNft.Tokencount??1,
                        NftId = nftid.Id,
                        PreparedpaymenttransactionsId = preparedpaymenttransaction.Id,
                        Lovelace = 0,
                        Nftuid = ptcReserveSpecificNft.NftUid
                    };
                    db.Add(reserve);
                    db.SaveChanges();
                }
            }

            preparedpaymenttransaction.Lovelace = 0;

            db.SaveChanges();


            return result;
        }

    }
}

