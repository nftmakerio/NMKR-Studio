namespace NMKR.Shared.Model;

public partial class Splitroyaltyaddressestransactionssplit
{
    public int Id { get; set; }

    public int SplitroyaltyaddressestransactionsId { get; set; }

    public string Splitaddress { get; set; }

    public long Amount { get; set; }

    public int Percentage { get; set; }

    public virtual Splitroyaltyaddressestransaction Splitroyaltyaddressestransactions { get; set; }
}
