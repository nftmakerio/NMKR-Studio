using System;

namespace NMKR.Shared.Model;

public partial class Splitroyaltyaddressessplit
{
    public int Id { get; set; }

    public int SplitroyaltyaddressesId { get; set; }

    public string Address { get; set; }

    /// <summary>
    /// percentage * 100 / so 10 percent = 1000
    /// </summary>
    public int Percentage { get; set; }

    public bool? IsMainReceiver { get; set; }

    public string State { get; set; }

    public DateTime? Activefrom { get; set; }

    public DateTime? Activeto { get; set; }

    public DateTime Created { get; set; }

    public virtual Splitroyaltyaddress Splitroyaltyaddresses { get; set; }
}
