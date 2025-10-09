using System;
using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Bitcoin
{
    public partial class BitcoinInscribeResult
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? Id { get; set; }

        [JsonProperty("charge", NullValueHandling = NullValueHandling.Ignore)]
        public Charge Charge { get; set; }

        [JsonProperty("chainFee", NullValueHandling = NullValueHandling.Ignore)]
        public long? ChainFee { get; set; }

        [JsonProperty("serviceFee", NullValueHandling = NullValueHandling.Ignore)]
        public long? ServiceFee { get; set; }

        [JsonProperty("fee", NullValueHandling = NullValueHandling.Ignore)]
        public long? Fee { get; set; }

        [JsonProperty("baseFee", NullValueHandling = NullValueHandling.Ignore)]
        public long? BaseFee { get; set; }

        [JsonProperty("postage", NullValueHandling = NullValueHandling.Ignore)]
        public long? Postage { get; set; }

        [JsonProperty("additionalFeeCharged", NullValueHandling = NullValueHandling.Ignore)]
        public long? AdditionalFeeCharged { get; set; }

        [JsonProperty("files", NullValueHandling = NullValueHandling.Ignore)]
        public BitcoinInscribeResultFile[] Files { get; set; }

        [JsonProperty("delegates")]
        public object Delegates { get; set; }

        [JsonProperty("parents")]
        public object Parents { get; set; }

        [JsonProperty("inscriptionIdPrefix")]
        public object InscriptionIdPrefix { get; set; }

        [JsonProperty("allowedSatributes")]
        public object AllowedSatributes { get; set; }

        [JsonProperty("additionalFee")]
        public object AdditionalFee { get; set; }

        [JsonProperty("referral")]
        public object Referral { get; set; }

        [JsonProperty("receiveAddress", NullValueHandling = NullValueHandling.Ignore)]
        public string ReceiveAddress { get; set; }

        [JsonProperty("webhookUrl")]
        public object WebhookUrl { get; set; }

        [JsonProperty("projectTag")]
        public object ProjectTag { get; set; }

        [JsonProperty("zeroConf")]
        public object ZeroConf { get; set; }

        [JsonProperty("payToAnchor", NullValueHandling = NullValueHandling.Ignore)]
        public bool? PayToAnchor { get; set; }

        [JsonProperty("uncurseIt", NullValueHandling = NullValueHandling.Ignore)]
        public bool? UncurseIt { get; set; }

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; set; }

        [JsonProperty("orderType", NullValueHandling = NullValueHandling.Ignore)]
        public string OrderType { get; set; }

        [JsonProperty("state", NullValueHandling = NullValueHandling.Ignore)]
        public string State { get; set; }

        [JsonProperty("createdAt", NullValueHandling = NullValueHandling.Ignore)]
        public CreatedAt CreatedAt { get; set; }
    }

    public partial class Charge
    {
        [JsonProperty("amount", NullValueHandling = NullValueHandling.Ignore)]
        public long? Amount { get; set; }
    }

    public partial class CreatedAt
    {
        [JsonProperty(".sv", NullValueHandling = NullValueHandling.Ignore)]
        public string Sv { get; set; }
    }

    public partial class BitcoinInscribeResultFile
    {
        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public long? Size { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("metadataSize", NullValueHandling = NullValueHandling.Ignore)]
        public long? MetadataSize { get; set; }

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Url { get; set; }

        [JsonProperty("s3Key", NullValueHandling = NullValueHandling.Ignore)]
        public string S3Key { get; set; }

        [JsonProperty("metadataUrl", NullValueHandling = NullValueHandling.Ignore)]
        public Uri MetadataUrl { get; set; }

        [JsonProperty("nonStandardTx", NullValueHandling = NullValueHandling.Ignore)]
        public bool? NonStandardTx { get; set; }

        [JsonProperty("mimeType", NullValueHandling = NullValueHandling.Ignore)]
        public string MimeType { get; set; }
    }
}
