namespace NMKR.Shared.Model;

public partial class PreparedpaymenttransactionsCustomproperty
{
    public int Id { get; set; }

    public int PreparedpaymenttransactionsId { get; set; }

    public string Key { get; set; }

    public string Value { get; set; }

    public virtual Preparedpaymenttransaction Preparedpaymenttransactions { get; set; }
}
