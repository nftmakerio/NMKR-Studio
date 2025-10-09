using Newtonsoft.Json;

namespace NMKR.Shared.Classes.AptosClasses
{
    public partial class AptosGetBalanceClass
    {
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public Data Data { get; set; }
    }

    public partial class Data
    {
        [JsonProperty("authentication_key", NullValueHandling = NullValueHandling.Ignore)]
        public string AuthenticationKey { get; set; }

        [JsonProperty("coin_register_events", NullValueHandling = NullValueHandling.Ignore)]
        public Events CoinRegisterEvents { get; set; }

        [JsonProperty("guid_creation_num", NullValueHandling = NullValueHandling.Ignore)]
        public long? GuidCreationNum { get; set; }

        [JsonProperty("key_rotation_events", NullValueHandling = NullValueHandling.Ignore)]
        public Events KeyRotationEvents { get; set; }

        [JsonProperty("rotation_capability_offer", NullValueHandling = NullValueHandling.Ignore)]
        public CapabilityOffer RotationCapabilityOffer { get; set; }

        [JsonProperty("sequence_number", NullValueHandling = NullValueHandling.Ignore)]
        public long? SequenceNumber { get; set; }

        [JsonProperty("signer_capability_offer", NullValueHandling = NullValueHandling.Ignore)]
        public CapabilityOffer SignerCapabilityOffer { get; set; }

        [JsonProperty("coin", NullValueHandling = NullValueHandling.Ignore)]
        public AptosCoin Coin { get; set; }

        [JsonProperty("deposit_events", NullValueHandling = NullValueHandling.Ignore)]
        public Events DepositEvents { get; set; }

        [JsonProperty("frozen", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Frozen { get; set; }

        [JsonProperty("withdraw_events", NullValueHandling = NullValueHandling.Ignore)]
        public Events WithdrawEvents { get; set; }
    }

    public partial class AptosCoin
    {
        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? Value { get; set; }
    }

    public partial class Events
    {
        [JsonProperty("counter", NullValueHandling = NullValueHandling.Ignore)]
        public ulong? Counter { get; set; }

        [JsonProperty("guid", NullValueHandling = NullValueHandling.Ignore)]
        public GuidClass Guid { get; set; }
    }

    public partial class GuidClass
    {
        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public Id Id { get; set; }
    }

    public partial class Id
    {
        [JsonProperty("addr", NullValueHandling = NullValueHandling.Ignore)]
        public string Addr { get; set; }

        [JsonProperty("creation_num", NullValueHandling = NullValueHandling.Ignore)]
        public long? CreationNum { get; set; }
    }

    public partial class CapabilityOffer
    {
        [JsonProperty("for", NullValueHandling = NullValueHandling.Ignore)]
        public For For { get; set; }
    }

    public partial class For
    {
        [JsonProperty("vec", NullValueHandling = NullValueHandling.Ignore)]
        public object[] Vec { get; set; }
    }
}
