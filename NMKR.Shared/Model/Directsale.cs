using System;
using System.Collections.Generic;

namespace NMKR.Shared.Model;

public partial class Directsale
{
    public int Id { get; set; }

    public int SmartcontractId { get; set; }

    public string Transactionid { get; set; }

    public int? NftprojectId { get; set; }

    public int? CustomerId { get; set; }

    public long Price { get; set; }

    public string Selleraddress { get; set; }

    public string Buyer { get; set; }

    public DateTime Created { get; set; }

    public string State { get; set; }

    public float? Royaltyfeespercent { get; set; }

    public string Royaltyaddress { get; set; }

    public float? Marketplacefeepercent { get; set; }

    public float? Nmkrfeepercent { get; set; }

    public float? Refererfeepercent { get; set; }

    public string Locknftstxinhashid { get; set; }

    public DateTime? Solddate { get; set; }

    public long Lockamount { get; set; }

    public string Name { get; set; }

    public string Nmkrpaylink { get; set; }

    public virtual Customer Customer { get; set; }

    public virtual ICollection<DirectsalesNft> DirectsalesNfts { get; set; } = new List<DirectsalesNft>();

    public virtual Nftproject Nftproject { get; set; }

    public virtual Smartcontract Smartcontract { get; set; }
}
