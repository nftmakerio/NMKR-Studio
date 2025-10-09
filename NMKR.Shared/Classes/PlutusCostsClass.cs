using Newtonsoft.Json;

namespace NMKR.Shared.Classes
{
    public partial class PlutusCostsClass
    {
        [JsonProperty("executionUnits", NullValueHandling = NullValueHandling.Ignore)]
        public ExecutionUnits ExecutionUnits { get; set; }

        [JsonProperty("lovelaceCost", NullValueHandling = NullValueHandling.Ignore)]
        public long? LovelaceCost { get; set; }

        [JsonProperty("scriptHash", NullValueHandling = NullValueHandling.Ignore)]
        public string ScriptHash { get; set; }
    }

    public partial class ExecutionUnits
    {
        [JsonProperty("memory", NullValueHandling = NullValueHandling.Ignore)]
        public long? Memory { get; set; }

        [JsonProperty("steps", NullValueHandling = NullValueHandling.Ignore)]
        public long? Steps { get; set; }
    }
}