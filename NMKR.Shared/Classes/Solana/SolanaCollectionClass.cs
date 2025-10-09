using NMKR.Shared.Functions;

namespace NMKR.Shared.Classes.Solana
{
    using Newtonsoft.Json;

    public class SolanaCollectionClass
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; } = "";

        [JsonProperty("symbol", NullValueHandling = NullValueHandling.Ignore)]
        public string Symbol { get; set; } = "";

        [JsonProperty("uri", NullValueHandling = NullValueHandling.Ignore)]
        public string Uri { get; set; } = "";

        [JsonProperty("updateAuthority", NullValueHandling = NullValueHandling.Ignore)]
        public SolanaKeysClass UpdateAuthority { get; set; } = new SolanaKeysClass();

        [JsonProperty("payer", NullValueHandling = NullValueHandling.Ignore)]
        public SolanaKeysClass Payer { get; set; } = new SolanaKeysClass();
        [JsonProperty("sellerFeeBasisPoints", NullValueHandling = NullValueHandling.Ignore)]
        public long SellerFeeBasisPoints { get; set; } = 0;

        [JsonProperty("network", NullValueHandling = NullValueHandling.Ignore)]
        public string Network { get; } = GlobalFunctions.IsMainnet()?"mainnet": "devnet";
    }

}
