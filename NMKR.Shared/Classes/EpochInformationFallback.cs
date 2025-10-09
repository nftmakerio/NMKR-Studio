using Newtonsoft.Json;

namespace NMKR.Shared.Classes
{
    public class EpochInformationFallback
    {
        [JsonProperty("epoch_no", NullValueHandling = NullValueHandling.Ignore)]
        public long? EpochNo { get; set; }

        [JsonProperty("min_fee_a")]
        public long? MinFeeA { get; set; }

        [JsonProperty("min_fee_b")]
        public long? MinFeeB { get; set; }

        [JsonProperty("max_block_size")]
        public long? MaxBlockSize { get; set; }

        [JsonProperty("max_tx_size")]
        public long? MaxTxSize { get; set; }

        [JsonProperty("max_bh_size")]
        public long? MaxBhSize { get; set; }

        [JsonProperty("key_deposit")]
        public long? KeyDeposit { get; set; }

        [JsonProperty("pool_deposit")]
        public long? PoolDeposit { get; set; }

        [JsonProperty("max_epoch")]
        public long? MaxEpoch { get; set; }

        [JsonProperty("optimal_pool_count")]
        public long? OptimalPoolCount { get; set; }

        [JsonProperty("influence")]
        public double? Influence { get; set; }

        [JsonProperty("monetary_expand_rate")]
        public double? MonetaryExpandRate { get; set; }

        [JsonProperty("treasury_growth_rate")]
        public double? TreasuryGrowthRate { get; set; }

        [JsonProperty("decentralisation")]
        public double? Decentralisation { get; set; }

        [JsonProperty("extra_entropy")]
        public object ExtraEntropy { get; set; }

        [JsonProperty("protocol_major")]
        public long? ProtocolMajor { get; set; }

        [JsonProperty("protocol_minor")]
        public long? ProtocolMinor { get; set; }

        [JsonProperty("min_utxo_value")]
        public long? MinUtxoValue { get; set; }

        [JsonProperty("min_pool_cost")]
        public long? MinPoolCost { get; set; }

        [JsonProperty("nonce")]
        public string Nonce { get; set; }

        [JsonProperty("block_hash", NullValueHandling = NullValueHandling.Ignore)]
        public string BlockHash { get; set; }

        [JsonProperty("cost_models")]
        public string CostModels { get; set; }

        [JsonProperty("price_mem")]
        public double? PriceMem { get; set; }

        [JsonProperty("price_step")]
        public double? PriceStep { get; set; }

        [JsonProperty("max_tx_ex_mem")]
        public long? MaxTxExMem { get; set; }

        [JsonProperty("max_tx_ex_steps")]
        public long? MaxTxExSteps { get; set; }

        [JsonProperty("max_block_ex_mem")]
        public long? MaxBlockExMem { get; set; }

        [JsonProperty("max_block_ex_steps")]
        public long? MaxBlockExSteps { get; set; }

        [JsonProperty("max_val_size")]
        public long? MaxValSize { get; set; }

        [JsonProperty("collateral_percent")]
        public long? CollateralPercent { get; set; }

        [JsonProperty("max_collateral_inputs")]
        public long? MaxCollateralInputs { get; set; }

        [JsonProperty("coins_per_utxo_size")]
        public long? CoinsPerUtxoSize { get; set; }

        [JsonProperty("coins_per_utxo_word")]
        public long? CoinsPerUtxoWord { get; set; }
    }
}
