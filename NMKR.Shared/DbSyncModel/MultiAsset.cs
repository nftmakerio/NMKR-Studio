namespace NMKR.Shared.DbSyncModel;

public partial class MultiAsset
{
    public long Id { get; set; }

    public byte[] Policy { get; set; }

    public byte[] Name { get; set; }

    public string Fingerprint { get; set; }
}
