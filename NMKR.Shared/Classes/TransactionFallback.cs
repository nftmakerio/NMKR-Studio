using Newtonsoft.Json;

namespace NMKR.Shared.Classes
{
    public partial class TransactionFallback
    {
        [JsonProperty("tx_hash", NullValueHandling = NullValueHandling.Ignore)]
        public string TxHash { get; set; }

        [JsonProperty("block_hash", NullValueHandling = NullValueHandling.Ignore)]
        public string BlockHash { get; set; }

        [JsonProperty("block_height", NullValueHandling = NullValueHandling.Ignore)]
        public long? BlockHeight { get; set; }

        [JsonProperty("epoch", NullValueHandling = NullValueHandling.Ignore)]
        public long? Epoch { get; set; }

        [JsonProperty("epoch_slot", NullValueHandling = NullValueHandling.Ignore)]
        public long? EpochSlot { get; set; }

        [JsonProperty("absolute_slot", NullValueHandling = NullValueHandling.Ignore)]
        public long? AbsoluteSlot { get; set; }

        [JsonProperty("tx_timestamp", NullValueHandling = NullValueHandling.Ignore)]
        public string TxTimestamp { get; set; }

        [JsonProperty("tx_block_index", NullValueHandling = NullValueHandling.Ignore)]
        public long? TxBlockIndex { get; set; }

        [JsonProperty("tx_size", NullValueHandling = NullValueHandling.Ignore)]
        public long? TxSize { get; set; }

        [JsonProperty("total_output", NullValueHandling = NullValueHandling.Ignore)]
        public long? TotalOutput { get; set; }

        [JsonProperty("fee", NullValueHandling = NullValueHandling.Ignore)]
        public long? Fee { get; set; }

        [JsonProperty("deposit", NullValueHandling = NullValueHandling.Ignore)]
        public long? Deposit { get; set; }

        [JsonProperty("invalid_before")] public object InvalidBefore { get; set; }

        [JsonProperty("invalid_after", NullValueHandling = NullValueHandling.Ignore)]
        public long? InvalidAfter { get; set; }

        [JsonProperty("inputs", NullValueHandling = NullValueHandling.Ignore)]
        public InOutput[] Inputs { get; set; }

        [JsonProperty("outputs", NullValueHandling = NullValueHandling.Ignore)]
        public InOutput[] Outputs { get; set; }
    }

    public partial class InOutput
    {
        [JsonProperty("payment_addr", NullValueHandling = NullValueHandling.Ignore)]
        public PaymentAddr PaymentAddr { get; set; }

        [JsonProperty("stake_addr")] public string StakeAddr { get; set; }

        [JsonProperty("tx_hash", NullValueHandling = NullValueHandling.Ignore)]
        public string TxHash { get; set; }

        [JsonProperty("tx_index", NullValueHandling = NullValueHandling.Ignore)]
        public long? TxIndex { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public long? Value { get; set; }

    }

    public partial class PaymentAddr
    {
        [JsonProperty("bech32", NullValueHandling = NullValueHandling.Ignore)]
        public string Bech32 { get; set; }

        [JsonProperty("cred", NullValueHandling = NullValueHandling.Ignore)]
        public string Cred { get; set; }
    }
}

