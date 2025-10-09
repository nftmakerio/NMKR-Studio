using Newtonsoft.Json;

namespace NMKR.Shared.Classes.EVM
{
    public partial class EvmContractDeployTransactionResultClass
    {
        [JsonProperty("nmkrProjectId", NullValueHandling = NullValueHandling.Ignore)]
        public long? NmkrProjectId { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("symbol", NullValueHandling = NullValueHandling.Ignore)]
        public string Symbol { get; set; }

        [JsonProperty("contractId", NullValueHandling = NullValueHandling.Ignore)]
        public string ContractId { get; set; }

        [JsonProperty("chainId", NullValueHandling = NullValueHandling.Ignore)]
        public string ChainId { get; set; }

        [JsonProperty("gas", NullValueHandling = NullValueHandling.Ignore)]
        public string Gas { get; set; }

        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public string Data { get; set; }

        [JsonProperty("internalSigningAddress", NullValueHandling = NullValueHandling.Ignore)]
        public string InternalSigningAddress { get; set; }

        [JsonProperty("feeAddress", NullValueHandling = NullValueHandling.Ignore)]
        public string FeeAddress { get; set; }

        [JsonProperty("success", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Success { get; set; }
    }
}