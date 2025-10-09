using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Bitcoin
{
    public partial class BitcoinInscribe
    {
        [JsonProperty("files", NullValueHandling = NullValueHandling.Ignore)]
        public BitcoinInscribeFile[] Files { get; set; }

        [JsonProperty("postage", NullValueHandling = NullValueHandling.Ignore)]
        public long? Postage { get; set; }

        [JsonProperty("receiveAddress", NullValueHandling = NullValueHandling.Ignore)]
        public string ReceiveAddress { get; set; }

        [JsonProperty("fee", NullValueHandling = NullValueHandling.Ignore)]
        public long? Fee { get; set; }
    }

    public partial class BitcoinInscribeFile
    {
        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public long? Size { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("dataURL", NullValueHandling = NullValueHandling.Ignore)]
        public string DataUrl { get; set; }

        [JsonProperty("metadataDataURL", NullValueHandling = NullValueHandling.Ignore)]
        public string MetadataDataUrl { get; set; }

        [JsonProperty("metadataSize", NullValueHandling = NullValueHandling.Ignore)]
        public long? MetadataSize { get; set; }
    }
}