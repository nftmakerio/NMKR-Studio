namespace NMKR.Shared.Model;

public partial class PreparedpaymenttransactionsSmartcontractOutputsAsset
{
    public int Id { get; set; }

    public int PreparedpaymenttransactionsSmartcontractOutputsId { get; set; }

    public string Tokennameinhex { get; set; }

    public string Policyid { get; set; }

    public long Amount { get; set; }

    public virtual PreparedpaymenttransactionsSmartcontractOutput PreparedpaymenttransactionsSmartcontractOutputs { get; set; }
}
