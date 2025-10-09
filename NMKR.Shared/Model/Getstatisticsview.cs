namespace NMKR.Shared.Model;

public partial class Getstatisticsview
{
    public long Totaltransactions { get; set; }

    public string Coin { get; set; }

    public string Transactiontype { get; set; }

    public decimal? Totalsendbacktousers { get; set; }

    public double? Totalsendbacktouserseuro { get; set; }

    public decimal? Totalfees { get; set; }

    public double? Totalfeeseuro { get; set; }

    public decimal? Totalmintingcosts { get; set; }

    public double? Totalmintingcostseuro { get; set; }

    public decimal? Totalpayout { get; set; }

    public double? Totalpayouteuro { get; set; }

    public long Totalnfts { get; set; }

    public decimal? Totalnmkrcosts { get; set; }

    public double? Totalnmkrcostseuro { get; set; }

    public string Countryselect { get; set; }
}
