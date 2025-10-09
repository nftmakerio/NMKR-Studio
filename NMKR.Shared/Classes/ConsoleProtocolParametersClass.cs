using System.Collections.Generic;
using Newtonsoft.Json;

namespace NMKR.Shared.Classes
{
    public partial class ConsoleProtocolParametersClass
    {
        [JsonProperty("collateralPercentage", NullValueHandling = NullValueHandling.Ignore)]
        public long? CollateralPercentage { get; set; }

        [JsonProperty("costModels", NullValueHandling = NullValueHandling.Ignore)]
        public CostModels CostModels { get; set; }

        [JsonProperty("decentralization")]
        public object Decentralization { get; set; }

        [JsonProperty("executionUnitPrices", NullValueHandling = NullValueHandling.Ignore)]
        public ExecutionUnitPrices ExecutionUnitPrices { get; set; }

        [JsonProperty("extraPraosEntropy")]
        public object ExtraPraosEntropy { get; set; }

        [JsonProperty("maxBlockBodySize", NullValueHandling = NullValueHandling.Ignore)]
        public long? MaxBlockBodySize { get; set; }

        [JsonProperty("maxBlockExecutionUnits", NullValueHandling = NullValueHandling.Ignore)]
        public MaxExecutionUnits MaxBlockExecutionUnits { get; set; }

        [JsonProperty("maxBlockHeaderSize", NullValueHandling = NullValueHandling.Ignore)]
        public long? MaxBlockHeaderSize { get; set; }

        [JsonProperty("maxCollateralInputs", NullValueHandling = NullValueHandling.Ignore)]
        public long? MaxCollateralInputs { get; set; }

        [JsonProperty("maxTxExecutionUnits", NullValueHandling = NullValueHandling.Ignore)]
        public MaxExecutionUnits MaxTxExecutionUnits { get; set; }

        [JsonProperty("maxTxSize", NullValueHandling = NullValueHandling.Ignore)]
        public long? MaxTxSize { get; set; }

        [JsonProperty("maxValueSize", NullValueHandling = NullValueHandling.Ignore)]
        public long? MaxValueSize { get; set; }

        [JsonProperty("minPoolCost", NullValueHandling = NullValueHandling.Ignore)]
        public long? MinPoolCost { get; set; }

        [JsonProperty("minUTxOValue")]
        public object MinUTxOValue { get; set; }

        [JsonProperty("monetaryExpansion", NullValueHandling = NullValueHandling.Ignore)]
        public double? MonetaryExpansion { get; set; }

        [JsonProperty("poolPledgeInfluence", NullValueHandling = NullValueHandling.Ignore)]
        public double? PoolPledgeInfluence { get; set; }

        [JsonProperty("poolRetireMaxEpoch", NullValueHandling = NullValueHandling.Ignore)]
        public long? PoolRetireMaxEpoch { get; set; }

        [JsonProperty("protocolVersion", NullValueHandling = NullValueHandling.Ignore)]
        public ProtocolVersion ProtocolVersion { get; set; }

        [JsonProperty("stakeAddressDeposit", NullValueHandling = NullValueHandling.Ignore)]
        public long? StakeAddressDeposit { get; set; }

        [JsonProperty("stakePoolDeposit", NullValueHandling = NullValueHandling.Ignore)]
        public long? StakePoolDeposit { get; set; }

        [JsonProperty("stakePoolTargetNum", NullValueHandling = NullValueHandling.Ignore)]
        public long? StakePoolTargetNum { get; set; }

        [JsonProperty("treasuryCut", NullValueHandling = NullValueHandling.Ignore)]
        public double? TreasuryCut { get; set; }

        [JsonProperty("txFeeFixed", NullValueHandling = NullValueHandling.Ignore)]
        public long? TxFeeFixed { get; set; }

        [JsonProperty("txFeePerByte", NullValueHandling = NullValueHandling.Ignore)]
        public long? TxFeePerByte { get; set; }

        [JsonProperty("utxoCostPerByte", NullValueHandling = NullValueHandling.Ignore)]
        public long? UtxoCostPerByte { get; set; }

        [JsonProperty("utxoCostPerWord")]
        public long? UtxoCostPerWord { get; set; }
    }

    public partial class CostModels
    {
        [JsonProperty("PlutusScriptV1", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, long> PlutusScriptV1 { get; set; }

        [JsonProperty("PlutusScriptV2", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, long> PlutusScriptV2 { get; set; }
    }

    public partial class ExecutionUnitPrices
    {
        [JsonProperty("priceMemory", NullValueHandling = NullValueHandling.Ignore)]
        public double? PriceMemory { get; set; }

        [JsonProperty("priceSteps", NullValueHandling = NullValueHandling.Ignore)]
        public double? PriceSteps { get; set; }
    }

    public partial class MaxExecutionUnits
    {
        [JsonProperty("memory", NullValueHandling = NullValueHandling.Ignore)]
        public long? Memory { get; set; }

        [JsonProperty("steps", NullValueHandling = NullValueHandling.Ignore)]
        public long? Steps { get; set; }
    }

    public partial class ProtocolVersion
    {
        [JsonProperty("major", NullValueHandling = NullValueHandling.Ignore)]
        public long? Major { get; set; }

        [JsonProperty("minor", NullValueHandling = NullValueHandling.Ignore)]
        public long? Minor { get; set; }
    }
}
