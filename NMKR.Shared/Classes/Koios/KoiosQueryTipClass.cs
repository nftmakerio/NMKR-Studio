using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Koios
{
    public partial class KoiosQueryTipClass
    {
        [JsonProperty("hash", NullValueHandling = NullValueHandling.Ignore)]
        public string Hash { get; set; }

        [JsonProperty("epoch_no", NullValueHandling = NullValueHandling.Ignore)]
        public long? EpochNo { get; set; }

        [JsonProperty("abs_slot", NullValueHandling = NullValueHandling.Ignore)]
        public long? AbsSlot { get; set; }

        [JsonProperty("epoch_slot", NullValueHandling = NullValueHandling.Ignore)]
        public long? EpochSlot { get; set; }

        [JsonProperty("block_no", NullValueHandling = NullValueHandling.Ignore)]
        public long? BlockNo { get; set; }

        [JsonProperty("block_time", NullValueHandling = NullValueHandling.Ignore)]
        public long? BlockTime { get; set; }
    }
}
