using Newtonsoft.Json;

namespace NMKR.Shared.Classes
{
    public partial class CardanoMessageClass
    {
        [JsonProperty("674", NullValueHandling = NullValueHandling.Ignore)]
        public The674 The674 { get; set; }
    }

    public partial class The674
    {
        [JsonProperty("msg", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Msg { get; set; }
    }
}