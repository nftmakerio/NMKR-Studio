using Newtonsoft.Json;

namespace NMKR.Shared.Classes
{

    public class UploadNftClassV2
    {
        [JsonProperty("tokenname", NullValueHandling = NullValueHandling.Ignore)]
        public string Tokenname { get; set; }
        [JsonProperty("displayname", NullValueHandling = NullValueHandling.Ignore)]
        public string Displayname { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("previewImageNft", NullValueHandling = NullValueHandling.Ignore)]
        public NftFileV2 PreviewImageNft { get; set; }

        [JsonProperty("subfiles", NullValueHandling = NullValueHandling.Ignore)]
        public NftSubfileFileV2[] Subfiles { get; set; }

        [JsonProperty("metadataPlaceholder", NullValueHandling = NullValueHandling.Ignore)]
        public MetadataPlaceholderClass[] MetadataPlaceholder { get; set; }

        [JsonProperty("metadataOverride", NullValueHandling = NullValueHandling.Ignore)]
        public string MetadataOverride { get; set; }
        [JsonProperty("metadataOverrideCip68", NullValueHandling = NullValueHandling.Ignore)]
        public string MetadataOverrideCip68 { get; set; }

        [JsonProperty("priceInLovelace", NullValueHandling = NullValueHandling.Ignore)]
        public long? PriceInLovelace { get; set; }

        [JsonProperty("isblocked", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsBlocked { get; set; }
    }

    public class NftFileV2
    {
        [JsonProperty("mimetype", NullValueHandling = NullValueHandling.Ignore)]
        public string Mimetype { get; set; }

        [JsonProperty("fileFromBase64", NullValueHandling = NullValueHandling.Ignore)]
        public string FileFromBase64 { get; set; }

        [JsonProperty("fileFromUrl", NullValueHandling = NullValueHandling.Ignore)]
        public string FileFromsUrl { get; set; }

        [JsonProperty("fileFromIPFS", NullValueHandling = NullValueHandling.Ignore)]
        public string FileFromIPFS { get; set; }

    }
    public class NftSubfileFileV2
    {
        [JsonProperty("subfile", NullValueHandling = NullValueHandling.Ignore)]
        public NftFileV2 Subfile { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("metadataPlaceholder", NullValueHandling = NullValueHandling.Ignore)]
        public MetadataPlaceholderClass[] MetadataPlaceholder { get; set; }
    }

    public class UploadToIpfsClass
    {
        [JsonProperty("mimetype", NullValueHandling = NullValueHandling.Ignore)]
        public string Mimetype { get; set; }

        [JsonProperty("fileFromBase64", NullValueHandling = NullValueHandling.Ignore)]
        public string FileFromBase64 { get; set; }

        [JsonProperty("fileFromUrl", NullValueHandling = NullValueHandling.Ignore)]
        public string FileFromsUrl { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
    }

}