using System;
using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Iagon
{
    public partial class IagonCreateDirectoryResultClass
    {
        [JsonProperty("success", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Success { get; set; }

        [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }

        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public DataDirectory Data { get; set; }
    }

    public partial class DataDirectory
    {
        [JsonProperty("client_id", NullValueHandling = NullValueHandling.Ignore)]
        public string ClientId { get; set; }

        [JsonProperty("visibility", NullValueHandling = NullValueHandling.Ignore)]
        public string Visibility { get; set; }

        [JsonProperty("path", NullValueHandling = NullValueHandling.Ignore)]
        public string Path { get; set; }

        [JsonProperty("directory_name", NullValueHandling = NullValueHandling.Ignore)]
        public string DirectoryName { get; set; }

        [JsonProperty("parent_directory_id")]
        public object ParentDirectoryId { get; set; }

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
}