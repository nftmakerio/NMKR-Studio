using Newtonsoft.Json;

namespace NMKR.Shared.Classes
{
    public partial class RoyaltyTokenClass
    {
        [JsonProperty("key")]
        public long Key { get; set; }

        [JsonProperty("json")]
        public JsonR Json { get; set; }
    }

    public partial class JsonR
    {
        [JsonProperty("pct", NullValueHandling = NullValueHandling.Ignore)]
        public string Pct { get; set; }

        [JsonProperty("rate", NullValueHandling = NullValueHandling.Ignore)]
        public string Rate { get; set; }

        [JsonProperty("addr", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Addr { get; set; }
    }

   
        public partial class RoyaltyTokenClass
        {
            public static RoyaltyTokenClass FromJson(string json) => JsonConvert.DeserializeObject<RoyaltyTokenClass>(json, QuickType.Converter.Settings);
        }

        public static class SerializeRoyaltyTokenClass
    {
            public static string ToJson(this RoyaltyTokenClass self) => JsonConvert.SerializeObject(self, QuickType.Converter.Settings);
        }

     
    }
