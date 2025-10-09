using System;
using Newtonsoft.Json;

namespace NMKR.Shared.Classes.EVM
{
    public partial class EvmContractLinkProjectClass
    {
        [JsonProperty("txHash", NullValueHandling = NullValueHandling.Ignore)]
        public string TxHash { get; set; }

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

        [JsonProperty("baseURI", NullValueHandling = NullValueHandling.Ignore)]
        public Uri BaseUri { get; set; }

        [JsonProperty("feeAccountAddress", NullValueHandling = NullValueHandling.Ignore)]
        public string FeeAccountAddress { get; set; }

        [JsonProperty("lockingTime", NullValueHandling = NullValueHandling.Ignore)]
        public long? LockingTime { get; set; }

        [JsonProperty("maxNFTs", NullValueHandling = NullValueHandling.Ignore)]
        public long? MaxNfTs { get; set; }

        [JsonProperty("metadataURI", NullValueHandling = NullValueHandling.Ignore)]
        public Uri MetadataUri { get; set; }

        [JsonProperty("royaltyAddress", NullValueHandling = NullValueHandling.Ignore)]
        public string RoyaltyAddress { get; set; }

        [JsonProperty("royaltyFee", NullValueHandling = NullValueHandling.Ignore)]
        public long? RoyaltyFee { get; set; }
    }
}
