using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Blockfrost
{
    public partial class BlockfrostAddressInformationClass
    {
        [JsonProperty("address", NullValueHandling = NullValueHandling.Ignore)]
        public string Address { get; set; }

        [JsonProperty("amount", NullValueHandling = NullValueHandling.Ignore)]
        public Amount[] Amount { get; set; }

        [JsonProperty("stake_address", NullValueHandling = NullValueHandling.Ignore)]
        public string StakeAddress { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("script", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Script { get; set; }
    }
}