using System;
using System.Collections.Generic;

namespace NMKR.Shared.Model;

public partial class Customer
{
    public int Id { get; set; }

    public string Email { get; set; }

    public string Password { get; set; }

    public string Salt { get; set; }

    public string Company { get; set; }

    public string Firstname { get; set; }

    public string Lastname { get; set; }

    public string Street { get; set; }

    public string Zip { get; set; }

    public string City { get; set; }

    public int CountryId { get; set; }

    public string Ustid { get; set; }

    public string Confirmationcode { get; set; }

    public string State { get; set; }

    public DateTime Created { get; set; }

    public string Ipaddress { get; set; }

    public int Failedlogon { get; set; }

    public string Twofactor { get; set; }

    public string Mobilenumber { get; set; }

    public DateTime? Lockeduntil { get; set; }

    public int Avatarid { get; set; }

    public string Pendingpassword { get; set; }

    public DateTime? Pendingpasswordcreated { get; set; }

    public bool Sendmailonlogon { get; set; }

    public bool Sendmailonlogonfailure { get; set; }

    public bool Sendmailonpayout { get; set; }

    public bool Sendmailonnews { get; set; }

    public bool Sendmailonservice { get; set; }

    public string Adaaddress { get; set; }

    public string Privatevkey { get; set; }

    public string Privateskey { get; set; }

    public long? Lovelace { get; set; }

    public bool Addressblocked { get; set; }

    public int Blockcounter { get; set; }

    public bool Sendmailonsale { get; set; }

    public int DefaultsettingsId { get; set; }

    public int MarketplacesettingsId { get; set; }

    public bool Checkaddressalways { get; set; }

    public string Ftppassword { get; set; }

    public string Referal { get; set; }

    public int Checkaddresscount { get; set; }

    public DateTime? Lastcheckforutxo { get; set; }

    public string Comments { get; set; }

    public DateTime? Twofactorenabled { get; set; }

    public string Kycaccesstoken { get; set; }

    public DateTime? Kycprocessed { get; set; }

    public string Kycstatus { get; set; }

    public string Checkkycstate { get; set; }

    public bool? Showkycstate { get; set; }

    public bool Showpayoutbutton { get; set; }

    public string Kycresultmessage { get; set; }

    /// <summary>
    /// 200 = 2 percent
    /// </summary>
    public int Splitroyaltyaddressespercentage { get; set; }

    /// <summary>
    /// Will not used anymore - use newpurchasedmints now
    /// </summary>
    public int Purchasedmints { get; set; }

    public int? DefaultpromotionId { get; set; }

    public string Lasttxhash { get; set; }

    public string Stakevkey { get; set; }

    public string Stakeskey { get; set; }

    /// <summary>
    /// Mark if this account is uses as internal account
    /// </summary>
    public bool Internalaccount { get; set; }

    public long Chargemintandsendcostslovelace { get; set; }

    public string Connectedwallettype { get; set; }

    public string Connectedwalletchangeaddress { get; set; }

    public bool Donotneedtolocktokens { get; set; }

    /// <summary>
    /// Yoti or IAMX
    /// </summary>
    public string Kycprovider { get; set; }

    /// <summary>
    /// New Mint Coupons with comma. So we can have also 0,5 mint coupons for an update
    /// </summary>
    public float Newpurchasedmints { get; set; }

    public string Solanaseedphrase { get; set; }

    public string Solanapublickey { get; set; }

    public long Lamports { get; set; }

    public bool Soladdressblocked { get; set; }

    public DateTime? Sollastcheckforutxo { get; set; }

    public int? SubcustomerId { get; set; }

    public string Subcustomerdescription { get; set; }

    public string Subcustomerexternalid { get; set; }

    public string Aptosaddress { get; set; }

    public string Aptosprivatekey { get; set; }

    public string Aptosseed { get; set; }

    public bool Aptaddressblocked { get; set; }

    public DateTime? Aptlastcheckforutxo { get; set; }

    public long Octas { get; set; }

    public bool? Two2falogin { get; set; }

    public bool? Two2facreatewallet { get; set; }

    public bool? Two2faexportkeys { get; set; }

    public bool? Two2fapaymentsmanagedwallets { get; set; }

    public bool? Two2fadeleteprojects { get; set; }

    public bool? Two2facreateapikey { get; set; }

    public virtual ICollection<Apikey> Apikeys { get; set; } = new List<Apikey>();

    public virtual ICollection<Buyoutsmartcontractaddress> Buyoutsmartcontractaddresses { get; set; } = new List<Buyoutsmartcontractaddress>();

    public virtual Country Country { get; set; }

    public virtual ICollection<Custodialwallet> Custodialwallets { get; set; } = new List<Custodialwallet>();

    public virtual ICollection<Customeraddress> Customeraddresses { get; set; } = new List<Customeraddress>();

    public virtual ICollection<Customerlogin> Customerlogins { get; set; } = new List<Customerlogin>();

    public virtual ICollection<Customerwallet> Customerwallets { get; set; } = new List<Customerwallet>();

    public virtual Promotion Defaultpromotion { get; set; }

    public virtual Setting Defaultsettings { get; set; }

    public virtual ICollection<Directsale> Directsales { get; set; } = new List<Directsale>();

    public virtual ICollection<Getaccesstokensuser> Getaccesstokensusers { get; set; } = new List<Getaccesstokensuser>();

    public virtual ICollection<Customer> InverseSubcustomer { get; set; } = new List<Customer>();

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual ICollection<Ipfsupload> Ipfsuploads { get; set; } = new List<Ipfsupload>();

    public virtual ICollection<Kycmedium> Kycmedia { get; set; } = new List<Kycmedium>();

    public virtual ICollection<Legacyauction> Legacyauctions { get; set; } = new List<Legacyauction>();

    public virtual ICollection<Legacydirectsale> Legacydirectsales { get; set; } = new List<Legacydirectsale>();

    public virtual ICollection<Lockedasset> Lockedassets { get; set; } = new List<Lockedasset>();

    public virtual ICollection<Loggedinhash> Loggedinhashes { get; set; } = new List<Loggedinhash>();

    public virtual Smartcontractsmarketplacesetting Marketplacesettings { get; set; }

    public virtual ICollection<Mintandsend> Mintandsends { get; set; } = new List<Mintandsend>();

    public virtual ICollection<Nftproject> Nftprojects { get; set; } = new List<Nftproject>();

    public virtual ICollection<Onlinenotification> Onlinenotifications { get; set; } = new List<Onlinenotification>();

    public virtual ICollection<Payoutrequest> Payoutrequests { get; set; } = new List<Payoutrequest>();

    public virtual ICollection<Referer> Referers { get; set; } = new List<Referer>();

    public virtual ICollection<Splitroyaltyaddress> Splitroyaltyaddresses { get; set; } = new List<Splitroyaltyaddress>();

    public virtual ICollection<Statistic> Statistics { get; set; } = new List<Statistic>();

    public virtual Customer Subcustomer { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual ICollection<Websitelog> Websitelogs { get; set; } = new List<Websitelog>();
}
