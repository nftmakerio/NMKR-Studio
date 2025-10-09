using System.Text.Json.Serialization;


namespace NMKR.Shared.NotificationClasses
{
    /// <summary>
    /// The Class for the OURA Kafka Notification Messages
    /// We need to use System.Text.Json, because it does not use the Newtonsoft JSON Property Informations !!!
    /// </summary>
    public partial class OuraNotificationClass
        {
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("context")]
            public OuraNotificationContext Context { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("transaction")]
            public OuraNotificationTransaction Transaction { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("fingerprint")]
            public string Fingerprint { get; set; }
        }

        public partial class OuraNotificationContext
    {
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("block_hash")]
            public string BlockHash { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("block_number")]
            public long? BlockNumber { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("slot")]
            public long? Slot { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("timestamp")]
            public long? Timestamp { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("tx_idx")]
            public long? TxIdx { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("tx_hash")]
            public string TxHash { get; set; }

            [JsonPropertyName("input_idx")]
            public object InputIdx { get; set; }

            [JsonPropertyName("output_idx")]
            public object OutputIdx { get; set; }

            [JsonPropertyName("output_address")]
            public object OutputAddress { get; set; }

            [JsonPropertyName("certificate_idx")]
            public object CertificateIdx { get; set; }
        }

        public partial class OuraNotificationTransaction
    {
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("hash")]
            public string Hash { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("fee")]
            public long? Fee { get; set; }

            [JsonPropertyName("ttl")]
            public object Ttl { get; set; }

            [JsonPropertyName("validity_interval_start")]
            public object ValidityIntervalStart { get; set; }

            [JsonPropertyName("network_id")]
            public object NetworkId { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("input_count")]
            public long? InputCount { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("output_count")]
            public long? OutputCount { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("mint_count")]
            public long? MintCount { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("total_output")]
            public long? TotalOutput { get; set; }

            [JsonPropertyName("metadata")]
            public object Metadata { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("inputs")]
            public OuraNotificationInput[] Inputs { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("outputs")]
            public OuraNotificationOutput[] Outputs { get; set; }

            [JsonPropertyName("mint")]
            public object Mint { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("vkey_witnesses")]
            public OuraNotificationVkeyWitness[] VkeyWitnesses { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("native_witnesses")]
            public object[] NativeWitnesses { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("plutus_witnesses")]
            public object[] PlutusWitnesses { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("plutus_redeemers")]
            public object[] PlutusRedeemers { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("plutus_data")]
            public object[] PlutusData { get; set; }

            [JsonPropertyName("withdrawals")]
            public object Withdrawals { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("size")]
            public long? Size { get; set; }
        }

        public partial class OuraNotificationInput
    {
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("tx_id")]
            public string TxId { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("index")]
            public long? Index { get; set; }
        }

        public partial class OuraNotificationOutput
    {
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("address")]
            public string Address { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("amount")]
            public long? Amount { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("assets")]
            public OuraNotificationAsset[] Assets { get; set; }

            [JsonPropertyName("datum_hash")]
            public string DatumHash { get; set; }
        }

        public partial class OuraNotificationAsset
    {
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("policy")]
            public string Policy { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("asset")]
            public string Asset { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("asset_ascii")]
            public string AssetAscii { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("amount")]
            public long? Amount { get; set; }
        }

        public partial class OuraNotificationVkeyWitness
    {
            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("vkey_hex")]
            public string VkeyHex { get; set; }

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
            [JsonPropertyName("signature_hex")]
            public string SignatureHex { get; set; }
        }
    }


    /*


    public partial class OuraNotificationClass
    {
        [JsonProperty("context", NullValueHandling = NullValueHandling.Ignore)]
        public OuraNotificationContext Context { get; set; }

        [JsonProperty("transaction", NullValueHandling = NullValueHandling.Ignore)]
        public OuraNotificationTransaction Transaction { get; set; }

        [JsonProperty("fingerprint", NullValueHandling = NullValueHandling.Ignore)]
        public string Fingerprint { get; set; }
    }

    public partial class OuraNotificationContext
    {
        [JsonProperty("block_hash", NullValueHandling = NullValueHandling.Ignore)]
        public string BlockHash { get; set; }

        [JsonProperty("block_number", NullValueHandling = NullValueHandling.Ignore)]
        public long? BlockNumber { get; set; }

        [JsonProperty("slot", NullValueHandling = NullValueHandling.Ignore)]
        public long? Slot { get; set; }

        [JsonProperty("timestamp", NullValueHandling = NullValueHandling.Ignore)]
        public long? Timestamp { get; set; }

        [JsonProperty("tx_idx", NullValueHandling = NullValueHandling.Ignore)]
        public long? TxIdx { get; set; }

        [JsonProperty("tx_hash", NullValueHandling = NullValueHandling.Ignore)]
        public string TxHash { get; set; }

        [JsonProperty("input_idx")] public object InputIdx { get; set; }

        [JsonProperty("output_idx")] public object OutputIdx { get; set; }

        [JsonProperty("output_address")] public object OutputAddress { get; set; }

        [JsonProperty("certificate_idx")] public object CertificateIdx { get; set; }
    }

    public partial class OuraNotificationTransaction
    {
        [JsonProperty("hash", NullValueHandling = NullValueHandling.Ignore)]
        public string Hash { get; set; }

        [JsonProperty("fee", NullValueHandling = NullValueHandling.Ignore)]
        public long? Fee { get; set; }

        [JsonProperty("ttl", NullValueHandling = NullValueHandling.Ignore)]
        public long? Ttl { get; set; }

        [JsonProperty("validity_interval_start", NullValueHandling = NullValueHandling.Ignore)]
        public long? ValidityIntervalStart { get; set; }

        [JsonProperty("network_id")] public object NetworkId { get; set; }

        [JsonProperty("input_count", NullValueHandling = NullValueHandling.Ignore)]
        public long? InputCount { get; set; }

        [JsonProperty("output_count", NullValueHandling = NullValueHandling.Ignore)]
        public long? OutputCount { get; set; }

        [JsonProperty("mint_count", NullValueHandling = NullValueHandling.Ignore)]
        public long? MintCount { get; set; }

        [JsonProperty("total_output", NullValueHandling = NullValueHandling.Ignore)]
        public long? TotalOutput { get; set; }

        [JsonProperty("metadata")] public object Metadata { get; set; }

        [JsonProperty("inputs", NullValueHandling = NullValueHandling.Ignore)]
        public OuraNotificationInput[] Inputs { get; set; }

        [JsonProperty("outputs", NullValueHandling = NullValueHandling.Ignore)]
        public OuraNotificationOutput[] Outputs { get; set; }

        [JsonProperty("mint")] public object Mint { get; set; }

        [JsonProperty("vkey_witnesses", NullValueHandling = NullValueHandling.Ignore)]
        public OuraNotificationVkeyWitness[] VkeyWitnesses { get; set; }

        [JsonProperty("native_witnesses", NullValueHandling = NullValueHandling.Ignore)]
        public object[] NativeWitnesses { get; set; }

        [JsonProperty("plutus_witnesses", NullValueHandling = NullValueHandling.Ignore)]
        public OuraNotificationPlutusWitness[] PlutusWitnesses { get; set; }

        [JsonProperty("plutus_redeemers", NullValueHandling = NullValueHandling.Ignore)]
        public OuraNotificationPlutusRedeemer[] PlutusRedeemers { get; set; }

        [JsonProperty("plutus_data", NullValueHandling = NullValueHandling.Ignore)]
        public OuraNotificationPlutusDatum[] PlutusData { get; set; }

        [JsonProperty("withdrawals")] public object Withdrawals { get; set; }

        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public long? Size { get; set; }
    }

    public partial class OuraNotificationInput
    {
        [JsonProperty("tx_id", NullValueHandling = NullValueHandling.Ignore)]
        public string TxHash { get; set; }

        [JsonProperty("index", NullValueHandling = NullValueHandling.Ignore)]
        public long? Index { get; set; }
    }

    public partial class OuraNotificationOutput
    {
        [JsonProperty("address", NullValueHandling = NullValueHandling.Ignore)]
        public string Address { get; set; }

        [JsonProperty("amount", NullValueHandling = NullValueHandling.Ignore)]
        public long? Amount { get; set; }

        [JsonProperty("assets", NullValueHandling = NullValueHandling.Ignore)]
        public OuraNotificationAsset[] Assets { get; set; }

        [JsonProperty("datum_hash")] public string DatumHash { get; set; }
    }

    public partial class OuraNotificationAsset
    {
        [JsonProperty("policy", NullValueHandling = NullValueHandling.Ignore)]
        public string Policy { get; set; }

        [JsonProperty("asset", NullValueHandling = NullValueHandling.Ignore)]
        public string Asset { get; set; }

        [JsonProperty("asset_ascii", NullValueHandling = NullValueHandling.Ignore)]
        public string AssetAscii { get; set; }

        [JsonProperty("amount", NullValueHandling = NullValueHandling.Ignore)]
        public long? Amount { get; set; }
    }

    public partial class OuraNotificationPlutusDatum
    {
        [JsonProperty("datum_hash", NullValueHandling = NullValueHandling.Ignore)]
        public string DatumHash { get; set; }

        [JsonProperty("plutus_data", NullValueHandling = NullValueHandling.Ignore)]
        public OuraNotificationPlutusData PlutusData { get; set; }
    }

    public partial class OuraNotificationPlutusData
    {
        [JsonProperty("constructor", NullValueHandling = NullValueHandling.Ignore)]
        public long? Constructor { get; set; }

        [JsonProperty("fields", NullValueHandling = NullValueHandling.Ignore)]
        public OuraNotificationPlutusDataField[] Fields { get; set; }
    }

    public partial class OuraNotificationPlutusDataField
    {
        [JsonProperty("bytes", NullValueHandling = NullValueHandling.Ignore)]
        public string Bytes { get; set; }

        [JsonProperty("constructor", NullValueHandling = NullValueHandling.Ignore)]
        public long? Constructor { get; set; }
    }

    public partial class OuraNotificationPlutusRedeemer
    {
        [JsonProperty("purpose", NullValueHandling = NullValueHandling.Ignore)]
        public string Purpose { get; set; }

        [JsonProperty("ex_units_mem", NullValueHandling = NullValueHandling.Ignore)]
        public long? ExUnitsMem { get; set; }

        [JsonProperty("ex_units_steps", NullValueHandling = NullValueHandling.Ignore)]
        public long? ExUnitsSteps { get; set; }

        [JsonProperty("input_idx", NullValueHandling = NullValueHandling.Ignore)]
        public long? InputIdx { get; set; }

        [JsonProperty("plutus_data", NullValueHandling = NullValueHandling.Ignore)]
        public OuraNotificationPlutusData PlutusData { get; set; }
    }

    public partial class OuraNotificationPlutusWitness
    {
        [JsonProperty("script_hash", NullValueHandling = NullValueHandling.Ignore)]
        public string ScriptHash { get; set; }

        [JsonProperty("script_hex", NullValueHandling = NullValueHandling.Ignore)]
        public string ScriptHex { get; set; }
    }

    public partial class OuraNotificationVkeyWitness
    {
        [JsonProperty("vkey_hex", NullValueHandling = NullValueHandling.Ignore)]
        public string VkeyHex { get; set; }

        [JsonProperty("signature_hex", NullValueHandling = NullValueHandling.Ignore)]
        public string SignatureHex { get; set; }
    }
    */






