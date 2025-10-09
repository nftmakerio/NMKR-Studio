using Newtonsoft.Json;

namespace NMKR.Shared.Classes
{
    public partial class CborToJsonClass
    {
        [JsonProperty("json")]
        public string Json { get; set; }
    }
}