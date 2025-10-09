using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Koios
{
    public partial class KoiosStakeAddresses
    {
        [JsonProperty("address", NullValueHandling = NullValueHandling.Ignore)]
        public string Address { get; set; }
    }
}
