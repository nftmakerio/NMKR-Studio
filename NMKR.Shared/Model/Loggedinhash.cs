using System;

namespace NMKR.Shared.Model;

public partial class Loggedinhash
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public string Hash { get; set; }

    public string Ipaddress { get; set; }

    public DateTime Validuntil { get; set; }

    public DateTime? Lastlifesign { get; set; }

    public virtual Customer Customer { get; set; }
}
