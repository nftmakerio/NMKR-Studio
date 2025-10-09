using NMKR.Shared.Functions;
using Newtonsoft.Json;

namespace NMKR.Shared.Blockchains
{
    public class BlockchainKeysClass
    {
        [JsonProperty("publicKey", NullValueHandling = NullValueHandling.Ignore)]
        public string PublicKey { get; set; } = "";

        [JsonProperty("secretKey", NullValueHandling = NullValueHandling.Ignore)]
        public string SecretKey { get; set; } = "";

        [JsonProperty("address", NullValueHandling = NullValueHandling.Ignore)]
        public string Address { get; set; } = "";
        [JsonProperty("seed", NullValueHandling = NullValueHandling.Ignore)]
        public string Seed { get; set; } = "";
    }
    public class CollectionClass
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; } = "";

        [JsonProperty("symbol", NullValueHandling = NullValueHandling.Ignore)]
        public string Symbol { get; set; } = "";

        [JsonProperty("uri", NullValueHandling = NullValueHandling.Ignore)]
        public string Uri { get; set; } = "";

        [JsonProperty("updateAuthority", NullValueHandling = NullValueHandling.Ignore)]
        public BlockchainKeysClass UpdateAuthority { get; set; } = new BlockchainKeysClass();

        [JsonProperty("payer", NullValueHandling = NullValueHandling.Ignore)]
        public BlockchainKeysClass Payer { get; set; } = new BlockchainKeysClass();
        [JsonProperty("sellerFeeBasisPoints", NullValueHandling = NullValueHandling.Ignore)]
        public long SellerFeeBasisPoints { get; set; } = 0;

        [JsonProperty("network", NullValueHandling = NullValueHandling.Ignore)]
        public string Network { get; } = GlobalFunctions.IsMainnet() ? "mainnet" : "testnet";
    }
}
