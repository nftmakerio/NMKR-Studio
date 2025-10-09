using System;
using System.Collections.Generic;

namespace NMKR.Shared.Model;

public partial class Legacydirectsale
{
    public int Id { get; set; }

    public string Address { get; set; }

    public string Skey { get; set; }

    public string Vkey { get; set; }

    public string Salt { get; set; }

    public int? NftprojectId { get; set; }

    public int? CustomerId { get; set; }

    public long Price { get; set; }

    public string Selleraddress { get; set; }

    public string Buyer { get; set; }

    public DateTime Created { get; set; }

    /// <summary>
    /// Active=Address will monitored, Notactive=Adress not monitored, Finished=Auction finnished, but still monitoring, Ended=Auction finished, not any longer monitoring
    /// </summary>
    public string State { get; set; }

    public float? Royaltyfeespercent { get; set; }

    public string Royaltyaddress { get; set; }

    public float? Marketplacefeepercent { get; set; }

    public string Locknftstxinhashid { get; set; }

    public DateTime? Solddate { get; set; }

    public long Lockamount { get; set; }

    public virtual Customer Customer { get; set; }

    public virtual ICollection<LegacydirectsalesNft> LegacydirectsalesNfts { get; set; } = new List<LegacydirectsalesNft>();

    public virtual Nftproject Nftproject { get; set; }

    public virtual ICollection<Preparedpaymenttransaction> Preparedpaymenttransactions { get; set; } = new List<Preparedpaymenttransaction>();
}
