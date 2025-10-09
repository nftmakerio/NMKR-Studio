using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Blockfrost
{
    public partial class BlockfrostAssetsResultClass
    {
        [JsonProperty("unit", NullValueHandling = NullValueHandling.Ignore)]
        public string Unit { get; set; }

        [JsonProperty("quantity", NullValueHandling = NullValueHandling.Ignore)]
        public long? Quantity { get; set; }
    }
}