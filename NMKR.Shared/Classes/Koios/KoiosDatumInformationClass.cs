using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Koios
{
    public partial class KoiosDatumInformationClass
    {
        [JsonProperty("hash", NullValueHandling = NullValueHandling.Ignore)]
        public string Hash { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public Value Value { get; set; }

        [JsonProperty("bytes", NullValueHandling = NullValueHandling.Ignore)]
        public string Bytes { get; set; }
    }

    public partial class Value
    {
        [JsonProperty("bytes", NullValueHandling = NullValueHandling.Ignore)]
        public string Bytes { get; set; }
    }
}