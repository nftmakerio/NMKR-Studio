using System;
using System.Threading.Tasks;
using Solnet.Metaplex.NFT.Library;
using Solnet.Rpc;
using Solnet.Rpc.Types;
using Solnet.Wallet;

// ReSharper disable once CheckNamespace

namespace Solnet.SDK.Nft
{
    [Serializable]
    public class SolanaNftImage
    {
        public string name { get; set; }
        public string extension { get; set; }
        public string externalUrl { get; set; }
        public string file { get; set; }
    }

    [Serializable]
    public class SolanaNft
    {
        public Metaplex metaplexData;

        public SolanaNft() { }

        public SolanaNft(Metaplex metaplexData)
        {
            this.metaplexData = metaplexData;
        }

        /// <summary>
        /// Returns all data for listed nft
        /// </summary>
        /// <param name="mint"></param>
        /// <param name="connection">Rpc client</param>
        ///         /// <param name="loadTexture"></param>

        /// <param name="imageHeightAndWidth"></param>
        /// <param name="tryUseLocalContent">If use local content for image</param>
        /// <param name="commitment"></param>
        /// <returns></returns>
        public static async Task<SolanaNft> TryGetNftData(
            string mint,
            IRpcClient connection,
            bool loadTexture = true,
            int imageHeightAndWidth = 256,
            bool tryUseLocalContent = true,
            Commitment commitment = Commitment.Confirmed)
        {
            var newData = await MetadataAccount.GetAccount(connection, new PublicKey(mint));

            if (newData?.metadata == null || newData?.offchainData == null) return null;

            var met = new Metaplex(newData);
            var newNft = new SolanaNft(met);

            //FileLoader.SaveToPersistentDataPath(Path.Combine(Application.persistentDataPath, $"{mint}.json"), newNft.metaplexData.data);
            return newNft;
        }


    }
}