using NMKR.Shared.Enums;
using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Blockfrost
{
    public partial class BlockfrostAssetClass
    {
        [JsonProperty("asset", NullValueHandling = NullValueHandling.Ignore)]
        public string Asset { get; set; }

        [JsonProperty("policy_id", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyId { get; set; }

        [JsonProperty("asset_name", NullValueHandling = NullValueHandling.Ignore)]
        public string AssetName { get; set; }

        [JsonProperty("fingerprint", NullValueHandling = NullValueHandling.Ignore)]
        public string Fingerprint { get; set; }

        [JsonProperty("quantity", NullValueHandling = NullValueHandling.Ignore)]
        public long? Quantity { get; set; }

        [JsonProperty("initial_mint_tx_hash", NullValueHandling = NullValueHandling.Ignore)]
        public string InitialMintTxHash { get; set; }

        [JsonProperty("mint_or_burn_count", NullValueHandling = NullValueHandling.Ignore)]
        public long? MintOrBurnCount { get; set; }

        [JsonProperty("onchain_metadata", NullValueHandling = NullValueHandling.Ignore)]
        public object OnchainMetadata { get; set; }

        [JsonProperty("onchain_metadata_standard", NullValueHandling = NullValueHandling.Ignore)]
        public object OnchainMetadataStandard { get; set; }

        [JsonProperty("onchain_metadata_extra")]
        public object OnchainMetadataExtra { get; set; }

        [JsonProperty("metadata")]
        public object Metadata { get; set; }
        public Blockchain Blockchain { get; set; }
    }
}
