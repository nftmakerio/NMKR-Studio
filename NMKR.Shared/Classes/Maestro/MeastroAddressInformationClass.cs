using System;
using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Maestro
{
    public partial class MeastroAddressInformationClass
    {
        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public MaestroDatum[] Data { get; set; }

        [JsonProperty("last_updated", NullValueHandling = NullValueHandling.Ignore)]
        public LastUpdated LastUpdated { get; set; }

        [JsonProperty("next_cursor")]
        public object NextCursor { get; set; }
    }

    public partial class MaestroDatum
    {
        [JsonProperty("tx_hash", NullValueHandling = NullValueHandling.Ignore)]
        public string TxHash { get; set; }

        [JsonProperty("index", NullValueHandling = NullValueHandling.Ignore)]
        public long? Index { get; set; }

        [JsonProperty("slot", NullValueHandling = NullValueHandling.Ignore)]
        public long? Slot { get; set; }

        [JsonProperty("assets", NullValueHandling = NullValueHandling.Ignore)]
        public MaestroAsset[] Assets { get; set; }

        [JsonProperty("address", NullValueHandling = NullValueHandling.Ignore)]
        public string Address { get; set; }

        [JsonProperty("datum")]
        public object DatumDatum { get; set; }

        [JsonProperty("reference_script")]
        public object ReferenceScript { get; set; }

        [JsonProperty("txout_cbor")]
        public object TxoutCbor { get; set; }
    }

    public partial class MaestroAsset
    {
        [JsonProperty("unit", NullValueHandling = NullValueHandling.Ignore)]
        public string Unit { get; set; }

        [JsonProperty("amount", NullValueHandling = NullValueHandling.Ignore)]
        public long? Amount { get; set; }
    }

    public partial class LastUpdated
    {
        [JsonProperty("timestamp", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? Timestamp { get; set; }

        [JsonProperty("block_hash", NullValueHandling = NullValueHandling.Ignore)]
        public string BlockHash { get; set; }

        [JsonProperty("block_slot", NullValueHandling = NullValueHandling.Ignore)]
        public long? BlockSlot { get; set; }
    }
}
