using System;
using Newtonsoft.Json;

namespace NMKR.Shared.Classes.NmkrStore
{
    public partial class NmkrStoreApiGetVerifiedCollectionClass
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? Id { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("discord", NullValueHandling = NullValueHandling.Ignore)]
        public string Discord { get; set; }

        [JsonProperty("instagram", NullValueHandling = NullValueHandling.Ignore)]
        public string Instagram { get; set; }

        [JsonProperty("twitter", NullValueHandling = NullValueHandling.Ignore)]
        public string Twitter { get; set; }

        [JsonProperty("website", NullValueHandling = NullValueHandling.Ignore)]
        public string Website { get; set; }

        [JsonProperty("isActive", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsActive { get; set; }

        [JsonProperty("isFraudulent", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsFraudulent { get; set; }

        [JsonProperty("isLockedForCrawling", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsLockedForCrawling { get; set; }

        [JsonProperty("isVerified", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsVerified { get; set; }

        [JsonProperty("creatorName", NullValueHandling = NullValueHandling.Ignore)]
        public string CreatorName { get; set; }

        [JsonProperty("previewImage", NullValueHandling = NullValueHandling.Ignore)]
        public string PreviewImage { get; set; }

        [JsonProperty("policyIdList", NullValueHandling = NullValueHandling.Ignore)]
        public string[] PolicyIdList { get; set; }

        [JsonProperty("projectId", NullValueHandling = NullValueHandling.Ignore)]
        public long? ProjectId { get; set; }

        [JsonProperty("dropUrl", NullValueHandling = NullValueHandling.Ignore)]
        public string DropUrl { get; set; }

        [JsonProperty("dropPriceInLovelace", NullValueHandling = NullValueHandling.Ignore)]
        public long? DropPriceInLovelace { get; set; }

        [JsonProperty("isDropInProgress", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsDropInProgress { get; set; }
        [JsonProperty("projectUid", NullValueHandling = NullValueHandling.Ignore)]
        public string ProjectUid { get; set; }
    }
}
