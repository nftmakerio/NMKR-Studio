using Newtonsoft.Json;

namespace NMKR.Shared.Classes
{
    public class UploadMetadataClass
    {
        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public string Metadata { get; set; }
    }
}
