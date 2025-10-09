using Newtonsoft.Json;

namespace NMKR.Shared.Mailerlite
{
    public partial class MailerliteSubscriberClass
    {
        [JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
        public string Email { get; set; }

        [JsonProperty("fields", NullValueHandling = NullValueHandling.Ignore)]
        public Fields Fields { get; set; }

        [JsonProperty("groups", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Groups { get; set; }
    }

    public partial class Fields
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("last_name", NullValueHandling = NullValueHandling.Ignore)]
        public string LastName { get; set; }

        [JsonProperty("company")]
        public object Company { get; set; }

        [JsonProperty("country")]
        public object Country { get; set; }

        [JsonProperty("city")]
        public object City { get; set; }

        [JsonProperty("phone")]
        public object Phone { get; set; }

        [JsonProperty("state")]
        public object State { get; set; }

        [JsonProperty("z_i_p")]
        public object ZIP { get; set; }
    }
}