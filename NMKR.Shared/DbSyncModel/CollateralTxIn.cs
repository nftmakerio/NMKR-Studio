namespace NMKR.Shared.DbSyncModel;

public partial class CollateralTxIn
{
    public long Id { get; set; }

    public long TxInId { get; set; }

    public long TxOutId { get; set; }

    public short TxOutIndex { get; set; }
}
