namespace NMKR.Shared.Model;

public partial class Invoicedetail
{
    public int Id { get; set; }

    public int InvoiceId { get; set; }

    public string Description { get; set; }

    public int Count { get; set; }

    public double Pricesingleada { get; set; }

    public double Pricesingleeur { get; set; }

    public double Pricetotalada { get; set; }

    public double Pricetotaleur { get; set; }

    public double Averageadarate { get; set; }

    public double Mintcostsada { get; set; }

    public double Mintcostseur { get; set; }

    public virtual Invoice Invoice { get; set; }
}
