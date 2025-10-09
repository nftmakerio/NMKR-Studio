using System;

namespace NMKR.Shared.Model;

public partial class NftprojectsView
{
    public int? Id { get; set; }

    public int? CustomerId { get; set; }

    public string Projectname { get; set; }

    public string Payoutaddress { get; set; }

    public string Policyscript { get; set; }

    /// <summary>
    /// This address is the pay in address of the project
    /// </summary>
    public string Policyaddress { get; set; }

    public string Policyid { get; set; }

    public string Policyvkey { get; set; }

    public string Policyskey { get; set; }

    public DateTime? Policyexpire { get; set; }

    public string State { get; set; }

    public string Password { get; set; }

    public string Tokennameprefix { get; set; }

    public int? SettingsId { get; set; }

    public int? Expiretime { get; set; }

    public int? CustomerwalletId { get; set; }

    public string Description { get; set; }

    public long? Maxsupply { get; set; }

    public string Version { get; set; }

    public string Minutxo { get; set; }

    public string Metadata { get; set; }

    public bool? Oldmetadatascheme { get; set; }

    public DateTime? Lastupdate { get; set; }

    public string Projecturl { get; set; }

    public bool? Hasroyality { get; set; }

    public float? Royalitypercent { get; set; }

    public string Royalityaddress { get; set; }

    public DateTime? Royaltiycreated { get; set; }

    public bool? Activatepayinaddress { get; set; }

    public long? Total { get; set; }

    public long? Free { get; set; }

    public long? Reserved { get; set; }

    public long? Sold { get; set; }

    public long? Error { get; set; }

    public long? Totaltokens { get; set; }

    public decimal? Tokenssold { get; set; }

    public decimal? Tokensreserved { get; set; }

    public long? Countprices { get; set; }
}
