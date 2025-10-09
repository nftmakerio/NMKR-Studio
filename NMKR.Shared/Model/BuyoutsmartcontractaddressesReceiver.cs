namespace NMKR.Shared.Model;

public partial class BuyoutsmartcontractaddressesReceiver
{
    public int Id { get; set; }

    public int BuyoutsmartcontractaddressesId { get; set; }

    public string Receiveraddress { get; set; }

    public long Lovelace { get; set; }

    public string Pkh { get; set; }

    public virtual Buyoutsmartcontractaddress Buyoutsmartcontractaddresses { get; set; }
}
