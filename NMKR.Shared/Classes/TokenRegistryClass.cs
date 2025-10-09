using Newtonsoft.Json;

namespace NMKR.Shared.Classes
{
    public partial class TokenRegistryClass
    {
        [JsonProperty("subject", NullValueHandling = NullValueHandling.Ignore)]
        public string Subject { get; set; }

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public Description Url { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public Description Name { get; set; }

        [JsonProperty("ticker", NullValueHandling = NullValueHandling.Ignore)]
        public Description Ticker { get; set; }

        [JsonProperty("decimals", NullValueHandling = NullValueHandling.Ignore)]
        public Decimals Decimals { get; set; }

        [JsonProperty("policy", NullValueHandling = NullValueHandling.Ignore)]
        public string Policy { get; set; }

        [JsonProperty("logo", NullValueHandling = NullValueHandling.Ignore)]
        public Description Logo { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public Description Description { get; set; }
    }

    public partial class Decimals
    {
        [JsonProperty("sequenceNumber", NullValueHandling = NullValueHandling.Ignore)]
        public long? SequenceNumber { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public int? Value { get; set; }

        [JsonProperty("signatures", NullValueHandling = NullValueHandling.Ignore)]
        public Signature[] Signatures { get; set; }
    }

    public partial class Signature
    {
        [JsonProperty("signature", NullValueHandling = NullValueHandling.Ignore)]
        public string SignatureSignature { get; set; }

        [JsonProperty("publicKey", NullValueHandling = NullValueHandling.Ignore)]
        public string PublicKey { get; set; }
    }

    public partial class Description
    {
        [JsonProperty("sequenceNumber", NullValueHandling = NullValueHandling.Ignore)]
        public long? SequenceNumber { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public string Value { get; set; }

        [JsonProperty("signatures", NullValueHandling = NullValueHandling.Ignore)]
        public Signature[] Signatures { get; set; }
    }
}
