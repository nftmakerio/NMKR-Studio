using System;

namespace NMKR.Shared.Model;

public partial class Projectaddressestxhash
{
    public int Id { get; set; }

    public string Txhash { get; set; }

    public DateTime Created { get; set; }

    public long Lovelace { get; set; }

    public string Tokens { get; set; }

    public string Address { get; set; }
}
