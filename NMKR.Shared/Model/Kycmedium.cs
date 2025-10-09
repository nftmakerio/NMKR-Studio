namespace NMKR.Shared.Model;

public partial class Kycmedium
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public string Mimetype { get; set; }

    public string Documenttype { get; set; }

    public string Base64uri { get; set; }

    public byte[] Content { get; set; }

    public string Contenttext { get; set; }

    public virtual Customer Customer { get; set; }
}
