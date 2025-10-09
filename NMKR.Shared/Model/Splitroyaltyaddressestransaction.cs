using System;
using System.Collections.Generic;

namespace NMKR.Shared.Model;

public partial class Splitroyaltyaddressestransaction
{
    public int Id { get; set; }

    public int SplitroyaltyaddressesId { get; set; }

    public long Amount { get; set; }

    public DateTime Created { get; set; }

    public string Changeaddress { get; set; }

    public long Fee { get; set; }

    public string Txid { get; set; }

    public long Costs { get; set; }

    public string Costsaddress { get; set; }

    public virtual Splitroyaltyaddress Splitroyaltyaddresses { get; set; }

    public virtual ICollection<Splitroyaltyaddressestransactionssplit> Splitroyaltyaddressestransactionssplits { get; set; } = new List<Splitroyaltyaddressestransactionssplit>();
}
