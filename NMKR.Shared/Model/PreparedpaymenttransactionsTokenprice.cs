namespace NMKR.Shared.Model;

public partial class PreparedpaymenttransactionsTokenprice
{
    public int Id { get; set; }

    public int PreparedpaymenttransactionId { get; set; }

    public string Policyid { get; set; }

    public string Assetname { get; set; }

    public long Tokencount { get; set; }

    public long Totalcount { get; set; }

    public long Multiplier { get; set; }

    public long Decimals { get; set; }

    public string Assetnamehex { get; set; }

    public virtual Preparedpaymenttransaction Preparedpaymenttransaction { get; set; }
}
