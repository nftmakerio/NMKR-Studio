using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Crossmint
{
    public partial class CrossmintSuccessClass
    {
        [JsonProperty("collectionId", NullValueHandling = NullValueHandling.Ignore)]
        public string? CollectionId { get; set; }
    }
}