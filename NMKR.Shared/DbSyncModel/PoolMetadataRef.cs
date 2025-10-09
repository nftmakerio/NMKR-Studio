namespace NMKR.Shared.DbSyncModel;

public partial class PoolMetadataRef
{
    public long Id { get; set; }

    public long PoolId { get; set; }

    public string Url { get; set; }

    public byte[] Hash { get; set; }

    public long RegisteredTxId { get; set; }
}
