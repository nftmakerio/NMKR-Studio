using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Bitcoin
{
    public partial class BitcoinIncribePriceResultClass
    {
        [JsonProperty("chainFee", NullValueHandling = NullValueHandling.Ignore)]
        public long? ChainFee { get; set; }

        [JsonProperty("baseFee", NullValueHandling = NullValueHandling.Ignore)]
        public long? BaseFee { get; set; }

        [JsonProperty("rareSatsFee", NullValueHandling = NullValueHandling.Ignore)]
        public long? RareSatsFee { get; set; }

        [JsonProperty("additionalFee", NullValueHandling = NullValueHandling.Ignore)]
        public long? AdditionalFee { get; set; }

        [JsonProperty("count", NullValueHandling = NullValueHandling.Ignore)]
        public long? Count { get; set; }

        [JsonProperty("price", NullValueHandling = NullValueHandling.Ignore)]
        public long? Price { get; set; }

        [JsonProperty("collectionServiceFee", NullValueHandling = NullValueHandling.Ignore)]
        public long? CollectionServiceFee { get; set; }

        [JsonProperty("postage", NullValueHandling = NullValueHandling.Ignore)]
        public long? Postage { get; set; }

        [JsonProperty("additionalFeeCharged", NullValueHandling = NullValueHandling.Ignore)]
        public long? AdditionalFeeCharged { get; set; }

        [JsonProperty("discounts", NullValueHandling = NullValueHandling.Ignore)]
        public object[] Discounts { get; set; }

        [JsonProperty("amount", NullValueHandling = NullValueHandling.Ignore)]
        public long? Amount { get; set; }

        [JsonProperty("serviceFee", NullValueHandling = NullValueHandling.Ignore)]
        public long? ServiceFee { get; set; }
    }
}