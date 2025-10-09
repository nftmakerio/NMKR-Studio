using System.Collections.Generic;

namespace NMKR.Shared.Model;

public partial class Validationaddress
{
    public int Id { get; set; }

    public string Address { get; set; }

    public string Privateskey { get; set; }

    public string Privatevkey { get; set; }

    public string Password { get; set; }

    public string State { get; set; }

    public virtual ICollection<Validationamount> Validationamounts { get; set; } = new List<Validationamount>();
}
