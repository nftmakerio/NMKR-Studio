using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Koios
{
    public partial class KoiosTransactionClass
    {
        [JsonProperty("tx_hash", NullValueHandling = NullValueHandling.Ignore)]
        public string TxHash { get; set; }

        [JsonProperty("block_hash", NullValueHandling = NullValueHandling.Ignore)]
        public string BlockHash { get; set; }

        [JsonProperty("block_height", NullValueHandling = NullValueHandling.Ignore)]
        public long? BlockHeight { get; set; }

        [JsonProperty("epoch_no", NullValueHandling = NullValueHandling.Ignore)]
        public long? EpochNo { get; set; }

        [JsonProperty("epoch_slot", NullValueHandling = NullValueHandling.Ignore)]
        public long? EpochSlot { get; set; }

        [JsonProperty("absolute_slot", NullValueHandling = NullValueHandling.Ignore)]
        public long? AbsoluteSlot { get; set; }

        [JsonProperty("tx_timestamp", NullValueHandling = NullValueHandling.Ignore)]
        public long? TxTimestamp { get; set; }

        [JsonProperty("tx_block_index", NullValueHandling = NullValueHandling.Ignore)]
        public long? TxBlockIndex { get; set; }

        [JsonProperty("tx_size", NullValueHandling = NullValueHandling.Ignore)]
        public long? TxSize { get; set; }

        [JsonProperty("total_output", NullValueHandling = NullValueHandling.Ignore)]
        public long? TotalOutput { get; set; }

        [JsonProperty("fee", NullValueHandling = NullValueHandling.Ignore)]
        public long? Fee { get; set; }

        [JsonProperty("deposit", NullValueHandling = NullValueHandling.Ignore)]
        public string Deposit { get; set; }

        [JsonProperty("invalid_before", NullValueHandling = NullValueHandling.Ignore)]
        public long? InvalidBefore { get; set; }

        [JsonProperty("invalid_after", NullValueHandling = NullValueHandling.Ignore)]
        public long? InvalidAfter { get; set; }

        [JsonProperty("collateral_inputs", NullValueHandling = NullValueHandling.Ignore)]
        public InOutputs[] CollateralInputs { get; set; }

        [JsonProperty("collateral_outputs", NullValueHandling = NullValueHandling.Ignore)]
        public InOutputs[] CollateralOutputs { get; set; }

        [JsonProperty("reference_inputs", NullValueHandling = NullValueHandling.Ignore)]
        public InOutputs[] ReferenceInputs { get; set; }

        [JsonProperty("inputs", NullValueHandling = NullValueHandling.Ignore)]
        public InOutputs[] Inputs { get; set; }

        [JsonProperty("outputs", NullValueHandling = NullValueHandling.Ignore)]
        public InOutputs[] Outputs { get; set; }


        [JsonProperty("assets_minted", NullValueHandling = NullValueHandling.Ignore)]
        public KoiosTransactionAssetsClass[] AssetsMinted { get; set; }

      

        [JsonProperty("certificates", NullValueHandling = NullValueHandling.Ignore)]
        public Certificate[] Certificates { get; set; }

        [JsonProperty("native_scripts", NullValueHandling = NullValueHandling.Ignore)]
        public NativeScript[] NativeScripts { get; set; }

        [JsonProperty("plutus_contracts", NullValueHandling = NullValueHandling.Ignore)]
        public PlutusContract[] PlutusContracts { get; set; }
    }
    public partial class InOutputs
    {
        [JsonProperty("payment_addr", NullValueHandling = NullValueHandling.Ignore)]
        public PaymentAddr PaymentAddr { get; set; }

        [JsonProperty("stake_addr", NullValueHandling = NullValueHandling.Ignore)]
        public string StakeAddr { get; set; }

        [JsonProperty("tx_hash", NullValueHandling = NullValueHandling.Ignore)]
        public string TxHash { get; set; }

        [JsonProperty("tx_index", NullValueHandling = NullValueHandling.Ignore)]
        public long? TxIndex { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public long? Value { get; set; }

        [JsonProperty("datum_hash", NullValueHandling = NullValueHandling.Ignore)]
        public string DatumHash { get; set; }

        [JsonProperty("inline_datum", NullValueHandling = NullValueHandling.Ignore)]
        public InlineDatum InlineDatum { get; set; }

        [JsonProperty("reference_script", NullValueHandling = NullValueHandling.Ignore)]
        public ReferenceScript ReferenceScript { get; set; }

        [JsonProperty("asset_list", NullValueHandling = NullValueHandling.Ignore)]
        public Asset[] AssetList { get; set; }
    }


    public partial class KoiosTransactionAssetsClass
    {
        [JsonProperty("policy_id", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyId { get; set; }

        [JsonProperty("asset_name", NullValueHandling = NullValueHandling.Ignore)]
        public string AssetName { get; set; }

        [JsonProperty("quantity", NullValueHandling = NullValueHandling.Ignore)]
        public long? Quantity { get; set; }
    }


    public partial class KoiosTransactionInfoClass
    {
        [JsonProperty("stake_address", NullValueHandling = NullValueHandling.Ignore)]
        public string StakeAddress { get; set; }

        [JsonProperty("pool", NullValueHandling = NullValueHandling.Ignore)]
        public string Pool { get; set; }
    }


    public partial class KoiosTransactionPaymentAddrClass
    {
        [JsonProperty("bech32", NullValueHandling = NullValueHandling.Ignore)]
        public string Bech32 { get; set; }

        [JsonProperty("cred", NullValueHandling = NullValueHandling.Ignore)]
        public string Cred { get; set; }
    }

    public partial class KoiosTransactionScriptClass
    {
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("keyHash", NullValueHandling = NullValueHandling.Ignore)]
        public string KeyHash { get; set; }
    }
}

