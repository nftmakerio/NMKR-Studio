using Newtonsoft.Json;

namespace NMKR.Shared.Classes.EVM
{
    public partial class EvmContractLinkProjectResultClass
    {
        [JsonProperty("success", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Success { get; set; }
    }
}