namespace NMKR.Shared.Model;

public partial class Getprojectstatisticsview
{
    public int? NftprojectId { get; set; }

    public string Projectname { get; set; }

    public int? CustomerId { get; set; }

    public string Transactiontype { get; set; }

    public long Totaltransactions { get; set; }

    public decimal? Totalsendbacktousers { get; set; }

    public double? Totalsendbacktouserseuro { get; set; }

    public decimal? Totalfees { get; set; }

    public double? Totalfeeseuro { get; set; }

    public decimal? Totalmintingcosts { get; set; }

    public double? Totalmintingcostseuro { get; set; }

    public decimal? Totalpayout { get; set; }

    public double? Totalpayouteuro { get; set; }

    public string Coin { get; set; }
}
