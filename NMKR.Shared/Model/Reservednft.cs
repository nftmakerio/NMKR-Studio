using System;

namespace NMKR.Shared.Model;

public partial class Reservednft
{
    public int Id { get; set; }

    public int NftId { get; set; }

    public int Reservedcount { get; set; }

    public DateTime Reserveduntil { get; set; }

    public string Reservedforaddress { get; set; }

    public virtual Nft Nft { get; set; }
}
