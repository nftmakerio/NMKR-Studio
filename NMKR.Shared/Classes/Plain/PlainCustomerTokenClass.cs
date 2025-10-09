using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Plain
{
    public partial class PlainCustomerTokenClass
    {
        [JsonProperty("fullName", NullValueHandling = NullValueHandling.Ignore)]
        public string FullName { get; set; }

        [JsonProperty("shortName", NullValueHandling = NullValueHandling.Ignore)]
        public string ShortName { get; set; }

        [JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
        public Email Email { get; set; }

        [JsonProperty("externalId", NullValueHandling = NullValueHandling.Ignore)]
        public string ExternalId { get; set; }
    }

    public partial class Email
    {
        [JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
        public string EmailEmail { get; set; }

        [JsonProperty("isVerified", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsVerified { get; set; }
    }
}