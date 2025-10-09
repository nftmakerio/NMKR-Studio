namespace NMKR.Shared.DbSyncModel;

public partial class TxIn
{
    public long Id { get; set; }

    public long TxInId { get; set; }

    public long TxOutId { get; set; }

    public short TxOutIndex { get; set; }

    public long? RedeemerId { get; set; }
}
