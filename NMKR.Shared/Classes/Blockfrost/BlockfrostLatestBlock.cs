using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Blockfrost
{
    public partial class BlockfrostLatestBlock
    {
        [JsonProperty("time", NullValueHandling = NullValueHandling.Ignore)]
        public long? Time { get; set; }

        [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
        public long? Height { get; set; }

        [JsonProperty("hash", NullValueHandling = NullValueHandling.Ignore)]
        public string Hash { get; set; }

        [JsonProperty("slot", NullValueHandling = NullValueHandling.Ignore)]
        public long? Slot { get; set; }

        [JsonProperty("epoch", NullValueHandling = NullValueHandling.Ignore)]
        public long? Epoch { get; set; }

        [JsonProperty("epoch_slot", NullValueHandling = NullValueHandling.Ignore)]
        public long? EpochSlot { get; set; }

        [JsonProperty("slot_leader", NullValueHandling = NullValueHandling.Ignore)]
        public string SlotLeader { get; set; }

        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public long? Size { get; set; }

        [JsonProperty("tx_count", NullValueHandling = NullValueHandling.Ignore)]
        public long? TxCount { get; set; }

        [JsonProperty("output", NullValueHandling = NullValueHandling.Ignore)]
        public string Output { get; set; }

        [JsonProperty("fees", NullValueHandling = NullValueHandling.Ignore)]
        public long? Fees { get; set; }

        [JsonProperty("block_vrf", NullValueHandling = NullValueHandling.Ignore)]
        public string BlockVrf { get; set; }

        [JsonProperty("op_cert", NullValueHandling = NullValueHandling.Ignore)]
        public string OpCert { get; set; }

        [JsonProperty("op_cert_counter", NullValueHandling = NullValueHandling.Ignore)]
        public long? OpCertCounter { get; set; }

        [JsonProperty("previous_block", NullValueHandling = NullValueHandling.Ignore)]
        public string PreviousBlock { get; set; }

        [JsonProperty("next_block")]
        public object NextBlock { get; set; }

        [JsonProperty("confirmations", NullValueHandling = NullValueHandling.Ignore)]
        public long? Confirmations { get; set; }
    }
}
