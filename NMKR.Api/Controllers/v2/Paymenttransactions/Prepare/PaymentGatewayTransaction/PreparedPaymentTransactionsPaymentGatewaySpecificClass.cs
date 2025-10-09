using System.Linq;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.v2.Paymenttransactions.Prepare.PaymentGatewayTransaction
{
    public class PreparedPaymentTransactionsPaymentGatewaySpecificClass : PreparedPaymentTransactionsPaymentGatewayClass
    {
        public PreparedPaymentTransactionsPaymentGatewaySpecificClass(EasynftprojectsContext db, IConnectionMultiplexer redis, PaymentTransactionTypes PaymentTransactionType) : base(db,redis, PaymentTransactionType)
        {
        }

        public override ApiErrorResultClass CheckParameter(CreatePaymentTransactionClass paymenttransaction, Nftproject project, ApiErrorResultClass result, out int statuscode)
        {
            result = base.CheckParameter(paymenttransaction,project, result, out statuscode);
            if (statuscode != 0)
            {
                return result;
            }

            if (project.Disablespecificsales)
            {
                LogClass.LogMessage(db, "API-CALL: ERROR: Specific sales not enabled ");
                result.ErrorCode = 4502;
                result.ErrorMessage = "Specific sales are not enabled on this project";
                result.ResultState = ResultStates.Error;
                statuscode = 406;
                return result;
            }

            statuscode = 0;
            if (paymenttransaction.PaymentTransactionType != PaymentTransactionTypes.paymentgateway_nft_specific)
                return result;

            if (paymenttransaction.PaymentgatewayParameters.MintNfts == null)
            {
                result.ErrorCode = 1006;
                result.ErrorMessage = "You must specify NFTs when you will sell them specific";
                result.ResultState = ResultStates.Error;
                statuscode = 406;
                return result;
            }

            if (paymenttransaction.PaymentgatewayParameters.MintNfts.ReserveNfts == null ||
                !paymenttransaction.PaymentgatewayParameters.MintNfts.ReserveNfts.Any())
            {
                result.ErrorCode = 1004;
                result.ErrorMessage = "You must specify NFTs when you will sell them specific";
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


            long lovelace = 0;
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


                long? l = 0;
                l = ptcReserveSpecificNft.Lovelace ?? GlobalFunctions.GetPrice(db,redis, nftid, ptcReserveSpecificNft.Tokencount ?? 1);

                if (l != null)
                    lovelace += ((long) l);
                
                string tokenname = string.IsNullOrEmpty(project.Tokennameprefix) ? nftid.Name : project.Tokennameprefix + nftid.Name;
                var reserve = new PreparedpaymenttransactionsNft()
                {
                    Count = ptcReserveSpecificNft.Tokencount ?? 1,
                    NftId = nftid.Id,
                    PreparedpaymenttransactionsId = preparedpaymenttransaction.Id,
                    Lovelace = l,
                    Nftuid = ptcReserveSpecificNft.NftUid,
                    Tokenname = tokenname,
                    Tokennamehex = tokenname.ToHex(),
                };
                db.Add(reserve);
                db.SaveChanges();
            }

            preparedpaymenttransaction.Lovelace = lovelace;

            db.SaveChanges();


            return result;
        }

    }
}
