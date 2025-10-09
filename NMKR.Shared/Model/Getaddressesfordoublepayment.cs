using System;

namespace NMKR.Shared.Model;

public partial class Getaddressesfordoublepayment
{
    public int Id { get; set; }

    public string Address { get; set; }

    public DateTime? Lastcheckforutxo { get; set; }

    public DateTime? Paydate { get; set; }

    public string State { get; set; }

    public DateTime Created { get; set; }

    public bool Checkfordoublepayment { get; set; }

    public string Coin { get; set; }
}
