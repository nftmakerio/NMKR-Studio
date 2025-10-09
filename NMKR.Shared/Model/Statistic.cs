namespace NMKR.Shared.Model;

public partial class Statistic
{
    public int Id { get; set; }

    public int NftprojectId { get; set; }

    public int CustomerId { get; set; }

    public int Day { get; set; }

    public int Month { get; set; }

    public int Year { get; set; }

    public int Sales { get; set; }

    public double Amount { get; set; }

    public double Mintingcosts { get; set; }

    public double Transactionfees { get; set; }

    public double Minutxo { get; set; }

    public virtual Customer Customer { get; set; }

    public virtual Nftproject Nftproject { get; set; }
}
