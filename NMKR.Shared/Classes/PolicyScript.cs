using System.Collections.Generic;
using Newtonsoft.Json;

namespace NMKR.Shared.Classes
{
    public partial class PolicyScript
    {
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("scripts", NullValueHandling = NullValueHandling.Ignore)]
        public List<PolicyScriptScript> Scripts { get; set; }
    }

    public partial class PolicyScriptScript
    {
        [JsonProperty("slot", NullValueHandling = NullValueHandling.Ignore)]
        public long? Slot { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("keyHash", NullValueHandling = NullValueHandling.Ignore)]
        public string KeyHash { get; set; }
    }
}