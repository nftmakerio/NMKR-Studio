using System;

namespace NMKR.Shared.Model;

public partial class Transactionstatistic
{
    public long Counttx { get; set; }

    public decimal? Sumada { get; set; }

    public decimal? Sumfee { get; set; }

    public decimal? Sumprojectada { get; set; }

    public decimal? Summintcosts { get; set; }

    public decimal? Sumtotal { get; set; }

    public decimal? Sumcosts { get; set; }

    public int? CustomerId { get; set; }

    public int? NftprojectId { get; set; }

    public DateOnly? D1 { get; set; }
}
