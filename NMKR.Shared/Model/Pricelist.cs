using System;

namespace NMKR.Shared.Model;

public partial class Pricelist
{
    public int Id { get; set; }

    public int NftprojectId { get; set; }

    public long Countnftortoken { get; set; }

    public long Priceinlovelace { get; set; }

    public DateTime? Validfrom { get; set; }

    public DateTime? Validto { get; set; }

    public string State { get; set; }

    public int? NftgroupId { get; set; }

    public long? Priceintoken { get; set; }

    public string Tokenpolicyid { get; set; }

    public string Tokenassetid { get; set; }

    public string Currency { get; set; }

    public string Changeaddresswhenpaywithtokens { get; set; }

    public int? PromotionId { get; set; }

    public int? Promotionmultiplier { get; set; }

    public long? Tokenmultiplier { get; set; }

    public string Assetnamehex { get; set; }

    public virtual Nftgroup Nftgroup { get; set; }

    public virtual Nftproject Nftproject { get; set; }

    public virtual Promotion Promotion { get; set; }
}
