namespace NMKR.Shared.DbSyncModel;

public partial class MaTxOut
{
    public long Id { get; set; }

    public decimal Quantity { get; set; }

    public long TxOutId { get; set; }

    public long Ident { get; set; }
}
