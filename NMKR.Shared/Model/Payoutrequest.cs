using System;

namespace NMKR.Shared.Model;

public partial class Payoutrequest
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public int WalletId { get; set; }

    public long Ada { get; set; }

    public DateTime Created { get; set; }

    public string Confirmationcode { get; set; }

    public DateTime Confirmationexpire { get; set; }

    public string Confirmationipaddress { get; set; }

    public string State { get; set; }

    public DateTime? Executiontime { get; set; }

    public string Payoutinitiator { get; set; }

    public DateTime? Confirmationtime { get; set; }

    public string Transactionid { get; set; }

    public string Logfile { get; set; }

    public virtual Customer Customer { get; set; }

    public virtual Customerwallet Wallet { get; set; }
}
