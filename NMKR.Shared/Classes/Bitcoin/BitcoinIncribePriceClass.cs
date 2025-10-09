using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Bitcoin
{
    public partial class BitcoinIncribePriceClass
    {
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("order", NullValueHandling = NullValueHandling.Ignore)]
        public BitcoinIncribePriceOrder Order { get; set; }
    }

    public partial class BitcoinIncribePriceOrder
    {
        [JsonProperty("files", NullValueHandling = NullValueHandling.Ignore)]
        public BitcoinIncribePriceFile[] Files { get; set; }

        [JsonProperty("postage", NullValueHandling = NullValueHandling.Ignore)]
        public long? Postage { get; set; }

        [JsonProperty("receiveAddress", NullValueHandling = NullValueHandling.Ignore)]
        public string ReceiveAddress { get; set; }

        [JsonProperty("rareSats", NullValueHandling = NullValueHandling.Ignore)]
        public string RareSats { get; set; }

        [JsonProperty("compress", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Compress { get; set; }

        [JsonProperty("fee", NullValueHandling = NullValueHandling.Ignore)]
        public long? Fee { get; set; }
    }

    public partial class BitcoinIncribePriceFile
    {
        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public long? Size { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("metadataSize", NullValueHandling = NullValueHandling.Ignore)]
        public long? MetadataSize { get; set; }
    }
}