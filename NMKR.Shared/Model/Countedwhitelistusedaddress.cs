using System;

namespace NMKR.Shared.Model;

public partial class Countedwhitelistusedaddress
{
    public int Id { get; set; }

    public int CountedwhitelistId { get; set; }

    public string Usedaddress { get; set; }

    public string Originatoraddress { get; set; }

    public string Transactionid { get; set; }

    public DateTime Created { get; set; }

    public long Countnft { get; set; }

    public virtual Countedwhitelist Countedwhitelist { get; set; }
}
