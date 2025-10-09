using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Bitcoin
{
    public partial class BitcoinSendCoinsClass
    {
        [JsonProperty("senderAddress", NullValueHandling = NullValueHandling.Ignore)]
        public string SenderAddress { get; set; }

        [JsonProperty("privateKey", NullValueHandling = NullValueHandling.Ignore)]
        public string PrivateKey { get; set; }

        [JsonProperty("receiverAddress", NullValueHandling = NullValueHandling.Ignore)]
        public string ReceiverAddress { get; set; }

        [JsonProperty("amountSats", NullValueHandling = NullValueHandling.Ignore)]
        public long? AmountSats { get; set; }

        [JsonProperty("feeRate", NullValueHandling = NullValueHandling.Ignore)]
        public long? FeeRate { get; set; }

        [JsonProperty("useTestnet", NullValueHandling = NullValueHandling.Ignore)]
        public bool? UseTestnet { get; set; }
    }
}