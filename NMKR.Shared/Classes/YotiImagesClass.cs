using System;
using Newtonsoft.Json;

namespace NMKR.Shared.Classes
{
    public partial class YotiImagesClass
    {
        [JsonProperty("facemap", NullValueHandling = NullValueHandling.Ignore)]
        public Facemap Facemap { get; set; }

        [JsonProperty("frames", NullValueHandling = NullValueHandling.Ignore)]
        public Frame[] Frames { get; set; }

        [JsonProperty("liveness_type", NullValueHandling = NullValueHandling.Ignore)]
        public string LivenessType { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? Id { get; set; }

        [JsonProperty("tasks", NullValueHandling = NullValueHandling.Ignore)]
        public object[] Tasks { get; set; }

        [JsonProperty("source", NullValueHandling = NullValueHandling.Ignore)]
        public Source Source { get; set; }
    }

    public partial class Facemap
    {
        [JsonProperty("media", NullValueHandling = NullValueHandling.Ignore)]
        public Media Media { get; set; }
    }

    public partial class Media
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? Id { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("created", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? Created { get; set; }

        [JsonProperty("last_updated", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? LastUpdated { get; set; }
    }

    public partial class Frame
    {
        [JsonProperty("Media")]
        public Media Media { get; set; }
    }

    public partial class Source
    {
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }
    }

    public partial class YotiImagesClass
    {
        public static YotiImagesClass[] FromJson(string json) => JsonConvert.DeserializeObject<YotiImagesClass[]>(json, QuickType.Converter.Settings);
    }


}
