using System;
using System.Collections.Generic;

namespace NMKR.Shared.Model;

public partial class Countedwhitelist
{
    public int Id { get; set; }

    public int SaleconditionsId { get; set; }

    public string Address { get; set; }

    public string Stakeaddress { get; set; }

    public long Maxcount { get; set; }

    public DateTime Created { get; set; }

    public virtual ICollection<Countedwhitelistusedaddress> Countedwhitelistusedaddresses { get; set; } = new List<Countedwhitelistusedaddress>();

    public virtual Nftprojectsalecondition Saleconditions { get; set; }
}
