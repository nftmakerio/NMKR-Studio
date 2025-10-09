using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Solana;

public class SolanaVerifyCollectionClass : SolanaApiBaseClass
{
    [JsonProperty("nftMintAddress", NullValueHandling = NullValueHandling.Ignore)]
    public string NftMintAddress { get; set; } = "";

    [JsonProperty("collectionaddress", NullValueHandling = NullValueHandling.Ignore)]
    public string CollectionAddress { get; set; } = "";


    [JsonProperty("updateAuthority", NullValueHandling = NullValueHandling.Ignore)]
    public SolanaKeysClass UpdateAuthority { get; set; } = new SolanaKeysClass();
  
}