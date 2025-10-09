using System.Collections.Generic;

namespace NMKR.Shared.Model;

public partial class Referer
{
    public int Id { get; set; }

    public string Referertoken { get; set; }

    public int ReferercustomerId { get; set; }

    public string State { get; set; }

    public float Commisionpercent { get; set; }

    public int? PayoutwalletId { get; set; }

    public virtual ICollection<Nftaddress> Nftaddresses { get; set; } = new List<Nftaddress>();

    public virtual Customerwallet Payoutwallet { get; set; }

    public virtual Customer Referercustomer { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
