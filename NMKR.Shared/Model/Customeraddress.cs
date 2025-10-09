using System;

namespace NMKR.Shared.Model;

public partial class Customeraddress
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public string Address { get; set; }

    public string Seedphrase { get; set; }

    public string Vkey { get; set; }

    public string Skey { get; set; }

    public string Blockchain { get; set; }

    public string State { get; set; }

    public DateTime? Lastchecked { get; set; }

    public string Salt { get; set; }

    public virtual Customer Customer { get; set; }
}
