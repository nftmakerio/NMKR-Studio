using System;
using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Koios
{
    public  class KoiosAssetPolicyInformationClass
    {
        [JsonProperty("asset_name", NullValueHandling = NullValueHandling.Ignore)]
        public string AssetName { get; set; }

        [JsonProperty("asset_name_ascii", NullValueHandling = NullValueHandling.Ignore)]
        public string AssetNameAscii { get; set; }

        [JsonProperty("fingerprint", NullValueHandling = NullValueHandling.Ignore)]
        public string Fingerprint { get; set; }

        [JsonProperty("token_registry_metadata", NullValueHandling = NullValueHandling.Ignore)]
        public TokenRegistryMetadata TokenRegistryMetadata { get; set; }

        [JsonProperty("total_supply", NullValueHandling = NullValueHandling.Ignore)]
        public long? TotalSupply { get; set; }

        [JsonProperty("creation_time", NullValueHandling = NullValueHandling.Ignore)]
        public long? CreationTime { get; set; }
    }

    public partial class TokenRegistryMetadata
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("ticker", NullValueHandling = NullValueHandling.Ignore)]
        public string Ticker { get; set; }

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Url { get; set; }

        [JsonProperty("logo", NullValueHandling = NullValueHandling.Ignore)]
        public string Logo { get; set; }

        [JsonProperty("decimals", NullValueHandling = NullValueHandling.Ignore)]
        public long? Decimals { get; set; }
    }
}