using System;

namespace NMKR.Shared.Model;

public partial class TransactionNft
{
    public int Id { get; set; }

    public int TransactionId { get; set; }

    public int? NftId { get; set; }

    public bool Mintedontransaction { get; set; }

    public long Tokencount { get; set; }

    public int? NftarchiveId { get; set; }

    public long Multiplier { get; set; }

    public bool Ispromotion { get; set; }

    public string Txhash { get; set; }

    public bool? Confirmed { get; set; }

    public DateTime? Checkforconfirmdate { get; set; }

    public long? Transactionblockchaintime { get; set; }

    public virtual Nft Nft { get; set; }

    public virtual NftsArchive Nftarchive { get; set; }

    public virtual Transaction Transaction { get; set; }
}
