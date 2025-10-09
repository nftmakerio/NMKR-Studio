using System;
using System.Collections.Generic;

namespace NMKR.Shared.Model;

public partial class Customerwallet
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public string Walletaddress { get; set; }

    public DateTime Created { get; set; }

    public string State { get; set; }

    public string Ipaddress { get; set; }

    public string Comment { get; set; }

    public string Confirmationcode { get; set; }

    public DateTime? Confirmationvalid { get; set; }

    public string Hash { get; set; }

    public DateTime? Confirmationdate { get; set; }

    public string Cointype { get; set; }

    public virtual Customer Customer { get; set; }

    public virtual ICollection<Nftproject> NftprojectAptoscustomerwallets { get; set; } = new List<Nftproject>();

    public virtual ICollection<Nftproject> NftprojectBitcoincustomerwallets { get; set; } = new List<Nftproject>();

    public virtual ICollection<Nftproject> NftprojectCustomerwallets { get; set; } = new List<Nftproject>();

    public virtual ICollection<Nftproject> NftprojectSolanacustomerwallets { get; set; } = new List<Nftproject>();

    public virtual ICollection<Nftproject> NftprojectUsdcwallets { get; set; } = new List<Nftproject>();

    public virtual ICollection<Nftprojectsadditionalpayout> Nftprojectsadditionalpayouts { get; set; } = new List<Nftprojectsadditionalpayout>();

    public virtual ICollection<Payoutrequest> Payoutrequests { get; set; } = new List<Payoutrequest>();

    public virtual ICollection<Referer> Referers { get; set; } = new List<Referer>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
