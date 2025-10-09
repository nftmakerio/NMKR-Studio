namespace NMKR.Shared.DbSyncModel;

public partial class Delegation
{
    public long Id { get; set; }

    public long AddrId { get; set; }

    public int CertIndex { get; set; }

    public long PoolHashId { get; set; }

    public long ActiveEpochNo { get; set; }

    public long TxId { get; set; }

    public long SlotNo { get; set; }

    public long? RedeemerId { get; set; }
}
