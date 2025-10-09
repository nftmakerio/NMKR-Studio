using System.Collections.Generic;

namespace NMKR.Shared.Model;

public partial class Country
{
    public int Id { get; set; }

    public string Iso { get; set; }

    public string Name { get; set; }

    public string Nicename { get; set; }

    public string Iso3 { get; set; }

    public short? Numcode { get; set; }

    public int Phonecode { get; set; }

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
