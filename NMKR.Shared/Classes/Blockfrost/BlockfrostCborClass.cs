using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Blockfrost
{
    public partial class BlockfrostCborClass
    {
        [JsonProperty("cbor", NullValueHandling = NullValueHandling.Ignore)]
        public string Cbor { get; set; }
    }
}