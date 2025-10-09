using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Solana
{
    public class SolanaMetadataClass
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; } = "";
        [JsonProperty("symbol", NullValueHandling = NullValueHandling.Ignore)]
        public string Symbol { get; set; } = "";
        [JsonProperty("uri", NullValueHandling = NullValueHandling.Ignore)]
        public string Uri { get; set; } = "";

        [JsonProperty("seller_fee_basis_points", NullValueHandling = NullValueHandling.Ignore)]
        public long SellerFeeBasisPoints { get; set; } = 0;
    }
}
