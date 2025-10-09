using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Crossmint
{
    public partial class CrossmintErrorMessageClass
    {
        [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
        public string Error { get; set; }

        [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }
    }
}