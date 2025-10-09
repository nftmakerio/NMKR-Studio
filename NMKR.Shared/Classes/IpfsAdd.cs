using Newtonsoft.Json;

namespace NMKR.Shared.Classes
{
    public partial class Ipfsadd
    {
        [JsonProperty("Name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("Hash", NullValueHandling = NullValueHandling.Ignore)]
        public string Hash { get; set; }

        [JsonProperty("Size", NullValueHandling = NullValueHandling.Ignore)]
        public long? Size { get; set; }
    }

    public partial class Ipfsadd
    {
        public static Ipfsadd FromJson(string json) => JsonConvert.DeserializeObject<Ipfsadd>(json);
    }

    /*public static class Serialize
    {
        public static string ToJson(this Ipfsadd self) => JsonConvert.SerializeObject(self);
    }*/
}