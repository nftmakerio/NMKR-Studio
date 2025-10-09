using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Helius
{
    public partial class HeliusGetPriorityFeesTransactionClass
    {
        [JsonProperty("jsonrpc", NullValueHandling = NullValueHandling.Ignore)]
        public string Jsonrpc { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public string Id { get; set; }

        [JsonProperty("method", NullValueHandling = NullValueHandling.Ignore)]
        public string Method { get; set; }

        [JsonProperty("params", NullValueHandling = NullValueHandling.Ignore)]
        public HeliusGetPriorityFeesTransactionParam[] Params { get; set; }
    }

    public partial class HeliusGetPriorityFeesTransactionParam
    {
        [JsonProperty("transaction", NullValueHandling = NullValueHandling.Ignore)]
        public string Transaction { get; set; }

        [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
        public HeliusGetPriorityFeesTransactionOptions Options { get; set; }
    }

    public partial class HeliusGetPriorityFeesTransactionOptions
    {
        [JsonProperty("priorityLevel", NullValueHandling = NullValueHandling.Ignore)]
        public string PriorityLevel { get; set; }
    }
}