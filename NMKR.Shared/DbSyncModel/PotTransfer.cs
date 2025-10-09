namespace NMKR.Shared.DbSyncModel;

public partial class PotTransfer
{
    public long Id { get; set; }

    public int CertIndex { get; set; }

    public decimal Treasury { get; set; }

    public decimal Reserves { get; set; }

    public long TxId { get; set; }
}
