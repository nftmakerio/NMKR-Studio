using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Koios
{
    public partial class KoiosAssetAddressListClass
    {
        [JsonProperty("payment_address", NullValueHandling = NullValueHandling.Ignore)]
        public string PaymentAddress { get; set; }

        [JsonProperty("quantity", NullValueHandling = NullValueHandling.Ignore)]
        public long? Quantity { get; set; }
    }
}
