using System.Threading.Tasks;
using NMKR.Shared.Blockchains;
using NMKR.Shared.Blockchains.APTOS;
using NMKR.Shared.Blockchains.Solana;
using NMKR.Shared.Classes;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions.Metadata;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.Extensions;
using StackExchange.Redis;
using NMKR.Shared.Model;

namespace NMKR.Api.Controllers.SharedClasses
{
    internal static class GetNftDetailsClass
    {
        internal static NftDetailsClass GetNftDetails(EasynftprojectsContext db, IConnectionMultiplexer redis, Nft nftx)
        {
            var res = Task.Run(async () => await GetNftDetailsAsync(db,redis,nftx));
            return res.Result;
        }
        internal static async Task<NftDetailsClass> GetNftDetailsAsync(EasynftprojectsContext db, IConnectionMultiplexer redis, Nft nftx)
        {
            GetMetadataClass gm = new(nftx.Id, "", true);
            string paylink = GeneralConfigurationClass.Paywindowlink + "p=" + nftx.Nftproject.Uid.Replace("-", "") + "&n=" + nftx.Uid.Replace("-", "");
            var price = GlobalFunctions.GetPrice(db, redis, nftx, 1);
            long? sendback =price==null?null: GlobalFunctions.CalculateSendbackToUser(db, redis, 1, nftx.NftprojectId);

            var pricesolana = GlobalFunctions.GetPrice(db, redis, nftx, 1, Coin.SOL);
            var priceaptos = GlobalFunctions.GetPrice(db, redis, nftx, 1, Coin.APT);


            string metadata = (await gm.MetadataResultAsync()).Metadata;

            IBlockchainFunctions blockchain = null;
            switch (nftx.Mintedonblockchain.ToEnum<Blockchain>())
            {
                case Blockchain.Aptos:
                    blockchain = new AptosBlockchainFunctions();
                    metadata = blockchain.GetMetadataFromCip25Metadata(metadata, nftx.Nftproject);
                    break;
                case Blockchain.Solana:
                    blockchain = new SolanaBlockchainFunctions();
                    metadata = blockchain.GetMetadataFromCip25Metadata(metadata, nftx.Nftproject);
                    break;
            }

            var nft = new NftDetailsClass()
            {
                Id = nftx.Id,
                Minted = nftx.Minted,
                Name = nftx.Name,
                Displayname = nftx.Displayname,
                Detaildata = nftx.Detaildata,
                State = nftx.State,
                Selldate = nftx.Selldate,
                Title = nftx.Title,
                Assetid = nftx.Mintedonblockchain == Blockchain.Cardano.ToString() ? string.IsNullOrEmpty(nftx.Assetid) ? GlobalFunctions.GetAssetId(nftx.Nftproject.Policyid, nftx.Nftproject.Tokennameprefix, nftx.Name) : nftx.Assetid : null,
                Assetname = string.IsNullOrEmpty(nftx.Assetname) ? GlobalFunctions.ToHexString(nftx.Nftproject.Tokennameprefix + nftx.Name) : nftx.Assetname,
                Fingerprint = nftx.Fingerprint,
                Initialminttxhash = nftx.Initialminttxhash,
                Ipfshash = nftx.Ipfshash,
                Policyid = nftx.Mintedonblockchain==Blockchain.Cardano.ToString()? nftx.Policyid : null,
                SinglePrice = nftx.Price == null ? null : price,
                SinglePriceSolana = nftx.Pricesolana == null ? null : pricesolana,
                SendBackCentralPaymentInLovelace = sendback,
                PriceInLovelaceCentralPayments = nftx.Nftproject.Enabledcoins.Contains(Coin.ADA.ToString()) == false ? null : nftx.Nftproject.Enabledecentralpayments ? price + sendback : price,
                PriceInLamportCentralPayments = nftx.Nftproject.Enabledcoins.Contains(Coin.SOL.ToString()) == false ? null : pricesolana, 
                PriceInOctsCentralPayments = nftx.Nftproject.Enabledcoins.Contains(Coin.APT.ToString()) == false ? null : priceaptos,
                Receiveraddress = nftx.Receiveraddress,
                Reserveduntil = nftx.Reserveduntil,
                Soldby = nftx.Soldby,
                Series = nftx.Series,
                IpfsGatewayAddress = GeneralConfigurationClass.IPFSGateway + nftx.Ipfshash,
                Metadata = metadata,
                Uid = nftx.Uid,
                PaymentGatewayLinkForSpecificSale = paylink,
                UploadSource = nftx.Uploadsource,
                MintedOnBlockchain=nftx.Mintedonblockchain.ToEnum<Blockchain>(),
                Mintingfees = nftx.Mintingfees,
            };
            return nft;
        }
    }
}
