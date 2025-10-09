using NMKR.Shared.Functions;
using Newtonsoft.Json;

namespace NMKR.Shared.Classes
{
    public class GetDiscountsClass
    {
        [JsonProperty("condition", NullValueHandling = NullValueHandling.Ignore)]
        public PricelistDiscountTypes Condition { get; set; }
        [JsonProperty("policyid1", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyId1 { get; set; }
        [JsonProperty("policyid2", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyId2 { get; set; }
        [JsonProperty("policyid3", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyId3 { get; set; }
        [JsonProperty("policyid4", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyId4 { get; set; }
        [JsonProperty("policyid5", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyId5 { get; set; }
        [JsonProperty("minormaxvalue", NullValueHandling = NullValueHandling.Ignore)]
        public long? MinOrMaxValue { get; set; }
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }
        [JsonProperty("discountinpercent", NullValueHandling = NullValueHandling.Ignore)]
        public float DiscountInPercent { get; set; }

        public long? MinValue1 { get; set; }
        public long? MinValue2 { get; set; }
        public long? MinValue3 { get; set; }
        public long? MinValue4 { get; set; }
        public long? MinValue5 { get; set; }
        public string Operator { get; set; }
        public string Couponcode { get; set; }
    }
}
