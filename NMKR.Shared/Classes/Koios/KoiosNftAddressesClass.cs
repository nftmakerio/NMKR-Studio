using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Koios
{
    public partial class KoiosNftAddressesClass
    {
        [JsonProperty("payment_address", NullValueHandling = NullValueHandling.Ignore)]
        public string PaymentAddress { get; set; }
    }
}