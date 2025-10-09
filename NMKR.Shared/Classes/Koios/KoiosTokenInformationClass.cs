using System;
using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Koios
{
    public partial class TokenInformationClass
    {
        [JsonProperty("policy_id", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyId { get; set; }

        [JsonProperty("asset_name", NullValueHandling = NullValueHandling.Ignore)]
        public string AssetName { get; set; }

        [JsonProperty("asset_name_ascii", NullValueHandling = NullValueHandling.Ignore)]
        public string AssetNameAscii { get; set; }

        [JsonProperty("ticker", NullValueHandling = NullValueHandling.Ignore)]
        public string Ticker { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Url { get; set; }

        [JsonProperty("decimals", NullValueHandling = NullValueHandling.Ignore)]
        public long? Decimals { get; set; }

        [JsonProperty("logo", NullValueHandling = NullValueHandling.Ignore)]
        public string Logo { get; set; }
    }
}