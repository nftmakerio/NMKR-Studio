using System;
using Newtonsoft.Json;

namespace NMKR.Shared.Classes
{
    public partial class NmkrHelpClass
    {
        [JsonProperty("items", NullValueHandling = NullValueHandling.Ignore)]
        public NmkrHelpItem[] Items { get; set; }

        [JsonProperty("next", NullValueHandling = NullValueHandling.Ignore)]
        public NmkrHelpNext Next { get; set; }
    }

    public partial class NmkrHelpItem
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        [JsonProperty("pages", NullValueHandling = NullValueHandling.Ignore)]
        public NmkrHelpPage[] Pages { get; set; }
    }

    public partial class NmkrHelpPage
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }

        [JsonProperty("path", NullValueHandling = NullValueHandling.Ignore)]
        public string Path { get; set; }

        [JsonProperty("sections", NullValueHandling = NullValueHandling.Ignore)]
        public NmkrHelpPage[] Sections { get; set; }

        [JsonProperty("urls", NullValueHandling = NullValueHandling.Ignore)]
        public NmkrHelpUrls Urls { get; set; }

        [JsonProperty("body", NullValueHandling = NullValueHandling.Ignore)]
        public string Body { get; set; }
    }

    public partial class NmkrHelpUrls
    {
        [JsonProperty("app", NullValueHandling = NullValueHandling.Ignore)]
        public Uri App { get; set; }
    }

    public partial class NmkrHelpNext
    {
        [JsonProperty("page", NullValueHandling = NullValueHandling.Ignore)]
        public long? Page { get; set; }
    }
}
