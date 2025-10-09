using Newtonsoft.Json;

namespace NMKR.Shared.Classes
{
    public partial class MatxRawClass
    {
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("cborHex", NullValueHandling = NullValueHandling.Ignore)]
        public string CborHex { get; set; }
    }
    
}