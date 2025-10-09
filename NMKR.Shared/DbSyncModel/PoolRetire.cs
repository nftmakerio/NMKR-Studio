namespace NMKR.Shared.DbSyncModel;

public partial class PoolRetire
{
    public long Id { get; set; }

    public long HashId { get; set; }

    public int CertIndex { get; set; }

    public long AnnouncedTxId { get; set; }

    public int RetiringEpoch { get; set; }
}
