using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Solana
{
    public partial class SolanaVerifiyCollectionResultClass
    {
        [JsonProperty("result", NullValueHandling = NullValueHandling.Ignore)]
        public Result Result { get; set; }
    }

    public partial class Result
    {
        [JsonProperty("response", NullValueHandling = NullValueHandling.Ignore)]
        public Response Response { get; set; }
    }

    public partial class Response
    {
        [JsonProperty("signature", NullValueHandling = NullValueHandling.Ignore)]
        public string Signature { get; set; }

        [JsonProperty("confirmResponse", NullValueHandling = NullValueHandling.Ignore)]
        public ConfirmResponse ConfirmResponse { get; set; }

        [JsonProperty("blockhash", NullValueHandling = NullValueHandling.Ignore)]
        public string Blockhash { get; set; }

        [JsonProperty("lastValidBlockHeight", NullValueHandling = NullValueHandling.Ignore)]
        public long? LastValidBlockHeight { get; set; }
    }

    public partial class ConfirmResponse
    {
        [JsonProperty("context", NullValueHandling = NullValueHandling.Ignore)]
        public Context Context { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public Value Value { get; set; }
    }

    public partial class Context
    {
        [JsonProperty("slot", NullValueHandling = NullValueHandling.Ignore)]
        public long? Slot { get; set; }
    }

    public partial class Value
    {
        [JsonProperty("err")]
        public object Err { get; set; }
    }
}