using Newtonsoft.Json;

namespace NMKR.Shared.Classes
{

    public class UploadNftClass
    {
        [JsonProperty("assetName", NullValueHandling = NullValueHandling.Ignore)]
        public string AssetName { get; set; }

        [JsonProperty("previewImageNft", NullValueHandling = NullValueHandling.Ignore)]
        public NftFile PreviewImageNft { get; set; }

        [JsonProperty("subfiles", NullValueHandling = NullValueHandling.Ignore)]
        public NftFile[] Subfiles { get; set; }

        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public string Metadata { get; set; }
        [JsonProperty("metadataCip68", NullValueHandling = NullValueHandling.Ignore)]
        public string MetadataCip68 { get; set; }
    }

    public class NftFile
    {
        [JsonProperty("mimetype", NullValueHandling = NullValueHandling.Ignore)]
        public string Mimetype { get; set; }

        [JsonProperty("fileFromBase64", NullValueHandling = NullValueHandling.Ignore)]
        public string FileFromBase64 { get; set; }

        [JsonProperty("fileFromsUrl", NullValueHandling = NullValueHandling.Ignore)]
        public string FileFromsUrl { get; set; }

        [JsonProperty("fileFromIPFS", NullValueHandling = NullValueHandling.Ignore)]
        public string FileFromIPFS { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("displayname", NullValueHandling = NullValueHandling.Ignore)]
        public string Displayname { get; set; }

        [JsonProperty("metadataPlaceholder", NullValueHandling = NullValueHandling.Ignore)]
        public MetadataPlaceholderClass[] MetadataPlaceholder { get; set; }
    }

    public class MetadataPlaceholderClass
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public string Value { get; set; }
    }

}
