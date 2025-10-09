using System;
using System.Runtime.Serialization;

namespace NMKR.Shared.Classes
{
    [Serializable]
    public class NmkrAssetAddress 
    {
        public string PolicyId { get; set; }
        public string AssetName { get; set; }
        public string AssetNameInHex { get; set; }
        public string Fingerprint { get; set; }
        public long? TotalSupply { get; set; }
        public long Multiplier { get; set; }
        public string Address { get; set; }
        public long Quantity { get; set; }
        public long Decimals { get; set; }
        public long? CreationTime { get; set; }
        public string MintingTxHash { get; set; }
        public MintingTransactionInformation MintingTransactionInformation { get; set; }
        public string SolanaSymbol { get; internal set; }
        public string SolanaDescription { get; internal set; }
        public string SolanaTokenStandard { get; internal set; }
    }
    [Serializable]
    public class NmkrAssetPolicySnapshot 
    {
        public NmkrAssetAddress[] AssetsOnStakeAddress { get; set; }
        [DataMember]
        public string StakeAddress { get; set; }
        [DataMember]
        public string Address { get; set; }
      
        [DataMember]
        public long TotalQuantity { get; set; }
    }

    public class MintingTransactionInformation
    {
        public string Address { get; set; }
        public long Quantity { get; set; }
        public long? Slot { get; set; }
    }
}
