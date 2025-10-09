using NMKR.Shared.Enums;
using Newtonsoft.Json;

namespace NMKR.Shared.Classes
{


    public class WhitelistetedCountClass
    {
        public string Address { get; set; }
        public long CountNft { get; set; }
        public string StakeAddress { get; set; }
    }

    public class GetSaleconditionsClass
    {
        [JsonProperty("condition", NullValueHandling = NullValueHandling.Ignore)]
        public SaleConditionsTypes Condition { get; set; }
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

        [JsonProperty("policyid6", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyId6 { get; set; }

        [JsonProperty("policyid7", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyId7 { get; set; }

        [JsonProperty("policyid8", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyId8 { get; set; }

        [JsonProperty("policyid9", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyId9 { get; set; }

        [JsonProperty("policyid10", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyId10 { get; set; }

        [JsonProperty("policyid11", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyId11 { get; set; }

        [JsonProperty("policyid12", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyId12 { get; set; }

        [JsonProperty("policyid13", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyId13 { get; set; }

        [JsonProperty("policyid14", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyId14 { get; set; }

        [JsonProperty("policyid15", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyId15 { get; set; }



        [JsonProperty("minormaxvalue", NullValueHandling = NullValueHandling.Ignore)]

        public long? MinOrMaxValue { get; set; }
        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }
        [JsonProperty("policyprojectname", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyProjectname { get; set; }
        [JsonProperty("whitelistedaddresses", NullValueHandling = NullValueHandling.Ignore)]

        public WhitelistetedCountClass[] WhitelistedAddresses { get; set; }
        [JsonProperty("blacklistedaddresses", NullValueHandling = NullValueHandling.Ignore)]
        public string[] BlacklistedAddresses { get; set; }

        [JsonProperty("onlyonesaleperwhitelistaddress", NullValueHandling = NullValueHandling.Ignore)]
        public bool? OnlyOneSalePerWhitelistAddress { get; set; }

        [JsonProperty("alreadyusedaddressorstakeaddress", NullValueHandling = NullValueHandling.Ignore)]
        public WhitelistetedCountClass[] AlreadyUsedAddressOrStakeaddress { get; set; }
    }

}
