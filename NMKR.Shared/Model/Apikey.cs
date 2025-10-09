using System;
using System.Collections.Generic;

namespace NMKR.Shared.Model;

public partial class Apikey
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public string Apikeyhash { get; set; }

    public DateTime Created { get; set; }

    public DateTime Expiration { get; set; }

    public string State { get; set; }

    public bool? Purchaserandomnft { get; set; }

    public bool? Uploadnft { get; set; }

    public bool? Listnft { get; set; }

    public bool? Makepayouts { get; set; }

    public string Comment { get; set; }

    public bool? Purchasespecificnft { get; set; }

    public bool? Checkaddresses { get; set; }

    public bool? Createprojects { get; set; }

    public string Apikeystartandend { get; set; }

    public bool? Listprojects { get; set; }

    public bool? Walletvalidation { get; set; }

    public bool? Paymenttransactions { get; set; }

    public virtual ICollection<Apikeyaccess> Apikeyaccesses { get; set; } = new List<Apikeyaccess>();

    public virtual Customer Customer { get; set; }
}
