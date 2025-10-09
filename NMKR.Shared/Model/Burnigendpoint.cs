using System;

namespace NMKR.Shared.Model;

public partial class Burnigendpoint
{
    public int Id { get; set; }

    public int NftprojectId { get; set; }

    public string Address { get; set; }

    public string Privateskey { get; set; }

    public string Privatevkey { get; set; }

    public long Lovelace { get; set; }

    public string Salt { get; set; }

    public DateTime Validuntil { get; set; }

    public string State { get; set; }

    public bool Fixnfts { get; set; }

    public bool Shownotification { get; set; }

    public string Blockchain { get; set; }

    public virtual Nftproject Nftproject { get; set; }
}
