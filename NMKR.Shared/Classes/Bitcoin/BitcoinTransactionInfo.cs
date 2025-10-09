using System;
using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Bitcoin
{
    public partial class BitcoinTransactionInfo
    {
        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public BitcoinTransactionInfoData Data { get; set; }

        [JsonProperty("last_updated", NullValueHandling = NullValueHandling.Ignore)]
        public LastUpdated LastUpdated { get; set; }
    }

    public partial class BitcoinTransactionInfoData
    {
        [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
        public long? Height { get; set; }

        [JsonProperty("block_hash", NullValueHandling = NullValueHandling.Ignore)]
        public string BlockHash { get; set; }

        [JsonProperty("confirmations", NullValueHandling = NullValueHandling.Ignore)]
        public long? Confirmations { get; set; }

        [JsonProperty("unix_timestamp", NullValueHandling = NullValueHandling.Ignore)]
        public long? UnixTimestamp { get; set; }

        [JsonProperty("timestamp", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? Timestamp { get; set; }

        [JsonProperty("tx_index", NullValueHandling = NullValueHandling.Ignore)]
        public long? TxIndex { get; set; }

        [JsonProperty("volume", NullValueHandling = NullValueHandling.Ignore)]
        public string Volume { get; set; }

        [JsonProperty("fees", NullValueHandling = NullValueHandling.Ignore)]
        public long? Fees { get; set; }

        [JsonProperty("sats_per_vb", NullValueHandling = NullValueHandling.Ignore)]
        public long? SatsPerVb { get; set; }

        [JsonProperty("metaprotocols", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Metaprotocols { get; set; }

        [JsonProperty("inputs", NullValueHandling = NullValueHandling.Ignore)]
        public BitcoinTransactionInfoInput[] Inputs { get; set; }

        [JsonProperty("outputs", NullValueHandling = NullValueHandling.Ignore)]
        public BitcoinTransactionInfoOutput[] Outputs { get; set; }
    }

    public partial class BitcoinTransactionInfoInput
    {
        [JsonProperty("txid", NullValueHandling = NullValueHandling.Ignore)]
        public string Txid { get; set; }

        [JsonProperty("vout", NullValueHandling = NullValueHandling.Ignore)]
        public long? Vout { get; set; }

        [JsonProperty("address", NullValueHandling = NullValueHandling.Ignore)]
        public string Address { get; set; }

        [JsonProperty("script_pubkey", NullValueHandling = NullValueHandling.Ignore)]
        public string ScriptPubkey { get; set; }

        [JsonProperty("satoshis", NullValueHandling = NullValueHandling.Ignore)]
        public string Satoshis { get; set; }
    }

    public partial class BitcoinTransactionInfoOutput
    {
        [JsonProperty("address", NullValueHandling = NullValueHandling.Ignore)]
        public string Address { get; set; }

        [JsonProperty("script_pubkey", NullValueHandling = NullValueHandling.Ignore)]
        public string ScriptPubkey { get; set; }

        [JsonProperty("satoshis", NullValueHandling = NullValueHandling.Ignore)]
        public string Satoshis { get; set; }

        [JsonProperty("spending_tx", NullValueHandling = NullValueHandling.Ignore)]
        public string SpendingTx { get; set; }
    }
}
