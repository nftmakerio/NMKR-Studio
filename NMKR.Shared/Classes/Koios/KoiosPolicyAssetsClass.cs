using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Koios
{
    public partial class KoiosPolicyAssetsClass
    {
        [JsonProperty("asset_name", NullValueHandling = NullValueHandling.Ignore)]
        public string AssetName { get; set; }

        [JsonProperty("asset_name_ascii", NullValueHandling = NullValueHandling.Ignore)]
        public string AssetNameAscii { get; set; }

        [JsonProperty("fingerprint", NullValueHandling = NullValueHandling.Ignore)]
        public string Fingerprint { get; set; }

        [JsonProperty("minting_tx_hash", NullValueHandling = NullValueHandling.Ignore)]
        public string MintingTxHash { get; set; }

        [JsonProperty("total_supply", NullValueHandling = NullValueHandling.Ignore)]
        public long? TotalSupply { get; set; }

        [JsonProperty("mint_cnt", NullValueHandling = NullValueHandling.Ignore)]
        public long? MintCnt { get; set; }

        [JsonProperty("burn_cnt", NullValueHandling = NullValueHandling.Ignore)]
        public long? BurnCnt { get; set; }

        [JsonProperty("creation_time", NullValueHandling = NullValueHandling.Ignore)]
        public long? CreationTime { get; set; }

        [JsonProperty("token_registry_metadata")]
        public object TokenRegistryMetadata { get; set; }
    }
  
}
