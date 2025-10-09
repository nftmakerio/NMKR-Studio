using System;

namespace NMKR.Shared.Model;

public partial class Newrate
{
    public int Id { get; set; }

    public string Coin { get; set; }

    public string Currency { get; set; }

    public DateTime Effectivedate { get; set; }

    public double? Price { get; set; }
}
