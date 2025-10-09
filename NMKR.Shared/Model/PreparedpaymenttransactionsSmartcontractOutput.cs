using System.Collections.Generic;

namespace NMKR.Shared.Model;

public partial class PreparedpaymenttransactionsSmartcontractOutput
{
    public int Id { get; set; }

    public int PreparedpaymenttransactionsId { get; set; }

    public string Address { get; set; }

    public long Lovelace { get; set; }

    public string Pkh { get; set; }

    public string Type { get; set; }

    public virtual Preparedpaymenttransaction Preparedpaymenttransactions { get; set; }

    public virtual ICollection<PreparedpaymenttransactionsSmartcontractOutputsAsset> PreparedpaymenttransactionsSmartcontractOutputsAssets { get; set; } = new List<PreparedpaymenttransactionsSmartcontractOutputsAsset>();
}
