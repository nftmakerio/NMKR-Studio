using Newtonsoft.Json;

namespace NMKR.Shared.Functions.Solana.Helios
{
    public partial class GetAssetsByOwnerClass
    {
        [JsonProperty("jsonrpc", NullValueHandling = NullValueHandling.Ignore)]
        public string Jsonrpc { get; set; } = "2.0";

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; } = "NMKR";

        [JsonProperty("method", NullValueHandling = NullValueHandling.Ignore)]
        public string Method { get; set; }

        [JsonProperty("params", NullValueHandling = NullValueHandling.Ignore)]
        public Params Params { get; set; }
    }

    public partial class Params
    {
        [JsonProperty("ownerAddress", NullValueHandling = NullValueHandling.Ignore)]
        public string OwnerAddress { get; set; }

        [JsonProperty("page", NullValueHandling = NullValueHandling.Ignore)]
        public long? Page { get; set; }

        [JsonProperty("limit", NullValueHandling = NullValueHandling.Ignore)]
        public long? Limit { get; set; }

        [JsonProperty("displayOptions", NullValueHandling = NullValueHandling.Ignore)]
        public DisplayOptions DisplayOptions { get; set; }
        [JsonProperty("groupKey", NullValueHandling = NullValueHandling.Ignore)]
        public string GroupKey { get; set; }
        [JsonProperty("groupValue", NullValueHandling = NullValueHandling.Ignore)]
        public string GroupValue { get; set; }
    }

    public partial class DisplayOptions
    {
        [JsonProperty("showFungible", NullValueHandling = NullValueHandling.Ignore)]
        public bool ShowFungible { get; set; }
    }
}