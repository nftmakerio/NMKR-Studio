namespace NMKR.Shared.Model;

public partial class DirectsalesNft
{
    public int Id { get; set; }

    public int DirectsaleId { get; set; }

    public string Policyid { get; set; }

    public string Tokennamehex { get; set; }

    public string Ipfshash { get; set; }

    public string Metadata { get; set; }

    public long Tokencount { get; set; }

    public virtual Directsale Directsale { get; set; }
}
