namespace NMKR.Shared.Model;

public partial class Apilog
{
    public int Id { get; set; }

    public int NftprojectId { get; set; }

    public string Apifunction { get; set; }

    public int Year { get; set; }

    public int Month { get; set; }

    public int Day { get; set; }

    public int Hour { get; set; }

    public int Minute { get; set; }

    public int Ratelimtexceed { get; set; }

    public int Apicalls { get; set; }

    public virtual Nftproject Nftproject { get; set; }
}
