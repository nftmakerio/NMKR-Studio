namespace NMKR.Shared.DbSyncModel;

public partial class Treasury
{
    public long Id { get; set; }

    public long AddrId { get; set; }

    public int CertIndex { get; set; }

    public decimal Amount { get; set; }

    public long TxId { get; set; }
}
