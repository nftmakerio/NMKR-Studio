using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Koios
{
    public partial class KoiosTxhash
    {
        [JsonProperty("_tx_hashes", NullValueHandling = NullValueHandling.Ignore)]
        public string[] TxHashes { get; set; }
    }

    public partial class KoiosDatumhash
    {
        [JsonProperty("_datum_hashes", NullValueHandling = NullValueHandling.Ignore)]
        public string[] DatumHashes { get; set; }
    }
}
