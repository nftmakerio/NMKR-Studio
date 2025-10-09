namespace NMKR.Shared.DbSyncModel;

public partial class EpochStake
{
    public long Id { get; set; }

    public long AddrId { get; set; }

    public long PoolId { get; set; }

    public decimal Amount { get; set; }

    public int EpochNo { get; set; }
}
