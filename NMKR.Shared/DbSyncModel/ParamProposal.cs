namespace NMKR.Shared.DbSyncModel;

public partial class ParamProposal
{
    public long Id { get; set; }

    public int EpochNo { get; set; }

    public byte[] Key { get; set; }

    public decimal? MinFeeA { get; set; }

    public decimal? MinFeeB { get; set; }

    public decimal? MaxBlockSize { get; set; }

    public decimal? MaxTxSize { get; set; }

    public decimal? MaxBhSize { get; set; }

    public decimal? KeyDeposit { get; set; }

    public decimal? PoolDeposit { get; set; }

    public decimal? MaxEpoch { get; set; }

    public decimal? OptimalPoolCount { get; set; }

    public double? Influence { get; set; }

    public double? MonetaryExpandRate { get; set; }

    public double? TreasuryGrowthRate { get; set; }

    public double? Decentralisation { get; set; }

    public byte[] Entropy { get; set; }

    public int? ProtocolMajor { get; set; }

    public int? ProtocolMinor { get; set; }

    public decimal? MinUtxoValue { get; set; }

    public decimal? MinPoolCost { get; set; }

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

    public long RegisteredTxId { get; set; }

    public decimal? CoinsPerUtxoSize { get; set; }
}
