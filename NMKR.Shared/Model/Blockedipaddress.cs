using System;

namespace NMKR.Shared.Model;

public partial class Blockedipaddress
{
    public int Id { get; set; }

    public string Ipaddress { get; set; }

    public DateTime Blockeduntil { get; set; }

    public int Blockcounter { get; set; }
}
