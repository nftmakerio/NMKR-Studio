using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Solana
{
    public class UpdateSolanaNftCoreClass : SolanaApiBaseClass
    {

        [JsonProperty("nftMintAddress", NullValueHandling = NullValueHandling.Ignore)]
        public string NftMintAddress { get; set; } = "";
        [JsonProperty("collectionaddress", NullValueHandling = NullValueHandling.Ignore)]
        public string CollectionAddress { get; set; } = "";

        [JsonProperty("newName", NullValueHandling = NullValueHandling.Ignore)]
        public string NewName { get; set; } = "";

        [JsonProperty("newUri", NullValueHandling = NullValueHandling.Ignore)]
        public string NewUri { get; set; } = "";

        [JsonProperty("updateAuthority", NullValueHandling = NullValueHandling.Ignore)]
        public SolanaKeysClass UpdateAuthority { get; set; } = new SolanaKeysClass();

    }
}