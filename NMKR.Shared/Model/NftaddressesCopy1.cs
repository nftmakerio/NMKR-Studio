using System;

namespace NMKR.Shared.Model;

public partial class NftaddressesCopy1
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

    public virtual Nftproject Nftproject { get; set; }

    public virtual Promotion Promotion { get; set; }

    public virtual Referer Referer { get; set; }
}
