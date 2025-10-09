using Newtonsoft.Json;

namespace NMKR.Shared.Classes
{
    public partial class AssetTransactionsClass
    {
        [JsonProperty("tx_hash", NullValueHandling = NullValueHandling.Ignore)]
        public string TxHash { get; set; }

        [JsonProperty("epoch_no", NullValueHandling = NullValueHandling.Ignore)]
        public long? EpochNo { get; set; }

        [JsonProperty("block_height", NullValueHandling = NullValueHandling.Ignore)]
        public long? BlockHeight { get; set; }

        [JsonProperty("block_time", NullValueHandling = NullValueHandling.Ignore)]
        public long? BlockTime { get; set; }
    }
}