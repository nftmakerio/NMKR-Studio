using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Bitcoin
{
    public partial class BitcoinSendCoinsResultClass
    {
        [JsonProperty("success", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Success { get; set; }

        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public BitcoinSendCoinsResultData Data { get; set; }
    }

    public partial class BitcoinSendCoinsResultData
    {
        [JsonProperty("txid", NullValueHandling = NullValueHandling.Ignore)]
        public string Txid { get; set; }

        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public long? Size { get; set; }

        [JsonProperty("fees", NullValueHandling = NullValueHandling.Ignore)]
        public long? Fees { get; set; }

        [JsonProperty("amountSent", NullValueHandling = NullValueHandling.Ignore)]
        public long? AmountSent { get; set; }

        [JsonProperty("changeAmount", NullValueHandling = NullValueHandling.Ignore)]
        public long? ChangeAmount { get; set; }

        [JsonProperty("blockHeight", NullValueHandling = NullValueHandling.Ignore)]
        public long? BlockHeight { get; set; }

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; set; }
    }
}