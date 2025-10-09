namespace NMKR.Shared.DbSyncModel;

public partial class Withdrawal
{
    public long Id { get; set; }

    public long AddrId { get; set; }

    public decimal Amount { get; set; }

    public long? RedeemerId { get; set; }

    public long TxId { get; set; }
}
