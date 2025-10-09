using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Bitcoin
{
    public partial class BitcoinGetBalance
    {
        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public long? Data { get; set; }

        [JsonProperty("last_updated", NullValueHandling = NullValueHandling.Ignore)]
        public LastUpdated LastUpdated { get; set; }
    }

    public partial class LastUpdated
    {
        [JsonProperty("block_hash", NullValueHandling = NullValueHandling.Ignore)]
        public string BlockHash { get; set; }

        [JsonProperty("block_height", NullValueHandling = NullValueHandling.Ignore)]
        public long? BlockHeight { get; set; }
    }
}