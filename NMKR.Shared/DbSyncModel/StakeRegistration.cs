namespace NMKR.Shared.DbSyncModel;

public partial class StakeRegistration
{
    public long Id { get; set; }

    public long AddrId { get; set; }

    public int CertIndex { get; set; }

    public int EpochNo { get; set; }

    public long TxId { get; set; }
}
