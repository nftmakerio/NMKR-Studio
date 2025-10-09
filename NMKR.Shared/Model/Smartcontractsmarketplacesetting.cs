using System.Collections.Generic;

namespace NMKR.Shared.Model;

public partial class Smartcontractsmarketplacesetting
{
    public int Id { get; set; }

    public string Skey { get; set; }

    public string Vkey { get; set; }

    public string Salt { get; set; }

    public string Address { get; set; }

    public string Pkh { get; set; }

    public string Description { get; set; }

    public float Percentage { get; set; }

    public string Fakesignaddress { get; set; }

    public string Fakesignvkey { get; set; }

    public string Fakesignskey { get; set; }

    public string Collateral { get; set; }

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

    public virtual ICollection<Nftproject> Nftprojects { get; set; } = new List<Nftproject>();
}
