using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Koios
{
    public partial class KoiosAccountAddressesClass
    {
        [JsonProperty("stake_address", NullValueHandling = NullValueHandling.Ignore)]
        public string StakeAddress { get; set; }

        [JsonProperty("addresses", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Addresses { get; set; }
    }
}