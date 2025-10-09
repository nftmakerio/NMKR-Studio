using System;

namespace NMKR.Shared.Model;

public partial class Legacyauctionshistory
{
    public int Id { get; set; }

    public int LegacyauctionId { get; set; }

    public string Txhash { get; set; }

    public string Senderaddress { get; set; }

    public long Bidamount { get; set; }

    public DateTime Created { get; set; }

    public string State { get; set; }

    public string Returntxhash { get; set; }

    public virtual Legacyauction Legacyauction { get; set; }
}
