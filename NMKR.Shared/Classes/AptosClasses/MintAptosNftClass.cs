using NMKR.Shared.Blockchains;
using Newtonsoft.Json;

public class MintAptosNftClass
{
    [JsonProperty("collection", NullValueHandling = NullValueHandling.Include)]
    public string Collection { get; set; } = "";

    [JsonProperty("description", NullValueHandling = NullValueHandling.Include)]
    public string Description { get; set; } = "";

    [JsonProperty("name", NullValueHandling = NullValueHandling.Include)]
    public string Name { get; set; } = "";

    [JsonProperty("uri", NullValueHandling = NullValueHandling.Include)]
    public string Uri { get; set; } = "";

    [JsonProperty("receiverAddress", NullValueHandling = NullValueHandling.Include)]
    public string ReceiverAddress { get; set; } = "";

    [JsonProperty("updateAuthority", NullValueHandling = NullValueHandling.Include)]
    public BlockchainKeysClass UpdateAuthority { get; set; } = new BlockchainKeysClass();

    [JsonProperty("payer", NullValueHandling = NullValueHandling.Include)]
    public BlockchainKeysClass Payer { get; set; } = new BlockchainKeysClass();

    [JsonProperty("network", NullValueHandling = NullValueHandling.Include)]
    public string Network { get; set; } = "testnet";
}

