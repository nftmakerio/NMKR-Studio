using System.Collections.Generic;

namespace NMKR.Shared.Model;

public partial class Setting
{
    public int Id { get; set; }

    public long Mintingcosts { get; set; }

    public string Mintingaddress { get; set; }

    public string Mintingaddressdescription { get; set; }

    public long Minutxo { get; set; }

    public long Minfees { get; set; }

    public string Metadatascaffold { get; set; }

    public string Description { get; set; }

    public int Minimumtxcount { get; set; }

    public int? MastersettingsId { get; set; }

    public float Feespercent { get; set; }

    /// <summary>
    /// When an upload source was  passed by the api function (uploadNft), we will set the price settings of the project to this setting
    /// </summary>
    public string Uploadsourceforceprice { get; set; }

    public int Mintandsendcoupons { get; set; }

    public long Mintingcostssolana { get; set; }

    public string Mintingaddresssolana { get; set; }

    public long Pricemintcoupons { get; set; }

    public float Priceupdatenfts { get; set; }

    public long Pricemintcouponssolana { get; set; }

    public long Pricemintcouponsaptos { get; set; }

    public long Mintingcostsaptos { get; set; }

    public string Mintingaddresssaptos { get; set; }

    public long Mintingcostsbitcoin { get; set; }

    public string Mintingaddressbitcoin { get; set; }

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

    public virtual ICollection<Setting> InverseMastersettings { get; set; } = new List<Setting>();

    public virtual Setting Mastersettings { get; set; }

    public virtual ICollection<Nftproject> Nftprojects { get; set; } = new List<Nftproject>();
}
