using System;

namespace NMKR.Shared.Model;

public partial class Manualrefund
{
    public int Id { get; set; }

    public string Txin { get; set; }

    public long Lovelace { get; set; }

    public string Senderaddress { get; set; }

    public bool Sendout { get; set; }

    public DateTime Txindate { get; set; }

    public string Transactionid { get; set; }

    public string Log { get; set; }
}
