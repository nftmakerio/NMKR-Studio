using NMKR.Shared.Blockchains;
using Newtonsoft.Json;

namespace NMKR.Shared.Classes.AptosClasses
{
    public partial class TransferAptosClass
    {
        [JsonProperty("transfers", NullValueHandling = NullValueHandling.Ignore)]
        public TransferDetails[] Transfers { get; set; }

        [JsonProperty("payer", NullValueHandling = NullValueHandling.Include)]
        public BlockchainKeysClass Payer { get; set; } = new BlockchainKeysClass();

        [JsonProperty("network", NullValueHandling = NullValueHandling.Ignore)]
        public string Network { get; set; }
    }

    public partial class TransferDetails
    {
        [JsonProperty("receiverAddress", NullValueHandling = NullValueHandling.Ignore)]
        public string ReceiverAddress { get; set; }

        [JsonProperty("amount", NullValueHandling = NullValueHandling.Ignore)]
        public long? Amount { get; set; }
    }
}