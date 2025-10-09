using Newtonsoft.Json;

namespace NMKR.Shared.Classes
{
        public partial class Tip
        {
            [JsonProperty("epoch", NullValueHandling = NullValueHandling.Ignore)]
            public long? Epoch { get; set; }

            [JsonProperty("hash", NullValueHandling = NullValueHandling.Ignore)]
            public string Hash { get; set; }

            [JsonProperty("slot", NullValueHandling = NullValueHandling.Ignore)]
            public long? Slot { get; set; }

            [JsonProperty("block", NullValueHandling = NullValueHandling.Ignore)]
            public long? Block { get; set; }

            [JsonProperty("era", NullValueHandling = NullValueHandling.Ignore)]
            public string Era { get; set; }
        }

      
    }




