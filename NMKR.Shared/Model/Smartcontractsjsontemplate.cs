namespace NMKR.Shared.Model;

public partial class Smartcontractsjsontemplate
{
    public int Id { get; set; }

    public int SmartcontractsId { get; set; }

    public string Templatetype { get; set; }

    public string Jsontemplate { get; set; }

    public string Redeemertemplate { get; set; }

    public string Recipienttemplate { get; set; }

    public virtual Smartcontract Smartcontracts { get; set; }
}
