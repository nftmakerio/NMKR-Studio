using System;

namespace NMKR.Shared.Model;

public partial class Usedaddressesonsalecondition
{
    public int Id { get; set; }

    public int SalecondtionsId { get; set; }

    public string Address { get; set; }

    public DateTime Created { get; set; }

    public virtual Nftprojectsalecondition Salecondtions { get; set; }
}
