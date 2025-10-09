namespace NMKR.Shared.DbSyncModel;

public partial class EpochParam
{
    public long Id { get; set; }

    public int EpochNo { get; set; }

    public int MinFeeA { get; set; }

    public int MinFeeB { get; set; }

    public int MaxBlockSize { get; set; }

    public int MaxTxSize { get; set; }

    public int MaxBhSize { get; set; }

    public decimal KeyDeposit { get; set; }

    public decimal PoolDeposit { get; set; }

    public int MaxEpoch { get; set; }

    public int OptimalPoolCount { get; set; }

    public double Influence { get; set; }

    public double MonetaryExpandRate { get; set; }

    public double TreasuryGrowthRate { get; set; }

    public double Decentralisation { get; set; }

    public int ProtocolMajor { get; set; }

    public int ProtocolMinor { get; set; }

    public decimal MinUtxoValue { get; set; }

    public decimal MinPoolCost { get; set; }

    public byte[] Nonce { get; set; }

    public long? CostModelId { get; set; }

    public double? PriceMem { get; set; }

    public double? PriceStep { get; set; }

    public decimal? MaxTxExMem { get; set; }

    public decimal? MaxTxExSteps { get; set; }

    public decimal? MaxBlockExMem { get; set; }

    public decimal? MaxBlockExSteps { get; set; }

    public decimal? MaxValSize { get; set; }

    public int? CollateralPercent { get; set; }

    public int? MaxCollateralInputs { get; set; }

    public long BlockId { get; set; }

    public byte[] ExtraEntropy { get; set; }

    public decimal? CoinsPerUtxoSize { get; set; }
}
