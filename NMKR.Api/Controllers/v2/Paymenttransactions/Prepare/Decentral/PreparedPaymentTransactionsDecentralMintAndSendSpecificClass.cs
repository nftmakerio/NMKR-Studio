using System.Linq;
using NMKR.Shared.Classes;
using NMKR.Shared.Functions;
using NMKR.Shared.Model;
using StackExchange.Redis;

namespace NMKR.Api.Controllers.v2.Paymenttransactions.Prepare.Decentral
{
    public class PreparedPaymentTransactionsDecentralMintAndSendSpecificClass : PreparedPaymentTransactionsDecentralClass
    {
        public PreparedPaymentTransactionsDecentralMintAndSendSpecificClass(EasynftprojectsContext db, IConnectionMultiplexer redis, PaymentTransactionTypes PaymentTransactionType) : base(db,redis, PaymentTransactionType)
        {
        }
        public override ApiErrorResultClass CheckParameter(CreatePaymentTransactionClass paymenttransaction, Nftproject project, ApiErrorResultClass result, out int statuscode)
        {
            result = base.CheckParameter(paymenttransaction, project, result, out statuscode);
            if (statuscode != 0)
            {
                return result;
            }

            statuscode = 0;
            if (paymenttransaction.PaymentTransactionType != PaymentTransactionTypes.decentral_mintandsend_specific)
                return result;

            if (paymenttransaction.DecentralParameters.MintNfts == null)
            {
                result.ErrorCode = 1006;
                result.ErrorMessage = "You must specify NFTs when you will mint them specific";
                result.ResultState = ResultStates.Error;
                statuscode = 406;
                return result;
            }

            if (paymenttransaction.DecentralParameters.MintNfts.ReserveNfts == null ||
                !paymenttransaction.DecentralParameters.MintNfts.ReserveNfts.Any())
            {
                result.ErrorCode = 1004;
                result.ErrorMessage = "You must specify NFTs when you will mint them specific";
                result.ResultState = ResultStates.Error;
                statuscode = 406;
                return result;
            }

            foreach (var mintNftsReserveNft in paymenttransaction.DecentralParameters.MintNfts.ReserveNfts)
            {
                if (mintNftsReserveNft.Tokencount == 0)
                {
                    result.ErrorCode = 1007;
                    result.ErrorMessage = "Specify tokencount in the reservenfts";
                    result.ResultState = ResultStates.Error;
                    statuscode = 406;
                    return result;
                }

                if (string.IsNullOrEmpty(mintNftsReserveNft.NftUid))
                {
                    result.ErrorCode = 1008;
                    result.ErrorMessage = "Specify nft uid in the reservenfts";
                    result.ResultState = ResultStates.Error;
                    statuscode = 406;
                    return result;
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


            foreach (var ptcReserveSpecificNft in paymenttransaction.DecentralParameters.MintNfts.ReserveNfts)
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
                    Count = ptcReserveSpecificNft.Tokencount??1,// * Math.Max(1, project.Multiplier),
                    NftId = nftid.Id,
                    Tokennamehex = GlobalFunctions.ToHexString(project + nftid.Name),
                    Policyid = nftid.Policyid,
                    Tokenname = project.Tokennameprefix + nftid.Name,
                    PreparedpaymenttransactionsId = preparedpaymenttransaction.Id,
                    Lovelace = 0,
                    Nftuid = ptcReserveSpecificNft.NftUid
                };
                db.Add(reserve);
                db.SaveChanges();
            }

            long countnft = paymenttransaction.DecentralParameters.MintNfts.ReserveNfts.Length;
            var costs = GlobalFunctions.GetMintingcosts2(project.Id, countnft,preparedpaymenttransaction.Lovelace??0).Costs;

            preparedpaymenttransaction.Lovelace = costs;

            if (paymenttransaction.DecentralParameters.CreateRoyaltyTokenIfNotExists != null && project.Hasroyality == false)
            {
                preparedpaymenttransaction.Lovelace += 2000000;
            }


            // Set Promotion Multiplier
            preparedpaymenttransaction.Promotionmultiplier = paymenttransaction.DecentralParameters.MintNfts.ReserveNfts.Length;


            db.SaveChanges();

            return result;
        }
    }
}
