using System;
using System.Collections.Generic;

namespace NMKR.Shared.Model;

public partial class Nftaddress
{
    public int Id { get; set; }

    public string Address { get; set; }

    public DateTime Expires { get; set; }

    public long? Price { get; set; }

    public string Privateskey { get; set; }

    public string Privatevkey { get; set; }

    public string State { get; set; }

    public DateTime Created { get; set; }

    public long? Lovelace { get; set; }

    public int? NftprojectId { get; set; }

    public string Txid { get; set; }

    public string Senderaddress { get; set; }

    public DateTime? Paydate { get; set; }

    public string Salt { get; set; }

    public long Utxo { get; set; }

    public DateTime? Lastcheckforutxo { get; set; }

    public string Errormessage { get; set; }

    public string Submissionresult { get; set; }

    public long? Countnft { get; set; }

    public string Reservationtype { get; set; }

    public int? Checkonlybyserverid { get; set; }

    /// <summary>
    /// not used at the moment
    /// </summary>
    public long Tokencount { get; set; }

    public string Reservationtoken { get; set; }

    public int? Serverid { get; set; }

    public int Addresscheckedcounter { get; set; }

    public bool Checkfordoublepayment { get; set; }

    public string Rejectreason { get; set; }

    public string Rejectparameter { get; set; }

    public string Ipaddress { get; set; }

    public long? Stakereward { get; set; }

    public long? Priceintoken { get; set; }

    public string Tokenpolicyid { get; set; }

    public string Tokenassetid { get; set; }

    public long Tokenmultiplier { get; set; }

    public long? Foundinslot { get; set; }

    public long? Discount { get; set; }

    public long Sendbacktouser { get; set; }

    public int? RefererId { get; set; }

    public int? PromotionId { get; set; }

    public int? Promotionmultiplier { get; set; }

    public string Customproperty { get; set; }

    public long? Tokenreward { get; set; }

    /// <summary>
    /// This stores the recevieraddress if specified (but it is optional)
    /// </summary>
    public string Optionalreceiveraddress { get; set; }

    public string Paymentmethod { get; set; }

    public int? PreparedpaymenttransactionsId { get; set; }

    public string Outgoingtxhash { get; set; }

    public string Refererstring { get; set; }

    public string Refundreceiveraddress { get; set; }

    public bool? Lovelaceamountmustbeexact { get; set; }

    public string Stakevkey { get; set; }

    public string Stakeskey { get; set; }

    public string Addresstype { get; set; }

    public string Coin { get; set; }

    public string Seedphrase { get; set; }

    public bool Freemint { get; set; }

    public virtual Nftproject Nftproject { get; set; }

    public virtual ICollection<Nfttonftaddress> Nfttonftaddresses { get; set; } = new List<Nfttonftaddress>();

    public virtual ICollection<Nfttonftaddresseshistory> Nfttonftaddresseshistories { get; set; } = new List<Nfttonftaddresseshistory>();

    public virtual Preparedpaymenttransaction Preparedpaymenttransactions { get; set; }

    public virtual ICollection<Preparedpaymenttransaction> PreparedpaymenttransactionsNavigation { get; set; } = new List<Preparedpaymenttransaction>();

    public virtual Promotion Promotion { get; set; }

    public virtual Referer Referer { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
