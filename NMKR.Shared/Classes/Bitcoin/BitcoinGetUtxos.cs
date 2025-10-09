using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Bitcoin
{
    public partial class BitcoinGetUtxos
    {
        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public BitcoinGetUtxosDatum[] Data { get; set; }

        [JsonProperty("last_updated", NullValueHandling = NullValueHandling.Ignore)]
        public BitcoinGetUtxosLastUpdated LastUpdated { get; set; }

        [JsonProperty("next_cursor")]
        public string NextCursor { get; set; }
    }

    public partial class BitcoinGetUtxosDatum
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
        public long? Satoshis { get; set; }

        [JsonProperty("confirmations", NullValueHandling = NullValueHandling.Ignore)]
        public long? Confirmations { get; set; }

        [JsonProperty("height", NullValueHandling = NullValueHandling.Ignore)]
        public long? Height { get; set; }

        [JsonProperty("runes", NullValueHandling = NullValueHandling.Ignore)]
        public object[] Runes { get; set; }

        [JsonProperty("inscriptions", NullValueHandling = NullValueHandling.Ignore)]
        public object[] Inscriptions { get; set; }
    }

    public partial class BitcoinGetUtxosLastUpdated
    {
        [JsonProperty("block_hash", NullValueHandling = NullValueHandling.Ignore)]
        public string BlockHash { get; set; }

        [JsonProperty("block_height", NullValueHandling = NullValueHandling.Ignore)]
        public long? BlockHeight { get; set; }
    }
}
