using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Crossmint
{
    public partial class RegisterCrossmintProjectsClass
    {
        [JsonProperty("chain", NullValueHandling = NullValueHandling.Ignore)]
        public string Chain { get; set; }

        [JsonProperty("contractType", NullValueHandling = NullValueHandling.Ignore)]
        public string ContractType { get; set; }

        [JsonProperty("args", NullValueHandling = NullValueHandling.Ignore)]
        public RegisterCrossmintArgs Args { get; set; }

        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public RegisterCrossmintMetadata Metadata { get; set; }
    }

    public partial class RegisterCrossmintArgs
    {
        [JsonProperty("policyId", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyId { get; set; }

        [JsonProperty("projectUuid", NullValueHandling = NullValueHandling.Ignore)]
        public string ProjectUuid { get; set; }
    }

    public partial class RegisterCrossmintMetadata
    {
        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("imageUrl", NullValueHandling = NullValueHandling.Ignore)]
        public string ImageUrl { get; set; }

        [JsonProperty("social", NullValueHandling = NullValueHandling.Ignore)]
        public RegisterCrossmintSocial Social { get; set; }
    }

    public partial class RegisterCrossmintSocial
    {
        [JsonProperty("twitter", NullValueHandling = NullValueHandling.Ignore)]
        public string Twitter { get; set; }

        [JsonProperty("discord", NullValueHandling = NullValueHandling.Ignore)]
        public string Discord { get; set; }

        [JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
        public string Email { get; set; }
    }
}
