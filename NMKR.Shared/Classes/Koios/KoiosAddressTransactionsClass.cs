using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Koios
{
    public partial class KoiosAddressTransactionsClass
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
