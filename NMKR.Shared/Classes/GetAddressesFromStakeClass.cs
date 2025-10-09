using Newtonsoft.Json;

namespace NMKR.Shared.Classes
{
    public partial class GetAddressesFromStakeClass
    {
        [JsonProperty("address")]
        public string Address { get; set; }
    }
}
