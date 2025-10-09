using System;
using System.Collections.Generic;

namespace NMKR.Shared.Model;

public partial class Premintednftsaddress
{
    public int Id { get; set; }

    public int? NftprojectId { get; set; }

    public string Address { get; set; }

    public string Privateskey { get; set; }

    public string Privatevkey { get; set; }

    public string Salt { get; set; }

    public long Lovelace { get; set; }

    public DateTime Lastcheckforutxo { get; set; }

    public string State { get; set; }

    public DateTime Created { get; set; }

    public DateTime? Expires { get; set; }

    public virtual Nftproject Nftproject { get; set; }

    public virtual ICollection<Nft> Nfts { get; set; } = new List<Nft>();
}
