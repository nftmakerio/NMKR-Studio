using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Solana
{
    public class SolanaKeysClass
    {
        [JsonProperty("publicKey", NullValueHandling = NullValueHandling.Ignore)]
        public string PublicKey { get; set; } = "";

        [JsonProperty("secretKey", NullValueHandling = NullValueHandling.Ignore)]
        public string SecretKey { get; set; } = "";

        [JsonProperty("address", NullValueHandling = NullValueHandling.Ignore)]
        public string Address { get; set; } = "";
    }
}
