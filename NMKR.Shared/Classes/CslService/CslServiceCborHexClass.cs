using Newtonsoft.Json;

namespace NMKR.Shared.Classes.CslService
{
    public partial class CslServiceCborHexClass
    {
        [JsonProperty("cborHex", NullValueHandling = NullValueHandling.Ignore)]
        public string CborHex { get; set; }
    }
}