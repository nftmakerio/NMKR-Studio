namespace NMKR.Shared.DbSyncModel;

public partial class ReservedPoolTicker
{
    public long Id { get; set; }

    public string Name { get; set; }

    public byte[] PoolHash { get; set; }
}
