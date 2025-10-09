using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Solana
{
    public class BurnSolanaNftCoreClass : SolanaApiBaseClass
    {

        [JsonProperty("nftMintAddress", NullValueHandling = NullValueHandling.Ignore)]
        public string NftMintAddress { get; set; } = "";

        [JsonProperty("walletAddress", NullValueHandling = NullValueHandling.Ignore)]
        public SolanaKeysClass WalletAddress { get; set; } = new SolanaKeysClass();
    }
}