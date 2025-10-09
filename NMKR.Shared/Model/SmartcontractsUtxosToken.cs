namespace NMKR.Shared.Model;

public partial class SmartcontractsUtxosToken
{
    public int Id { get; set; }

    public int SmartcontractsUtxosId { get; set; }

    public string Policyid { get; set; }

    public string Tokennameinhex { get; set; }

    public long Count { get; set; }

    public virtual SmartcontractsUtxo SmartcontractsUtxos { get; set; }
}
