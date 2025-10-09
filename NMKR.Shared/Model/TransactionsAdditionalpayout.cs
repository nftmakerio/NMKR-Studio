namespace NMKR.Shared.Model;

public partial class TransactionsAdditionalpayout
{
    public int Id { get; set; }

    public int TransactionId { get; set; }

    public string Payoutaddress { get; set; }

    public long Lovelace { get; set; }

    public virtual Transaction Transaction { get; set; }
}
