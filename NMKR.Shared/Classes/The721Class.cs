using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NMKR.Shared.Classes
{
    public partial class The721Class
    {
        [JsonProperty("721", NullValueHandling = NullValueHandling.Ignore)]
        public JObject The721 { get; set; }
    }
    public partial class The20Class
    {
        [JsonProperty("20", NullValueHandling = NullValueHandling.Ignore)]
        public JObject The20 { get; set; }
    }
    public partial class The777Class
    {
        [JsonProperty("777", NullValueHandling = NullValueHandling.Ignore)]
        public JObject The777 { get; set; }
    }
}
