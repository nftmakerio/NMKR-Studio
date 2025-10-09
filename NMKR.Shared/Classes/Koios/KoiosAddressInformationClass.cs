using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Koios
{
    public partial class KoiosAddressInformationClass
    {
        [JsonProperty("balance", NullValueHandling = NullValueHandling.Ignore)]
        public long? Balance { get; set; }

        [JsonProperty("stake_address", NullValueHandling = NullValueHandling.Ignore)]
        public string StakeAddress { get; set; }

        [JsonProperty("script_address", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ScriptAddress { get; set; }

        [JsonProperty("utxo_set", NullValueHandling = NullValueHandling.Ignore)]
        public KoiosAddressInformationUtxoSet[] UtxoSet { get; set; }
    }

    public partial class KoiosAddressInformationUtxoSet
    {
        [JsonProperty("tx_hash", NullValueHandling = NullValueHandling.Ignore)]
        public string TxHash { get; set; }

        [JsonProperty("tx_index", NullValueHandling = NullValueHandling.Ignore)]
        public long? TxIndex { get; set; }

        [JsonProperty("block_height", NullValueHandling = NullValueHandling.Ignore)]
        public long? BlockHeight { get; set; }

        [JsonProperty("block_time", NullValueHandling = NullValueHandling.Ignore)]
        public long? BlockTime { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public long? Value { get; set; }

        [JsonProperty("datum_hash")]
        public object DatumHash { get; set; }

        [JsonProperty("asset_list", NullValueHandling = NullValueHandling.Ignore)]
        public KoiosAddressInformationAssetList[] AssetList { get; set; }
    }

    public partial class KoiosAddressInformationAssetList
    {
        [JsonProperty("policy_id", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyId { get; set; }

        [JsonProperty("asset_name", NullValueHandling = NullValueHandling.Ignore)]
        public string AssetName { get; set; }

        [JsonProperty("quantity", NullValueHandling = NullValueHandling.Ignore)]
        public long? Quantity { get; set; }
    }
}
