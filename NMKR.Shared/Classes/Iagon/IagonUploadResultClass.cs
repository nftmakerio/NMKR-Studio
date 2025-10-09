using System;
using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Iagon
{
    public partial class IagonUploadResultClass
    {
        [JsonProperty("success", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Success { get; set; }

        [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }

        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public Data Data { get; set; }
    }

    public partial class Data
    {
        [JsonProperty("client_id", NullValueHandling = NullValueHandling.Ignore)]
        public string ClientId { get; set; }

        [JsonProperty("parent_directory_id")]
        public object ParentDirectoryId { get; set; }

        [JsonProperty("availability", NullValueHandling = NullValueHandling.Ignore)]
        public string Availability { get; set; }

        [JsonProperty("visibility", NullValueHandling = NullValueHandling.Ignore)]
        public string Visibility { get; set; }

        [JsonProperty("region")]
        public object Region { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("shards_info", NullValueHandling = NullValueHandling.Ignore)]
        public ShardsInfo[] ShardsInfo { get; set; }

        [JsonProperty("path", NullValueHandling = NullValueHandling.Ignore)]
        public string Path { get; set; }

        [JsonProperty("unique_id", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? UniqueId { get; set; }

        [JsonProperty("file_size_byte_native", NullValueHandling = NullValueHandling.Ignore)]
        public long? FileSizeByteNative { get; set; }

        [JsonProperty("file_size_byte_encrypted", NullValueHandling = NullValueHandling.Ignore)]
        public long? FileSizeByteEncrypted { get; set; }

        [JsonProperty("index_listing", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IndexListing { get; set; }

        [JsonProperty("_id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("created_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? CreatedAt { get; set; }

        [JsonProperty("updated_at", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? UpdatedAt { get; set; }

        [JsonProperty("__v", NullValueHandling = NullValueHandling.Ignore)]
        public long? V { get; set; }
    }

    public partial class ShardsInfo
    {
        [JsonProperty("file_index", NullValueHandling = NullValueHandling.Ignore)]
        public string FileIndex { get; set; }

        [JsonProperty("filename_hash", NullValueHandling = NullValueHandling.Ignore)]
        public string FilenameHash { get; set; }

        [JsonProperty("file_hash", NullValueHandling = NullValueHandling.Ignore)]
        public string FileHash { get; set; }

        [JsonProperty("shard_size", NullValueHandling = NullValueHandling.Ignore)]
        public long? ShardSize { get; set; }

        [JsonProperty("nodeId", NullValueHandling = NullValueHandling.Ignore)]
        public string NodeId { get; set; }
    }
}
