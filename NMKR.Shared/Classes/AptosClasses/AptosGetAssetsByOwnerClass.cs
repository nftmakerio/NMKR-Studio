using System;
using Newtonsoft.Json;

namespace NMKR.Shared.Classes.AptosClasses
{
    public partial class AptosGetAssetsByOwnerClass
    {
        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public AptosGetAssetsByOwnerClassData Data { get; set; }
    }

    public partial class AptosGetAssetsByOwnerClassData
    {
        [JsonProperty("current_token_ownerships_v2", NullValueHandling = NullValueHandling.Ignore)]
        public CurrentTokenOwnershipsV2[] CurrentTokenOwnershipsV2 { get; set; }
    }

    public partial class CurrentTokenOwnershipsV2
    {
        [JsonProperty("amount", NullValueHandling = NullValueHandling.Ignore)]
        public long? Amount { get; set; }

        [JsonProperty("token_standard", NullValueHandling = NullValueHandling.Ignore)]
        public string TokenStandard { get; set; }

        [JsonProperty("current_token_data", NullValueHandling = NullValueHandling.Ignore)]
        public CurrentTokenData CurrentTokenData { get; set; }
    }

    public partial class CurrentTokenData
    {
        [JsonProperty("aptos_name")]
        public object AptosName { get; set; }

        [JsonProperty("collection_id", NullValueHandling = NullValueHandling.Ignore)]
        public string CollectionId { get; set; }

        [JsonProperty("current_collection", NullValueHandling = NullValueHandling.Ignore)]
        public CurrentCollection CurrentCollection { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("token_name", NullValueHandling = NullValueHandling.Ignore)]
        public string TokenName { get; set; }

        [JsonProperty("token_uri", NullValueHandling = NullValueHandling.Ignore)]
        public Uri TokenUri { get; set; }
    }

    public partial class CurrentCollection
    {
        [JsonProperty("collection_name", NullValueHandling = NullValueHandling.Ignore)]
        public string CollectionName { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }
    }
}
