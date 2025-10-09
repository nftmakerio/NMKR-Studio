namespace QuickType
{
    using Newtonsoft.Json;

    public partial class BlockfrostTransaction
    {
        [JsonProperty("hash", NullValueHandling = NullValueHandling.Ignore)]
        public string Hash { get; set; }

        [JsonProperty("block", NullValueHandling = NullValueHandling.Ignore)]
        public string Block { get; set; }

        [JsonProperty("block_height", NullValueHandling = NullValueHandling.Ignore)]
        public long? BlockHeight { get; set; }

        [JsonProperty("block_time", NullValueHandling = NullValueHandling.Ignore)]
        public long? BlockTime { get; set; }

        [JsonProperty("slot", NullValueHandling = NullValueHandling.Ignore)]
        public long? Slot { get; set; }

        [JsonProperty("index", NullValueHandling = NullValueHandling.Ignore)]
        public long? Index { get; set; }


        [JsonProperty("fees", NullValueHandling = NullValueHandling.Ignore)]
        public long? Fees { get; set; }

        [JsonProperty("deposit", NullValueHandling = NullValueHandling.Ignore)]
        public long? Deposit { get; set; }

        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public long? Size { get; set; }

        [JsonProperty("invalid_before")]
        public object InvalidBefore { get; set; }

        [JsonProperty("invalid_hereafter", NullValueHandling = NullValueHandling.Ignore)]
        public long? InvalidHereafter { get; set; }

        [JsonProperty("utxo_count", NullValueHandling = NullValueHandling.Ignore)]
        public long? UtxoCount { get; set; }

        [JsonProperty("withdrawal_count", NullValueHandling = NullValueHandling.Ignore)]
        public long? WithdrawalCount { get; set; }

        [JsonProperty("mir_cert_count", NullValueHandling = NullValueHandling.Ignore)]
        public long? MirCertCount { get; set; }

        [JsonProperty("delegation_count", NullValueHandling = NullValueHandling.Ignore)]
        public long? DelegationCount { get; set; }

        [JsonProperty("stake_cert_count", NullValueHandling = NullValueHandling.Ignore)]
        public long? StakeCertCount { get; set; }

        [JsonProperty("pool_update_count", NullValueHandling = NullValueHandling.Ignore)]
        public long? PoolUpdateCount { get; set; }

        [JsonProperty("pool_retire_count", NullValueHandling = NullValueHandling.Ignore)]
        public long? PoolRetireCount { get; set; }

        [JsonProperty("asset_mint_or_burn_count", NullValueHandling = NullValueHandling.Ignore)]
        public long? AssetMintOrBurnCount { get; set; }

        [JsonProperty("redeemer_count", NullValueHandling = NullValueHandling.Ignore)]
        public long? RedeemerCount { get; set; }

        [JsonProperty("valid_contract", NullValueHandling = NullValueHandling.Ignore)]
        public bool? ValidContract { get; set; }
    }

   
}
