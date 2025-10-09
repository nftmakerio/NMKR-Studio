using System;

namespace NMKR.Shared.Model;

public partial class Customerlogin
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public DateTime Created { get; set; }

    public string Ipaddress { get; set; }

    public virtual Customer Customer { get; set; }
}
