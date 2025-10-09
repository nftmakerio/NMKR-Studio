namespace NMKR.Shared.DbSyncModel;

public partial class PoolHash
{
    public long Id { get; set; }

    public byte[] HashRaw { get; set; }

    public string View { get; set; }
}
