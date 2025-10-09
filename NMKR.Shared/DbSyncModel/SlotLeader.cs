namespace NMKR.Shared.DbSyncModel;

public partial class SlotLeader
{
    public long Id { get; set; }

    public byte[] Hash { get; set; }

    public long? PoolHashId { get; set; }

    public string Description { get; set; }
}
