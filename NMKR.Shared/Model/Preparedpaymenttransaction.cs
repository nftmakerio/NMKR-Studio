using System;
using System.Collections.Generic;

namespace NMKR.Shared.Model;

public partial class Preparedpaymenttransaction
{
    public int Id { get; set; }

    public string Transactionuid { get; set; }

    public string Transactiontype { get; set; }

    public int NftprojectId { get; set; }

    public long? Lovelace { get; set; }

    public DateTime Created { get; set; }

    public int? TransactionId { get; set; }

    public int? ReservationId { get; set; }

    public string State { get; set; }

    public long? Countnft { get; set; }

    public string Customeripaddress { get; set; }

    public int? NftaddressesId { get; set; }

    public int? SmartcontractsId { get; set; }

    public string Policyid { get; set; }

    public long? Tokencount { get; set; }

    public string Tokenname { get; set; }

    public long? Auctionminprice { get; set; }

    public int? Auctionduration { get; set; }

    public string Smartcontractstate { get; set; }

    public string Paymentgatewaystate { get; set; }

    public string Cachedresultgetpaymentaddress { get; set; }

    public string Sellerpkh { get; set; }

    public string Selleraddress { get; set; }

    public long? Estimatedfees { get; set; }

    public int SmartcontractsmarketplaceId { get; set; }

    public string Buyerpkh { get; set; }

    public string Buyeraddresses { get; set; }

    public string Cbor { get; set; }

    public string Signedcbor { get; set; }

    public string Changeaddress { get; set; }

    public string Buyeraddress { get; set; }

    public string Selleraddresses { get; set; }

    public string Reservationtoken { get; set; }

    public string Command { get; set; }

    public string Logfile { get; set; }

    public DateTime? Expires { get; set; }

    public long? Fee { get; set; }

    public int? LegacyauctionsId { get; set; }

    public long? Lockamount { get; set; }

    public int? LegacydirectsalesId { get; set; }

    public int? MintandsendId { get; set; }

    public long? Stakerewards { get; set; }

    public long? Discount { get; set; }

    public string Rejectparameter { get; set; }

    public string Rejectreason { get; set; }

    public string Txhash { get; set; }

    public DateTime? Submitteddate { get; set; }

    public DateTime? Confirmeddate { get; set; }

    public string Referer { get; set; }

    public string Createroyaltytokenaddress { get; set; }

    public float? Createroyaltytokenpercentage { get; set; }

    public int? PromotionId { get; set; }

    public int? Promotionmultiplier { get; set; }

    public int? ReferencedprepearedtransactionId { get; set; }

    public string Txinforalreadylockedtransactions { get; set; }

    public long? Tokenrewards { get; set; }

    public float? Overridemarketplacefee { get; set; }

    public string Overridemarketplaceaddress { get; set; }

    public int? BuyoutaddressesId { get; set; }

    public string Optionalreceiveraddress { get; set; }

    public virtual Buyoutsmartcontractaddress Buyoutaddresses { get; set; }

    public virtual ICollection<Preparedpaymenttransaction> InverseReferencedprepearedtransaction { get; set; } = new List<Preparedpaymenttransaction>();

    public virtual Legacyauction Legacyauctions { get; set; }

    public virtual Legacydirectsale Legacydirectsales { get; set; }

    public virtual Mintandsend Mintandsend { get; set; }

    public virtual ICollection<Nftaddress> Nftaddresses { get; set; } = new List<Nftaddress>();

    public virtual Nftaddress NftaddressesNavigation { get; set; }

    public virtual Nftproject Nftproject { get; set; }

    public virtual ICollection<PreparedpaymenttransactionsCustomproperty> PreparedpaymenttransactionsCustomproperties { get; set; } = new List<PreparedpaymenttransactionsCustomproperty>();

    public virtual ICollection<PreparedpaymenttransactionsNft> PreparedpaymenttransactionsNfts { get; set; } = new List<PreparedpaymenttransactionsNft>();

    public virtual ICollection<PreparedpaymenttransactionsNotification> PreparedpaymenttransactionsNotifications { get; set; } = new List<PreparedpaymenttransactionsNotification>();

    public virtual ICollection<PreparedpaymenttransactionsSmartcontractOutput> PreparedpaymenttransactionsSmartcontractOutputs { get; set; } = new List<PreparedpaymenttransactionsSmartcontractOutput>();

    public virtual ICollection<PreparedpaymenttransactionsSmartcontractsjson> PreparedpaymenttransactionsSmartcontractsjsons { get; set; } = new List<PreparedpaymenttransactionsSmartcontractsjson>();

    public virtual ICollection<PreparedpaymenttransactionsTokenprice> PreparedpaymenttransactionsTokenprices { get; set; } = new List<PreparedpaymenttransactionsTokenprice>();

    public virtual Promotion Promotion { get; set; }

    public virtual Preparedpaymenttransaction Referencedprepearedtransaction { get; set; }

    public virtual Nftreservation Reservation { get; set; }

    public virtual Smartcontract Smartcontracts { get; set; }

    public virtual Transaction Transaction { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
