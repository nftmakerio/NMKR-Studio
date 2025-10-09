using Newtonsoft.Json;

namespace NMKR.Shared.Classes
{
    public partial class BlockfrostMintedTokensClass
    {
        [JsonProperty("asset", NullValueHandling = NullValueHandling.Ignore)]
        public string Asset { get; set; }

        [JsonProperty("quantity", NullValueHandling = NullValueHandling.Ignore)]
        public long? Quantity { get; set; }
    }
}
