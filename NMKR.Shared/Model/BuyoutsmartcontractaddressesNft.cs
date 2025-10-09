namespace NMKR.Shared.Model;

public partial class BuyoutsmartcontractaddressesNft
{
    public int Id { get; set; }

    public int BuyoutsmartcontractaddressesIid { get; set; }

    public string Tokennameinhex { get; set; }

    public string Policyid { get; set; }

    public long Tokencount { get; set; }

    public virtual Buyoutsmartcontractaddress BuyoutsmartcontractaddressesI { get; set; }
}
