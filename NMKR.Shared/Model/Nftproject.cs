using System;
using System.Collections.Generic;

namespace NMKR.Shared.Model;

public partial class Nftproject
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public string Projectname { get; set; }

    public string Projectlogo { get; set; }

    public string Payoutaddress { get; set; }

    public string Policyscript { get; set; }

    /// <summary>
    /// This address is the pay in address of the project
    /// </summary>
    public string Policyaddress { get; set; }

    public string Policyid { get; set; }

    public string Policyvkey { get; set; }

    public string Policyskey { get; set; }

    public DateTime? Policyexpire { get; set; }

    public string State { get; set; }

    public string Password { get; set; }

    public string Tokennameprefix { get; set; }

    public int SettingsId { get; set; }

    public int Expiretime { get; set; }

    public int? CustomerwalletId { get; set; }

    public string Description { get; set; }

    public long Maxsupply { get; set; }

    public string Version { get; set; }

    public string Minutxo { get; set; }

    public string Metadata { get; set; }

    public bool? Oldmetadatascheme { get; set; }

    public DateTime Lastupdate { get; set; }

    public string Projecturl { get; set; }

    public bool Hasroyality { get; set; }

    public bool Hasidentity { get; set; }

    public float Royalitypercent { get; set; }

    public string Royalityaddress { get; set; }

    public DateTime? Royaltiycreated { get; set; }

    public bool Activatepayinaddress { get; set; }

    public DateTime? Created { get; set; }

    public long Total1 { get; set; }

    public long Free1 { get; set; }

    public long Reserved1 { get; set; }

    public long Sold1 { get; set; }

    public long? Blocked1 { get; set; }

    public long Error1 { get; set; }

    public long Totaltokens1 { get; set; }

    public long Tokenssold1 { get; set; }

    public long Tokensreserved1 { get; set; }

    public long Countprices1 { get; set; }

    public DateTime? Lastinputonaddress { get; set; }

    public string Placeholdercsv { get; set; }

    public bool Checkcsv { get; set; }

    public string Uid { get; set; }

    /// <summary>
    /// This address wil be used, if the policyaddress is already used by an other project with the same policy id
    /// </summary>
    public string Alternativeaddress { get; set; }

    public string Alternativepayskey { get; set; }

    public string Alternativepayvkey { get; set; }

    public int SmartcontractssettingsId { get; set; }

    public DateTime? Lastcheckforutxo { get; set; }

    public int Maxcountmintandsend { get; set; }

    /// <summary>
    /// This field indicates, if we enable the cross sale feature on the paywindow (eg the NMKR Token)
    /// </summary>
    public bool? Enablecrosssaleonpaywindow { get; set; }

    public bool Enablefiat { get; set; }

    public bool Isarchived { get; set; }

    public bool Donotarchive { get; set; }

    public int? UsdcwalletId { get; set; }

    public long Multiplier { get; set; }

    /// <summary>
    /// When will the policy expire - the slot
    /// </summary>
    public long? Lockslot { get; set; }

    /// <summary>
    /// When the PGW starts to be active
    /// </summary>
    public DateTime? Paymentgatewaysalestart { get; set; }

    public bool Enabledecentralpayments { get; set; }

    public int? DefaultpromotionId { get; set; }

    public bool Disablemanualmintingbutton { get; set; }

    public bool Disablerandomsales { get; set; }

    public bool Disablespecificsales { get; set; }

    public long? Nftsblocked { get; set; }

    public string Twitterhandle { get; set; }

    public double? Addrefereramounttopaymenttransactions { get; set; }

    public string Projecttype { get; set; }

    public float? Marketplacewhitelabelfee { get; set; }

    public string Nmkraccountoptions { get; set; }

    public bool Donotdisablepayinaddressautomatically { get; set; }

    public string Crossmintcollectionid { get; set; }

    public bool Cip68 { get; set; }

    public string Cip68referenceaddress { get; set; }

    public int? Cip68smartcontractId { get; set; }

    public string Mintandsendminutxo { get; set; }

    public string Discordurl { get; set; }

    public int Checkfiat { get; set; }

    public string Referenceaddress { get; set; }

    public string Referencevkey { get; set; }

    public string Referenceskey { get; set; }

    public string Twitterurl { get; set; }

    public bool Usefrankenprotection { get; set; }

    public string Storage { get; set; }

    public long Usedstorage { get; set; }

    /// <summary>
    /// obsolete
    /// </summary>
    public bool Enablesolana { get; set; }

    /// <summary>
    /// obsolete
    /// </summary>
    public bool? Enablecardano { get; set; }

    public string Solanaseedphrase { get; set; }

    public string Solanapublickey { get; set; }

    public string Solanasymbol { get; set; }

    public int? SolanacustomerwalletId { get; set; }

    public string Cip68extrafield { get; set; }

    public DateTime? Solanacollectioncreated { get; set; }

    public string Solanacollectiontransaction { get; set; }

    public bool? Integratesolanacollectionaddressinmetadata { get; set; }

    public bool? IntegratecardanopolicyIdinmetadata { get; set; }

    public int? SellerFeeBasisPoints { get; set; }

    public string Solanacollectionimage { get; set; }

    public string Solanacollectionfamily { get; set; }

    public bool Publishmintto3rdpartywebsites { get; set; }

    public string Solanacollectionimagemimetype { get; set; }

    public string Metadatatemplatename { get; set; }

    /// <summary>
    /// New field for all Blockchains as List of the Coins (eg: SOL APT ADA)
    /// </summary>
    public string Enabledcoins { get; set; }

    public int? AptoscustomerwalletId { get; set; }

    public string Aptosaddress { get; set; }

    public string Aptosseedphrase { get; set; }

    public string Aptospublickey { get; set; }

    public DateTime? Aptoscollectioncreated { get; set; }

    public string Aptoscollectiontransaction { get; set; }

    public string Aptoscollectionimagemimetype { get; set; }

    public string Aptoscollectionimage { get; set; }

    public string Aptoscollectionname { get; set; }

    public int? BitcoincustomerwalletId { get; set; }

    public string Bitcoinaddress { get; set; }

    public string Bitcoinseedphrase { get; set; }

    public string Bitcoinpublickey { get; set; }

    public string Bitcoinprivatekey { get; set; }

    public virtual ICollection<Airdrop> Airdrops { get; set; } = new List<Airdrop>();

    public virtual ICollection<Apilog> Apilogs { get; set; } = new List<Apilog>();

    public virtual Customerwallet Aptoscustomerwallet { get; set; }

    public virtual Customerwallet Bitcoincustomerwallet { get; set; }

    public virtual ICollection<Burnigendpoint> Burnigendpoints { get; set; } = new List<Burnigendpoint>();

    public virtual Smartcontract Cip68smartcontract { get; set; }

    public virtual Customer Customer { get; set; }

    public virtual Customerwallet Customerwallet { get; set; }

    public virtual Promotion Defaultpromotion { get; set; }

    public virtual ICollection<Digitalidentity> Digitalidentities { get; set; } = new List<Digitalidentity>();

    public virtual ICollection<Directsale> Directsales { get; set; } = new List<Directsale>();

    public virtual ICollection<Legacyauction> Legacyauctions { get; set; } = new List<Legacyauction>();

    public virtual ICollection<Legacydirectsale> Legacydirectsales { get; set; } = new List<Legacydirectsale>();

    public virtual ICollection<Metadatafield> Metadatafields { get; set; } = new List<Metadatafield>();

    public virtual ICollection<Mintandsend> Mintandsends { get; set; } = new List<Mintandsend>();

    public virtual ICollection<Nftaddress> Nftaddresses { get; set; } = new List<Nftaddress>();

    public virtual ICollection<Nftgroup> Nftgroups { get; set; } = new List<Nftgroup>();

    public virtual ICollection<Nftprojectadaaddress> Nftprojectadaaddresses { get; set; } = new List<Nftprojectadaaddress>();

    public virtual ICollection<Nftprojectsadditionalpayout> Nftprojectsadditionalpayouts { get; set; } = new List<Nftprojectsadditionalpayout>();

    public virtual ICollection<Nftprojectsalecondition> Nftprojectsaleconditions { get; set; } = new List<Nftprojectsalecondition>();

    public virtual ICollection<Nftprojectsendpremintedtoken> Nftprojectsendpremintedtokens { get; set; } = new List<Nftprojectsendpremintedtoken>();

    public virtual ICollection<Nft> Nfts { get; set; } = new List<Nft>();

    public virtual ICollection<NftsArchive> NftsArchives { get; set; } = new List<NftsArchive>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<Premintednftsaddress> Premintednftsaddresses { get; set; } = new List<Premintednftsaddress>();

    public virtual ICollection<Preparedpaymenttransaction> Preparedpaymenttransactions { get; set; } = new List<Preparedpaymenttransaction>();

    public virtual ICollection<Pricelistdiscount> Pricelistdiscounts { get; set; } = new List<Pricelistdiscount>();

    public virtual ICollection<Pricelist> Pricelists { get; set; } = new List<Pricelist>();

    public virtual ICollection<Promotion> Promotions { get; set; } = new List<Promotion>();

    public virtual ICollection<Refundlog> Refundlogs { get; set; } = new List<Refundlog>();

    public virtual Setting Settings { get; set; }

    public virtual ICollection<Smartcontract> Smartcontracts { get; set; } = new List<Smartcontract>();

    public virtual Smartcontractsmarketplacesetting Smartcontractssettings { get; set; }

    public virtual Customerwallet Solanacustomerwallet { get; set; }

    public virtual ICollection<Statistic> Statistics { get; set; } = new List<Statistic>();

    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual Customerwallet Usdcwallet { get; set; }

    public virtual ICollection<Whitelabelstorecollection> Whitelabelstorecollections { get; set; } = new List<Whitelabelstorecollection>();

    public virtual ICollection<Whitelabelstoresetting> Whitelabelstoresettings { get; set; } = new List<Whitelabelstoresetting>();
}
