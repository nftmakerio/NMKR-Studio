using System;
using System.Collections.Generic;

namespace NMKR.Shared.Model;

public partial class Lockedasset
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public DateTime Created { get; set; }

    public string Changeaddress { get; set; }

    public string Lockwalletpkh { get; set; }

    public string Lockassetaddress { get; set; }

    public long Lovelace { get; set; }

    public DateTime Lockeduntil { get; set; }

    public string Policyscript { get; set; }

    public string Locktxid { get; set; }

    public DateTime? Unlocked { get; set; }

    public string Unlocktxid { get; set; }

    public string Walletname { get; set; }

    public bool? Confirmedlock { get; set; }

    public bool? Confirmedunlock { get; set; }

    public long Lockslot { get; set; }

    public string Description { get; set; }

    public string State { get; set; }

    public virtual Customer Customer { get; set; }

    public virtual ICollection<Lockedassetstoken> Lockedassetstokens { get; set; } = new List<Lockedassetstoken>();
}
