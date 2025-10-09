namespace NMKR.Shared.DbSyncModel;

public partial class MaTxMint
{
    public long Id { get; set; }

    public decimal Quantity { get; set; }

    public long TxId { get; set; }

    public long Ident { get; set; }
}
