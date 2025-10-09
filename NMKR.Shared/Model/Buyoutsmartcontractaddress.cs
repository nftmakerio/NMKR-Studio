using System;
using System.Collections.Generic;

namespace NMKR.Shared.Model;

public partial class Buyoutsmartcontractaddress
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public string Address { get; set; }

    public string Skey { get; set; }

    public string Vkey { get; set; }

    public string Salt { get; set; }

    public long Lovelace { get; set; }

    public string State { get; set; }

    public DateTime Expiredate { get; set; }

    public long Lockamount { get; set; }

    public string Outgoingtxhash { get; set; }

    public string Smartcontracttxhash { get; set; }

    public string Logfile { get; set; }

    public string Transactionid { get; set; }

    public string Receiveraddress { get; set; }

    public long Additionalamount { get; set; }

    public virtual ICollection<BuyoutsmartcontractaddressesNft> BuyoutsmartcontractaddressesNfts { get; set; } = new List<BuyoutsmartcontractaddressesNft>();

    public virtual ICollection<BuyoutsmartcontractaddressesReceiver> BuyoutsmartcontractaddressesReceivers { get; set; } = new List<BuyoutsmartcontractaddressesReceiver>();

    public virtual Customer Customer { get; set; }

    public virtual ICollection<Preparedpaymenttransaction> Preparedpaymenttransactions { get; set; } = new List<Preparedpaymenttransaction>();
}
