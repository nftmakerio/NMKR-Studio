namespace NMKR.Shared.Model;

public partial class Getaccesstokensuser
{
    public int Id { get; set; }

    public string Friendlyname { get; set; }

    public string Secret { get; set; }

    public string State { get; set; }

    public int? CustomerId { get; set; }

    public virtual Customer Customer { get; set; }
}
