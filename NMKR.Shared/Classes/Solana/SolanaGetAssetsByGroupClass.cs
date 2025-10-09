using NMKR.Shared.Functions.Solana.Helios;
using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Solana
{
    public partial class SolanaGetAssetsByGroupClass
    {
        [JsonProperty("jsonrpc", NullValueHandling = NullValueHandling.Ignore)]
        public string Jsonrpc { get; set; }

        [JsonProperty("result", NullValueHandling = NullValueHandling.Ignore)]
        public Result Result { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }
    }

    public partial class Result
    {
        [JsonProperty("total", NullValueHandling = NullValueHandling.Ignore)]
        public long? Total { get; set; }

        [JsonProperty("limit", NullValueHandling = NullValueHandling.Ignore)]
        public long? Limit { get; set; }

        [JsonProperty("page", NullValueHandling = NullValueHandling.Ignore)]
        public long? Page { get; set; }

        [JsonProperty("items", NullValueHandling = NullValueHandling.Ignore)]
        public SolanaItem[] Items { get; set; }
    }

}
