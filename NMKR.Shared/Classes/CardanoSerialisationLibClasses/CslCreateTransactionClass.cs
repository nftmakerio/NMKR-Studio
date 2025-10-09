using Newtonsoft.Json;

namespace NMKR.Shared.Classes.CardanoSerialisationLibClasses
{
    public partial class CslCreateTransactionClass
    {
        [JsonProperty("changeAddressBech32", NullValueHandling = NullValueHandling.Ignore)]
        public string ChangeAddressBech32 { get; set; }

        [JsonProperty("fees", NullValueHandling = NullValueHandling.Ignore)]
        public long? Fees { get; set; }

        [JsonProperty("includeMetadataHashOnly", NullValueHandling = NullValueHandling.Ignore)]
        public bool IncludeMetadataHashOnly { get; set; }

        [JsonProperty("mints", NullValueHandling = NullValueHandling.Ignore)]
        public Token[] Mints { get; set; }

        [JsonProperty("protocolParameters", NullValueHandling = NullValueHandling.Ignore)]
        public ProtocolParameters ProtocolParameters { get; set; }

        [JsonProperty("txIns", NullValueHandling = NullValueHandling.Ignore)]
        public TxIn[] TxIns { get; set; }

        [JsonProperty("txOuts", NullValueHandling = NullValueHandling.Ignore)]
        public TxOut[] TxOuts { get; set; }

        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public Metadatum[] Metadata { get; set; }

        [JsonProperty("ttl", NullValueHandling = NullValueHandling.Ignore)]
        public long? Ttl { get; set; }
        [JsonProperty("metadatastring", NullValueHandling = NullValueHandling.Ignore)]
        public string MetadataString { get; set; }
        [JsonProperty("referenceaddress", NullValueHandling = NullValueHandling.Ignore)]
        public string ReferenceAddress { get; set; }
    }

    public partial class Metadatum
    {
        [JsonProperty("key", NullValueHandling = NullValueHandling.Ignore)]
        public string Key { get; set; }

        [JsonProperty("json", NullValueHandling = NullValueHandling.Ignore)]
        public string Json { get; set; }
    }

    public partial class Token
    {
        [JsonProperty("policyId", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyId { get; set; }

        [JsonProperty("tokenName", NullValueHandling = NullValueHandling.Ignore)]
        public string TokenName { get; set; }

        [JsonProperty("count", NullValueHandling = NullValueHandling.Ignore)]
        public long? Count { get; set; }

        [JsonProperty("policyScriptJson", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyScriptJson { get; set; }
        [JsonProperty("metadata", NullValueHandling = NullValueHandling.Ignore)]
        public string Metadata { get; set; }

        public string PolicyIdAndTokenname => (PolicyId??"") + (TokenName??"");
    }

    public partial class ProtocolParameters
    {
        [JsonProperty("minFeeA", NullValueHandling = NullValueHandling.Ignore)]
        public long? MinFeeA { get; set; }

        [JsonProperty("minFeeB", NullValueHandling = NullValueHandling.Ignore)]
        public long? MinFeeB { get; set; }

        [JsonProperty("coinsPerUtxoWord", NullValueHandling = NullValueHandling.Ignore)]
        public long? CoinsPerUtxoWord { get; set; }

        [JsonProperty("poolDeposit", NullValueHandling = NullValueHandling.Ignore)]
        public long? PoolDeposit { get; set; }

        [JsonProperty("keyDeposit", NullValueHandling = NullValueHandling.Ignore)]
        public long? KeyDeposit { get; set; }

        [JsonProperty("maxValueSize", NullValueHandling = NullValueHandling.Ignore)]
        public long? MaxValueSize { get; set; }

        [JsonProperty("maxTxSize", NullValueHandling = NullValueHandling.Ignore)]
        public long? MaxTxSize { get; set; }
        [JsonProperty("coinsPerUtxoByte", NullValueHandling = NullValueHandling.Ignore)]
        public long? CoinsPerUtxoByte { get; set; }
        [JsonProperty("priceMemory", NullValueHandling = NullValueHandling.Ignore)]
        public double? PriceMemory { get; set; }
        [JsonProperty("priceStep", NullValueHandling = NullValueHandling.Ignore)]
        public double? PriceStep { get; set; }
        [JsonProperty("maxTxExMem", NullValueHandling = NullValueHandling.Ignore)]
        public long? MaxTxExMem { get; set; }
        [JsonProperty("maxTxExSteps", NullValueHandling = NullValueHandling.Ignore)]
        public long? MaxTxExSteps { get; set; }
    }

    public partial class TxIn
    {
        [JsonProperty("addressBech32", NullValueHandling = NullValueHandling.Ignore)]
        public string AddressBech32 { get; set; }

        [JsonProperty("lovelace", NullValueHandling = NullValueHandling.Ignore)]
        public long? Lovelace { get; set; }

        [JsonProperty("transactionHashAndIndex", NullValueHandling = NullValueHandling.Ignore)]
        public string TransactionHashAndIndex { get; set; }

        [JsonProperty("tokens", NullValueHandling = NullValueHandling.Ignore)]
        public Token[] Tokens { get; set; }

        public string TransactionHash => string.IsNullOrEmpty(TransactionHashAndIndex) ? "" : TransactionHashAndIndex.Split('#')[0];
        public uint TransactionIndex => string.IsNullOrEmpty(TransactionHashAndIndex) ? 0 : uint.Parse(TransactionHashAndIndex.Split('#')[1]);
    }

    public partial class TxOut
    {
        [JsonProperty("addressBech32", NullValueHandling = NullValueHandling.Ignore)]
        public string AddressBech32 { get; set; }

        [JsonProperty("lovelace", NullValueHandling = NullValueHandling.Ignore)]
        public long? Lovelace { get; set; }

        [JsonProperty("tokens", NullValueHandling = NullValueHandling.Ignore)]
        public Token[] Tokens { get; set; }

        public bool ReduceHereTheMintingcosts { get; set; } = false;
    }
}
