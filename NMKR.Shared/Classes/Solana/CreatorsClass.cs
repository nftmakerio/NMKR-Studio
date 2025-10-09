using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Solana
{
    public class CreatorsClass
    {
        [JsonProperty("address", NullValueHandling = NullValueHandling.Ignore)]
        public string Address { get; set; }

        [JsonProperty("verified", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Verified { get; set; }

        [JsonProperty("share", NullValueHandling = NullValueHandling.Ignore)]
        public int? Share { get; set; }
    }
}