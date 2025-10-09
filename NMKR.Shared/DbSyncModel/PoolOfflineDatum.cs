namespace NMKR.Shared.DbSyncModel;

public partial class PoolOfflineDatum
{
    public long Id { get; set; }

    public long PoolId { get; set; }

    public string TickerName { get; set; }

    public byte[] Hash { get; set; }

    public string Json { get; set; }

    public byte[] Bytes { get; set; }

    public long PmrId { get; set; }
}
