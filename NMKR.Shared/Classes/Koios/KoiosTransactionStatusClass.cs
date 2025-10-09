using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Koios
{ 
    public partial class KoiosTransactionStatusClass
    {
        [JsonProperty("tx_hash", NullValueHandling = NullValueHandling.Ignore)]
        public string TxHash { get; set; }

        [JsonProperty("num_confirmations", NullValueHandling = NullValueHandling.Ignore)]
        public long? NumConfirmations { get; set; }
    }
}
