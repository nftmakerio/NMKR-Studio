using System;
using System.Collections.Generic;

namespace NMKR.Shared.Model;

public partial class Nft
{
    public int Id { get; set; }

    public int NftprojectId { get; set; }

    public string Filename { get; set; }

    public string Ipfshash { get; set; }

    public string State { get; set; }

    /// <summary>
    /// Tokenprefix (from Projects) + Name = Assetname
    /// </summary>
    public string Name { get; set; }

    public string Displayname { get; set; }

    /// <summary>
    /// If not Null, it is the second (High Resolution Image of the Main Pic) - Used in the Unsig Project
    /// </summary>
    public int? MainnftId { get; set; }

    /// <summary>
    /// Shows, if the NFT is already minted
    /// </summary>
    public bool Minted { get; set; }

    public string Detaildata { get; set; }

    public int? MetadatatemplateId { get; set; }

    public string Receiveraddress { get; set; }

    public string Transactionid { get; set; }

    public long? Mintingfees { get; set; }

    public string Mintingfeespaymentaddress { get; set; }

    public string Mintingfeestransactionid { get; set; }

    public DateTime? Selldate { get; set; }

    public string Soldby { get; set; }

    public string Buildtransaction { get; set; }

    public DateTime? Reserveduntil { get; set; }

    public int? Testmarker { get; set; }

    /// <summary>
    /// The Policy ID should be the same as in the Project - but we load it from Blockfrost to verify
    /// </summary>
    public string Policyid { get; set; }

    /// <summary>
    /// Value from Blockfrost
    /// </summary>
    public string Assetid { get; set; }

    /// <summary>
    /// Value from Blockfrost
    /// </summary>
    public string Assetname { get; set; }

    /// <summary>
    /// Value from Blockfrost
    /// </summary>
    public string Fingerprint { get; set; }

    /// <summary>
    /// Value from Blockfrost
    /// </summary>
    public string Initialminttxhash { get; set; }

    /// <summary>
    /// Value from Blockfrost
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Value from Blockfrost
    /// </summary>
    public string Series { get; set; }

    public DateTime? Markedaserror { get; set; }

    /// <summary>
    /// When the NFT is already minted and in Stock - here is the ID of the Address where it is
    /// </summary>
    public int? InstockpremintedaddressId { get; set; }

    /// <summary>
    /// When true - The program searches for the policyid/fingerprint on blockforst
    /// </summary>
    public bool? Checkpolicyid { get; set; }

    public string Mimetype { get; set; }

    public long Soldcount { get; set; }

    public long Reservedcount { get; set; }

    public long Errorcount { get; set; }

    public long Burncount { get; set; }

    public string Metadataoverride { get; set; }

    public DateTime? Lastpolicycheck { get; set; }

    public bool Uploadedtonftstorage { get; set; }

    public bool Isroyaltytoken { get; set; }

    public long Filesize { get; set; }

    public DateTime? Created { get; set; }

    public long? Price { get; set; }

    public int? NftgroupId { get; set; }

    public string Uid { get; set; }

    public string Reservationtoken { get; set; }

    public long Multiplier { get; set; }

    public string Uploadsource { get; set; }

    public string Cipversion { get; set; }

    public string Metadataoverridecip68 { get; set; }

    public string Iagonid { get; set; }

    public string Iagonuploadresult { get; set; }

    public string Solanacollectionnft { get; set; }

    public string Verifiedcollectionsolana { get; set; }

    public string Verifiedcollectionsignature { get; set; }

    public string Mintedonblockchain { get; set; }

    public string Solanatokenhash { get; set; }

    public long? Pricesolana { get; set; }

    public long? Priceaptos { get; set; }

    public long? Pricemidnight { get; set; }

    public virtual ICollection<Airdrop> Airdrops { get; set; } = new List<Airdrop>();

    public virtual Premintednftsaddress Instockpremintedaddress { get; set; }

    public virtual ICollection<Nft> InverseMainnft { get; set; } = new List<Nft>();

    public virtual Nft Mainnft { get; set; }

    public virtual ICollection<Metadata> Metadata { get; set; } = new List<Metadata>();

    public virtual Nftgroup Nftgroup { get; set; }

    public virtual Nftproject Nftproject { get; set; }

    public virtual ICollection<Nftreservation> Nftreservations { get; set; } = new List<Nftreservation>();

    public virtual ICollection<NftsArchive> NftsArchives { get; set; } = new List<NftsArchive>();

    public virtual ICollection<Nfttonftaddress> Nfttonftaddresses { get; set; } = new List<Nfttonftaddress>();

    public virtual ICollection<Nfttonftaddresseshistory> Nfttonftaddresseshistories { get; set; } = new List<Nfttonftaddresseshistory>();

    public virtual ICollection<PreparedpaymenttransactionsNft> PreparedpaymenttransactionsNfts { get; set; } = new List<PreparedpaymenttransactionsNft>();

    public virtual ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();

    public virtual ICollection<Reservednft> Reservednfts { get; set; } = new List<Reservednft>();

    public virtual ICollection<Soldnft> Soldnfts { get; set; } = new List<Soldnft>();

    public virtual ICollection<TransactionNft> TransactionNfts { get; set; } = new List<TransactionNft>();
}
