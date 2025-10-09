namespace NMKR.Shared.Model;

public partial class Nftprojectsadditionalpayout
{
    public int Id { get; set; }

    public int NftprojectId { get; set; }

    public int WalletId { get; set; }

    public double? Valuepercent { get; set; }

    public long? Valuetotal { get; set; }

    public string Custompropertycondition { get; set; }

    public string Coin { get; set; }

    public virtual Nftproject Nftproject { get; set; }

    public virtual Customerwallet Wallet { get; set; }
}
