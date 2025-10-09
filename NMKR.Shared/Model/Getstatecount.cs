namespace NMKR.Shared.Model;

public partial class Getstatecount
{
    public long C { get; set; }

    public int NftprojectId { get; set; }

    public string State { get; set; }

    public decimal Tokensreserved { get; set; }

    public decimal Tokenssold { get; set; }

    public decimal Tokenserror { get; set; }

    public long? Total { get; set; }
}
