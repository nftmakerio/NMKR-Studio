using System;
using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Bitcoin
{
    public partial class BitcoinOrderStatusResult
    {
        [JsonProperty("additionalFeeCharged", NullValueHandling = NullValueHandling.Ignore)]
        public long? AdditionalFeeCharged { get; set; }

        [JsonProperty("baseFee", NullValueHandling = NullValueHandling.Ignore)]
        public long? BaseFee { get; set; }

        [JsonProperty("chainFee", NullValueHandling = NullValueHandling.Ignore)]
        public long? ChainFee { get; set; }

        [JsonProperty("charge", NullValueHandling = NullValueHandling.Ignore)]
        public BitcoinOrderStatusResultCharge Charge { get; set; }

        [JsonProperty("createdAt", NullValueHandling = NullValueHandling.Ignore)]
        public long? CreatedAt { get; set; }

        [JsonProperty("fee", NullValueHandling = NullValueHandling.Ignore)]
        public long? Fee { get; set; }

        [JsonProperty("files", NullValueHandling = NullValueHandling.Ignore)]
        public BitcoinOrderStatusResultFile[] Files { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? Id { get; set; }

        [JsonProperty("orderType", NullValueHandling = NullValueHandling.Ignore)]
        public string OrderType { get; set; }

        [JsonProperty("payToAnchor", NullValueHandling = NullValueHandling.Ignore)]
        public bool? PayToAnchor { get; set; }

        [JsonProperty("postage", NullValueHandling = NullValueHandling.Ignore)]
        public long? Postage { get; set; }

        [JsonProperty("receiveAddress", NullValueHandling = NullValueHandling.Ignore)]
        public string ReceiveAddress { get; set; }

        [JsonProperty("serviceFee", NullValueHandling = NullValueHandling.Ignore)]
        public long? ServiceFee { get; set; }

        [JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
        public string State { get; set; }

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; set; }

        [JsonProperty("uncurseIt", NullValueHandling = NullValueHandling.Ignore)]
        public bool? UncurseIt { get; set; }
    }

    public partial class BitcoinOrderStatusResultCharge
    {
        [JsonProperty("address", NullValueHandling = NullValueHandling.Ignore)]
        public string Address { get; set; }

        [JsonProperty("amount", NullValueHandling = NullValueHandling.Ignore)]
        public long? Amount { get; set; }

        [JsonProperty("callback_url", NullValueHandling = NullValueHandling.Ignore)]
        public string CallbackUrl { get; set; }
    }

    public partial class BitcoinOrderStatusResultFile
    {
        [JsonProperty("metadataSize", NullValueHandling = NullValueHandling.Ignore)]
        public long? MetadataSize { get; set; }

        [JsonProperty("metadataUrl", NullValueHandling = NullValueHandling.Ignore)]
        public Uri MetadataUrl { get; set; }

        [JsonProperty("mimeType", NullValueHandling = NullValueHandling.Ignore)]
        public string MimeType { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("nonStandardTx", NullValueHandling = NullValueHandling.Ignore)]
        public bool? NonStandardTx { get; set; }

        [JsonProperty("s3Key", NullValueHandling = NullValueHandling.Ignore)]
        public string S3Key { get; set; }

        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public long? Size { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Url { get; set; }
    }
}
