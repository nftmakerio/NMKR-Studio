using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Solana
{
    public partial class SolanaOffchainCollectionMetadataClass
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("symbol", NullValueHandling = NullValueHandling.Ignore)]
        public string Symbol { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("seller_fee_basis_points", NullValueHandling = NullValueHandling.Ignore)]
        public long? SellerFeeBasisPoints { get; set; }

        [JsonProperty("image", NullValueHandling = NullValueHandling.Ignore)]
        public string Image { get; set; }

        [JsonProperty("external_url", NullValueHandling = NullValueHandling.Ignore)]
        public string ExternalUrl { get; set; }
        /*
        [JsonProperty("attributes", NullValueHandling = NullValueHandling.Ignore)]
        public Attribute[] Attributes { get; set; }

        [JsonProperty("collection", NullValueHandling = NullValueHandling.Ignore)]
        public Collection Collection { get; set; }
        */
        [JsonProperty("properties", NullValueHandling = NullValueHandling.Ignore)]
        public Properties Properties { get; set; }
    }

    public partial class Attribute
    {
        [JsonProperty("trait_type", NullValueHandling = NullValueHandling.Ignore)]
        public string TraitType { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public string Value { get; set; }
    }

    public partial class Collection
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("family", NullValueHandling = NullValueHandling.Ignore)]
        public string Family { get; set; }
    }

    public partial class Properties
    {
        [JsonProperty("files", NullValueHandling = NullValueHandling.Ignore)]
        public File[] Files { get; set; }

    /*    [JsonProperty("category", NullValueHandling = NullValueHandling.Ignore)]
        public string Category { get; set; }

        [JsonProperty("creators", NullValueHandling = NullValueHandling.Ignore)]
        public Creator[] Creators { get; set; }*/
    }

    public partial class Creator
    {
        [JsonProperty("address", NullValueHandling = NullValueHandling.Ignore)]
        public string Address { get; set; }

        [JsonProperty("share", NullValueHandling = NullValueHandling.Ignore)]
        public long? Share { get; set; }
    }

    public partial class File
    {
        [JsonProperty("uri", NullValueHandling = NullValueHandling.Ignore)]
        public string Uri { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }
    }
}
