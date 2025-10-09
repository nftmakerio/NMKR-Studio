using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Koios
{
    public partial class KoiosAccountInfoClass
    {
        [JsonProperty("stake_address", NullValueHandling = NullValueHandling.Ignore)]
        public string StakeAddress { get; set; }

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; set; }

        [JsonProperty("delegated_pool", NullValueHandling = NullValueHandling.Ignore)]
        public string DelegatedPool { get; set; }

        [JsonProperty("total_balance", NullValueHandling = NullValueHandling.Ignore)]
        public long? TotalBalance { get; set; }

        [JsonProperty("utxo", NullValueHandling = NullValueHandling.Ignore)]
        public string Utxo { get; set; }

        [JsonProperty("rewards", NullValueHandling = NullValueHandling.Ignore)]
        public long? Rewards { get; set; }

        [JsonProperty("withdrawals", NullValueHandling = NullValueHandling.Ignore)]
        public long? Withdrawals { get; set; }

        [JsonProperty("rewards_available", NullValueHandling = NullValueHandling.Ignore)]
        public long? RewardsAvailable { get; set; }

        [JsonProperty("reserves", NullValueHandling = NullValueHandling.Ignore)]
        public long? Reserves { get; set; }

        [JsonProperty("treasury", NullValueHandling = NullValueHandling.Ignore)]
        public long? Treasury { get; set; }
    }
}
