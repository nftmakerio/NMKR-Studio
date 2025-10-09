using NMKR.Shared.Functions;
using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Solana
{
    public class MintSolanaNftCoreClass : SolanaApiBaseClass
    {

        [JsonProperty("nftreceiveraddress", NullValueHandling = NullValueHandling.Ignore)]
        public string NftReceiverAddress { get; set; } = "";

        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public SolanaMetadataClass Metadata { get; set; } = new SolanaMetadataClass();

        [JsonProperty("collectionaddress", NullValueHandling = NullValueHandling.Ignore)]
        public string CollectionAddress { get; set; } = "";
        [JsonProperty("updateAuthority", NullValueHandling = NullValueHandling.Ignore)]
        public SolanaKeysClass UpdateAuthority { get; set; } = new SolanaKeysClass();

        [JsonProperty("network", NullValueHandling = NullValueHandling.Ignore)]
        public string Network { get; } = GlobalFunctions.IsMainnet() ? "mainnet" : "devnet";

        [JsonProperty("creators", NullValueHandling = NullValueHandling.Ignore)]
        public CreatorsClass[] Creators { get; set; }

        [JsonProperty("computeUnitPrice", NullValueHandling = NullValueHandling.Ignore)]
        public ulong ComputeUnitPrice { get; set; }

        [JsonProperty("computeUnitLimit", NullValueHandling = NullValueHandling.Ignore)]
        public ulong ComputeUnitLimit { get; set; }
    }
}
