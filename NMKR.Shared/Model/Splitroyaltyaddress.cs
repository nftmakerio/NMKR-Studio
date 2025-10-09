using System;
using System.Collections.Generic;

namespace NMKR.Shared.Model;

public partial class Splitroyaltyaddress
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public string Address { get; set; }

    public string Skey { get; set; }

    public string Vkey { get; set; }

    public string Salt { get; set; }

    public DateTime Created { get; set; }

    public string State { get; set; }

    public long Minthreshold { get; set; }

    public DateTime? Lastcheck { get; set; }

    public string Comment { get; set; }

    public long Lovelace { get; set; }

    public virtual Customer Customer { get; set; }

    public virtual ICollection<Splitroyaltyaddressessplit> Splitroyaltyaddressessplits { get; set; } = new List<Splitroyaltyaddressessplit>();

    public virtual ICollection<Splitroyaltyaddressestransaction> Splitroyaltyaddressestransactions { get; set; } = new List<Splitroyaltyaddressestransaction>();
}
