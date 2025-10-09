using System;
using System.Collections.Generic;

namespace NMKR.Shared.Model;

public partial class Invoice
{
    public int Id { get; set; }

    public int Invoiceno { get; set; }

    public int CustomerId { get; set; }

    public string Company { get; set; }

    public string Firstname { get; set; }

    public string Lastname { get; set; }

    public string Street { get; set; }

    public string Zip { get; set; }

    public string City { get; set; }

    public int CountryId { get; set; }

    public string Ustid { get; set; }

    public DateTime Invoicedate { get; set; }

    public double Adarate { get; set; }

    public double Netada { get; set; }

    public double Neteur { get; set; }

    public double Usteur { get; set; }

    public double Grosseur { get; set; }

    public double Discounteur { get; set; }

    public double Discountpercent { get; set; }

    public string Billingperiod { get; set; }

    public double Taxrate { get; set; }

    public virtual Country Country { get; set; }

    public virtual Customer Customer { get; set; }

    public virtual ICollection<Invoicedetail> Invoicedetails { get; set; } = new List<Invoicedetail>();
}
