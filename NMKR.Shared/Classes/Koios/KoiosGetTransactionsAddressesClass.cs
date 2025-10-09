using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Koios
{
    public partial class KoiosGetTransactionsAddressesClass
    {
        [JsonProperty("_addresses", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Addresses { get; set; }

        [JsonProperty("_after_block_height", NullValueHandling = NullValueHandling.Ignore)]
        public long? AfterBlockHeight { get; set; }
    }
}