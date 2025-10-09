using System;

namespace NMKR.Shared.Model;

public partial class Custodialwallet
{
    public int Id { get; set; }

    public string Uid { get; set; }

    public int CustomerId { get; set; }

    public string Walletname { get; set; }

    public string Address { get; set; }

    public string Skey { get; set; }

    public string Vkey { get; set; }

    public string Seedphrase { get; set; }

    public string Salt { get; set; }

    public string Wallettype { get; set; }

    public DateTime Created { get; set; }

    public DateTime? Lastcheckforutxo { get; set; }

    public string State { get; set; }

    public string Pincode { get; set; }

    public virtual Customer Customer { get; set; }
}
