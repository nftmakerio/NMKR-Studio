using System;
using System.Collections.Generic;

namespace NMKR.Shared.Model;

public partial class Promotion
{
    public int Id { get; set; }

    public int NftprojectId { get; set; }

    public int? NftId { get; set; }

    public long Count { get; set; }

    public string State { get; set; }

    public DateTime? Startdate { get; set; }

    public DateTime? Enddate { get; set; }

    public long Soldcount { get; set; }

    public long? Maxcount { get; set; }

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

    public virtual Nft Nft { get; set; }

    public virtual ICollection<Nftaddress> Nftaddresses { get; set; } = new List<Nftaddress>();

    public virtual Nftproject Nftproject { get; set; }

    public virtual ICollection<Nftproject> Nftprojects { get; set; } = new List<Nftproject>();

    public virtual ICollection<Preparedpaymenttransaction> Preparedpaymenttransactions { get; set; } = new List<Preparedpaymenttransaction>();

    public virtual ICollection<Pricelist> Pricelists { get; set; } = new List<Pricelist>();
}
