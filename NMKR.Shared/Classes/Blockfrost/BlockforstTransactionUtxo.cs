using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Blockfrost
{
    public partial class BlockfrostTransactionUtxo
    {
        [JsonProperty("hash", NullValueHandling = NullValueHandling.Ignore)]
        public string Hash { get; set; }

        [JsonProperty("inputs", NullValueHandling = NullValueHandling.Ignore)]
        public Put[] Inputs { get; set; }

        [JsonProperty("outputs", NullValueHandling = NullValueHandling.Ignore)]
        public Put[] Outputs { get; set; }
    }

    public partial class Put
    {
        [JsonProperty("address", NullValueHandling = NullValueHandling.Ignore)]
        public string Address { get; set; }

        [JsonProperty("amount", NullValueHandling = NullValueHandling.Ignore)]
        public Amount[] Amount { get; set; }

        [JsonProperty("tx_hash", NullValueHandling = NullValueHandling.Ignore)]
        public string TxHash { get; set; }

        [JsonProperty("output_index", NullValueHandling = NullValueHandling.Ignore)]
        public long? OutputIndex { get; set; }

        [JsonProperty("data_hash")]
        public string DataHash { get; set; }

        [JsonProperty("inline_datum")]
        public object InlineDatum { get; set; }

        [JsonProperty("reference_script_hash")]
        public object ReferenceScriptHash { get; set; }

        [JsonProperty("collateral", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Collateral { get; set; }

        [JsonProperty("reference", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Reference { get; set; }
    }

    public partial class Amount
    {
        [JsonProperty("unit", NullValueHandling = NullValueHandling.Ignore)]
        public string Unit { get; set; }

        [JsonProperty("quantity", NullValueHandling = NullValueHandling.Ignore)]
        public long? Quantity { get; set; }
    }
}
