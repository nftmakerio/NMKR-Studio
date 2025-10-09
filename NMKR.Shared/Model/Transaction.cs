using System;
using System.Collections.Generic;

namespace NMKR.Shared.Model;

public partial class Transaction
{
    public int Id { get; set; }

    public string Senderaddress { get; set; }

    public string Receiveraddress { get; set; }

    public long Ada { get; set; }

    public DateTime Created { get; set; }

    public long? Fee { get; set; }

    public string Transactiontype { get; set; }

    public string Transactionid { get; set; }

    public int? NftaddressId { get; set; }

    public int? CustomerId { get; set; }

    public string Projectaddress { get; set; }

    public long? Projectada { get; set; }

    public string Mintingcostsaddress { get; set; }

    public long? Mintingcostsada { get; set; }

    public int? NftprojectId { get; set; }

    public string State { get; set; }

    public float? Eurorate { get; set; }

    public int? WalletId { get; set; }

    public int? Serverid { get; set; }

    public string Projectincomingtxhash { get; set; }

    public long? Stakereward { get; set; }

    public long? Discount { get; set; }

    public int? RefererId { get; set; }

    public long? RefererCommission { get; set; }

    public string Originatoraddress { get; set; }

    public string Stakeaddress { get; set; }

    public bool Confirmed { get; set; }

    public DateTime? Checkforconfirmdate { get; set; }

    public string Cbor { get; set; }

    public string Ipaddress { get; set; }

    public string Metadata { get; set; }

    public int? PreparedpaymenttransactionId { get; set; }

    public long? Tokenreward { get; set; }

    public long Nmkrcosts { get; set; }

    public string Paymentmethod { get; set; }

    public int Nftcount { get; set; }

    public long? Telemetrytooktime { get; set; }

    public long? Priceintokensquantity { get; set; }

    public string Priceintokenspolicyid { get; set; }

    public string Priceintokenstokennamehex { get; set; }

    public long? Priceintokensmultiplier { get; set; }

    public bool Stopresubmitting { get; set; }

    public string Customerproperty { get; set; }

    public string Discountcode { get; set; }

    public string Coin { get; set; }

    public long? Incomingtxblockchaintime { get; set; }

    public long? Transactionblockchaintime { get; set; }

    public string Metadatastandard { get; set; }

    public string Cip68referencetokenaddress { get; set; }

    public long? Cip68referencetokenminutxo { get; set; }

    public virtual Customer Customer { get; set; }

    public virtual Nftaddress Nftaddress { get; set; }

    public virtual Nftproject Nftproject { get; set; }

    public virtual Preparedpaymenttransaction Preparedpaymenttransaction { get; set; }

    public virtual ICollection<Preparedpaymenttransaction> Preparedpaymenttransactions { get; set; } = new List<Preparedpaymenttransaction>();

    public virtual Referer Referer { get; set; }

    public virtual ICollection<TransactionNft> TransactionNfts { get; set; } = new List<TransactionNft>();

    public virtual ICollection<TransactionsAdditionalpayout> TransactionsAdditionalpayouts { get; set; } = new List<TransactionsAdditionalpayout>();

    public virtual Customerwallet Wallet { get; set; }
}
