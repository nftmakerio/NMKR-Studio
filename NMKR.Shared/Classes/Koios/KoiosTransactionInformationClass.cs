using Newtonsoft.Json;

namespace NMKR.Shared.Classes.Koios
{
    public partial class KoiosTransactionInformationClass
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
        public Put[] CollateralInputs { get; set; }

        [JsonProperty("collateral_output", NullValueHandling = NullValueHandling.Ignore)]
        public Put CollateralOutput { get; set; }

        [JsonProperty("reference_inputs", NullValueHandling = NullValueHandling.Ignore)]
        public Put[] ReferenceInputs { get; set; }

        [JsonProperty("inputs", NullValueHandling = NullValueHandling.Ignore)]
        public Put[] Inputs { get; set; }

        [JsonProperty("outputs", NullValueHandling = NullValueHandling.Ignore)]
        public Put[] Outputs { get; set; }

        [JsonProperty("withdrawals", NullValueHandling = NullValueHandling.Ignore)]
        public Withdrawal[] Withdrawals { get; set; }

        [JsonProperty("assets_minted", NullValueHandling = NullValueHandling.Ignore)]
        public Asset[] AssetsMinted { get; set; }

        [JsonProperty("certificates", NullValueHandling = NullValueHandling.Ignore)]
        public Certificate[] Certificates { get; set; }

        [JsonProperty("native_scripts", NullValueHandling = NullValueHandling.Ignore)]
        public NativeScript[] NativeScripts { get; set; }

        [JsonProperty("plutus_contracts", NullValueHandling = NullValueHandling.Ignore)]
        public PlutusContract[] PlutusContracts { get; set; }
    }

    public partial class Asset
    {
        [JsonProperty("policy_id", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyId { get; set; }

        [JsonProperty("asset_name", NullValueHandling = NullValueHandling.Ignore)]
        public string AssetName { get; set; }

        [JsonProperty("quantity", NullValueHandling = NullValueHandling.Ignore)]
        public long? Quantity { get; set; }

        [JsonProperty("fingerprint", NullValueHandling = NullValueHandling.Ignore)]
        public string Fingerprint { get; set; }
    }

    public partial class Certificate
    {
        [JsonProperty("index", NullValueHandling = NullValueHandling.Ignore)]
        public long? Index { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("info", NullValueHandling = NullValueHandling.Ignore)]
        public Info Info { get; set; }
    }

    public partial class Info
    {
        [JsonProperty("stake_address", NullValueHandling = NullValueHandling.Ignore)]
        public string StakeAddress { get; set; }

        [JsonProperty("pool", NullValueHandling = NullValueHandling.Ignore)]
        public string Pool { get; set; }
    }

    public partial class Put
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

    public partial class InlineDatum
    {
        [JsonProperty("bytes", NullValueHandling = NullValueHandling.Ignore)]
        public string Bytes { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public InlineDatumValue Value { get; set; }
    }

    public partial class InlineDatumValue
    {
        [JsonProperty("int", NullValueHandling = NullValueHandling.Ignore)]
        public long? Int { get; set; }
    }

    public partial class PaymentAddr
    {
        [JsonProperty("bech32", NullValueHandling = NullValueHandling.Ignore)]
        public string Bech32 { get; set; }

        [JsonProperty("cred", NullValueHandling = NullValueHandling.Ignore)]
        public string Cred { get; set; }
    }

    public partial class ReferenceScript
    {
        [JsonProperty("hash", NullValueHandling = NullValueHandling.Ignore)]
        public string Hash { get; set; }

        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public long? Size { get; set; }

        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("bytes", NullValueHandling = NullValueHandling.Ignore)]
        public string Bytes { get; set; }

    }
   
    public partial class NativeScript
    {
        [JsonProperty("script_hash", NullValueHandling = NullValueHandling.Ignore)]
        public string ScriptHash { get; set; }

        [JsonProperty("script_json", NullValueHandling = NullValueHandling.Ignore)]
        public ScriptJson ScriptJson { get; set; }
    }

    public partial class ScriptJson
    {
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("scripts", NullValueHandling = NullValueHandling.Ignore)]
        public Scriptx[] Scripts { get; set; }
    }

    public partial class Scriptx
    {
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string Type { get; set; }

        [JsonProperty("keyHash", NullValueHandling = NullValueHandling.Ignore)]
        public string KeyHash { get; set; }
    }

    public partial class PlutusContract
    {
        [JsonProperty("address", NullValueHandling = NullValueHandling.Ignore)]
        public string Address { get; set; }

        [JsonProperty("script_hash", NullValueHandling = NullValueHandling.Ignore)]
        public string ScriptHash { get; set; }

        [JsonProperty("bytecode", NullValueHandling = NullValueHandling.Ignore)]
        public string Bytecode { get; set; }

        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public long? Size { get; set; }

        [JsonProperty("valid_contract", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ValidContract { get; set; }

     /*   [JsonProperty("input", NullValueHandling = NullValueHandling.Ignore)]
        public Input Input { get; set; }

        [JsonProperty("output", NullValueHandling = NullValueHandling.Ignore)]
        public Output Output { get; set; }*/
    }

    public partial class Input
    {
        [JsonProperty("redeemer", NullValueHandling = NullValueHandling.Ignore)]
        public Redeemer Redeemer { get; set; }

        [JsonProperty("datum", NullValueHandling = NullValueHandling.Ignore)]
        public Datum Datum { get; set; }
    }
    public partial class Datum
    {
        [JsonProperty("hash", NullValueHandling = NullValueHandling.Ignore)]
        public string Hash { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public Value Value { get; set; }
    }
    public partial class Value
    {
        [JsonProperty("fields", NullValueHandling = NullValueHandling.Ignore)]
        public ValueField[] Fields { get; set; }

        [JsonProperty("constructor", NullValueHandling = NullValueHandling.Ignore)]
        public long? Constructor { get; set; }
    }
    public partial class ValueField
    {
        [JsonProperty("bytes", NullValueHandling = NullValueHandling.Ignore)]
        public string Bytes { get; set; }

        [JsonProperty("list", NullValueHandling = NullValueHandling.Ignore)]
        public Listx1[] List { get; set; }
    }
    public partial class Listx1
    {
        [JsonProperty("fields", NullValueHandling = NullValueHandling.Ignore)]
        public ListField[] Fields { get; set; }

        [JsonProperty("constructor", NullValueHandling = NullValueHandling.Ignore)]
        public long? Constructor { get; set; }
    }
    public partial class ListField
    {
        [JsonProperty("fields", NullValueHandling = NullValueHandling.Ignore)]
        public PurpleField[] Fields { get; set; }

        [JsonProperty("constructor", NullValueHandling = NullValueHandling.Ignore)]
        public long? Constructor { get; set; }

        [JsonProperty("map", NullValueHandling = NullValueHandling.Ignore)]
        public PurpleMap[] Map { get; set; }
    }
    public partial class PurpleField
    {
        [JsonProperty("fields", NullValueHandling = NullValueHandling.Ignore)]
        public FluffyField[] Fields { get; set; }

        [JsonProperty("constructor", NullValueHandling = NullValueHandling.Ignore)]
        public long? Constructor { get; set; }
    }
    public partial class FluffyField
    {
        [JsonProperty("bytes", NullValueHandling = NullValueHandling.Ignore)]
        public string Bytes { get; set; }

        [JsonProperty("fields", NullValueHandling = NullValueHandling.Ignore)]
        public TentacledField[] Fields { get; set; }

        [JsonProperty("constructor", NullValueHandling = NullValueHandling.Ignore)]
        public long? Constructor { get; set; }
    }
    public partial class TentacledField
    {
        [JsonProperty("fields", NullValueHandling = NullValueHandling.Ignore)]
        public K[] Fields { get; set; }

        [JsonProperty("constructor", NullValueHandling = NullValueHandling.Ignore)]
        public long? Constructor { get; set; }
    }
    public partial class K
    {
        [JsonProperty("bytes", NullValueHandling = NullValueHandling.Ignore)]
        public string Bytes { get; set; }
    }

    public partial class PurpleMap
    {
        [JsonProperty("k", NullValueHandling = NullValueHandling.Ignore)]
        public K K { get; set; }

        [JsonProperty("v", NullValueHandling = NullValueHandling.Ignore)]
        public PurpleV V { get; set; }
    }
    public partial class PurpleV
    {
        [JsonProperty("fields", NullValueHandling = NullValueHandling.Ignore)]
        public VField[] Fields { get; set; }

        [JsonProperty("constructor", NullValueHandling = NullValueHandling.Ignore)]
        public long? Constructor { get; set; }
    }

    public partial class VField
    {
        [JsonProperty("int", NullValueHandling = NullValueHandling.Ignore)]
        public long? Int { get; set; }

        [JsonProperty("map", NullValueHandling = NullValueHandling.Ignore)]
        public FluffyMap[] Map { get; set; }
    }

    public partial class FluffyMap
    {
        [JsonProperty("k", NullValueHandling = NullValueHandling.Ignore)]
        public K K { get; set; }

        [JsonProperty("v", NullValueHandling = NullValueHandling.Ignore)]
        public FluffyV V { get; set; }
    }

    public partial class FluffyV
    {
        [JsonProperty("int", NullValueHandling = NullValueHandling.Ignore)]
        public long? Int { get; set; }
    }
    public partial class Output
    {
        [JsonProperty("hash", NullValueHandling = NullValueHandling.Ignore)]
        public string Hash { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public OutputValue Value { get; set; }
    }

    public partial class OutputValue
    {
        [JsonProperty("bytes", NullValueHandling = NullValueHandling.Ignore)]
        public string Bytes { get; set; }
    }

    public partial class Redeemer
    {
        [JsonProperty("purpose", NullValueHandling = NullValueHandling.Ignore)]
        public string Purpose { get; set; }

        [JsonProperty("fee", NullValueHandling = NullValueHandling.Ignore)]
        public long? Fee { get; set; }

        [JsonProperty("unit", NullValueHandling = NullValueHandling.Ignore)]
        public Unit Unit { get; set; }

        [JsonProperty("datum", NullValueHandling = NullValueHandling.Ignore)]
        public Output Datum { get; set; }
    }

    public partial class Unit
    {
        [JsonProperty("steps", NullValueHandling = NullValueHandling.Ignore)]
        public long? Steps { get; set; }

        [JsonProperty("mem", NullValueHandling = NullValueHandling.Ignore)]
        public long? Mem { get; set; }
    }

    public partial class Withdrawal
    {
        [JsonProperty("amount", NullValueHandling = NullValueHandling.Ignore)]
        public long? Amount { get; set; }

        [JsonProperty("stake_addr", NullValueHandling = NullValueHandling.Ignore)]
        public StakeAddr StakeAddr { get; set; }
    }

    public partial class StakeAddr
    {
        [JsonProperty("bech32", NullValueHandling = NullValueHandling.Ignore)]
        public string Bech32 { get; set; }
    }
}