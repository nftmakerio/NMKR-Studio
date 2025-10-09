using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Blockfrost
{
    public partial class BlockfrostDatumJsonClass
    {
        [JsonProperty("json_value", NullValueHandling = NullValueHandling.Ignore)]
        public string JsonValue { get; set; }
    }
}
