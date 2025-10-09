using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Bitcoin
{
    public partial class BitcoinAddressTxs
    {
        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public BitcoinAddressTxsDatum[] Data { get; set; }

        [JsonProperty("last_updated", NullValueHandling = NullValueHandling.Ignore)]
        public LastUpdated LastUpdated { get; set; }

        [JsonProperty("next_cursor")]
        public object NextCursor { get; set; }
    }

    public partial class BitcoinAddressTxsDatum
    {
        [JsonProperty("tx_hash", NullValueHandling = NullValueHandling.Ignore)]
        public string TxHash { get; set; }

        [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
        public long? Height { get; set; }

        [JsonProperty("input", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Input { get; set; }

        [JsonProperty("output", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Output { get; set; }
    }
}