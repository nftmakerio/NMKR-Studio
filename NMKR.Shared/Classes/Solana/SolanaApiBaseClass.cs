using NMKR.Shared.Functions;
using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Solana
{
    public class SolanaApiBaseClass
    {
        [JsonProperty("payer", NullValueHandling = NullValueHandling.Ignore)]
        public SolanaKeysClass Payer { get; set; } = new SolanaKeysClass();

        [JsonProperty("network", NullValueHandling = NullValueHandling.Ignore)]
        public string Network { get; } = GlobalFunctions.IsMainnet() ? "mainnet" : "devnet";
    }
}
