using System;

namespace NMKR.Shared.Model;

public partial class Validationamount
{
    public int Id { get; set; }

    public int ValidationaddressId { get; set; }

    public long Lovelace { get; set; }

    public string State { get; set; }

    public string Senderaddress { get; set; }

    public DateTime Validuntil { get; set; }

    public string Uid { get; set; }

    public string Optionalvalidationname { get; set; }

    public virtual Validationaddress Validationaddress { get; set; }
}
