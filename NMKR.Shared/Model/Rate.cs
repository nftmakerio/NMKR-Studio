using System;

namespace NMKR.Shared.Model;

public partial class Rate
{
    public int Id { get; set; }

    public DateTime Created { get; set; }

    public float Eurorate { get; set; }

    public float? Usdrate { get; set; }

    public float? Jpyrate { get; set; }

    public float? Btcrate { get; set; }

    public float? Soleurorate { get; set; }

    public float? Solusdrate { get; set; }

    public float? Soljpyrate { get; set; }

    public float? Solbtcrate { get; set; }
}
