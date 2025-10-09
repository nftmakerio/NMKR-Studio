using System;

namespace NMKR.Shared.Model;

public partial class PreparedpaymenttransactionsSmartcontractsjson
{
    public int Id { get; set; }

    public int PreparedpaymenttransactionsId { get; set; }

    public string Templatetype { get; set; }

    public string Json { get; set; }

    public string Hash { get; set; }

    public string Address { get; set; }

    public string Rawtx { get; set; }

    public DateTime? Created { get; set; }

    public DateTime? Signed { get; set; }

    public DateTime? Submitted { get; set; }

    public string Signedcbr { get; set; }

    public string Txid { get; set; }

    public string Redeemer { get; set; }

    public long? Fee { get; set; }

    public long? Bidamount { get; set; }

    public string Logfile { get; set; }

    public string Signinguid { get; set; }

    public bool Signedandsubmitted { get; set; }

    public bool Confirmed { get; set; }

    public DateTime? Checkforconfirmdate { get; set; }

    public virtual Preparedpaymenttransaction Preparedpaymenttransactions { get; set; }
}
