namespace NMKR.Shared.DbSyncModel;

public partial class Reward
{
    public long Id { get; set; }

    public long AddrId { get; set; }

    public decimal Amount { get; set; }

    public long EarnedEpoch { get; set; }

    public long SpendableEpoch { get; set; }

    public long? PoolId { get; set; }
}
