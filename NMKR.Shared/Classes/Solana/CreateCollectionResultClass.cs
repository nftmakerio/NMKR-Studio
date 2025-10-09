using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Solana
{
    public partial class CreateCollectionResultClass
    {
        [JsonProperty("result", NullValueHandling = NullValueHandling.Ignore)]
        public string Result { get; set; }
    }
}