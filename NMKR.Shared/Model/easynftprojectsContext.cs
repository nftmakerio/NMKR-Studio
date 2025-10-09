using Microsoft.EntityFrameworkCore;

namespace NMKR.Shared.Model;

public partial class EasynftprojectsContext : DbContext
{
    public EasynftprojectsContext(DbContextOptions<EasynftprojectsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Activeblockchain> Activeblockchains { get; set; }

    public virtual DbSet<Adahandle> Adahandles { get; set; }

    public virtual DbSet<Adminlogin> Adminlogins { get; set; }

    public virtual DbSet<Adminmintandsendaddress> Adminmintandsendaddresses { get; set; }

    public virtual DbSet<Airdrop> Airdrops { get; set; }

    public virtual DbSet<Apikey> Apikeys { get; set; }

    public virtual DbSet<Apikeyaccess> Apikeyaccesses { get; set; }

    public virtual DbSet<Apilog> Apilogs { get; set; }

    public virtual DbSet<Backgroundserver> Backgroundservers { get; set; }

    public virtual DbSet<Backgroundtasklogview> Backgroundtasklogviews { get; set; }

    public virtual DbSet<Backgroundtaskslog> Backgroundtaskslogs { get; set; }

    public virtual DbSet<Blockedipaddress> Blockedipaddresses { get; set; }

    public virtual DbSet<Burnigendpoint> Burnigendpoints { get; set; }

    public virtual DbSet<Buyoutsmartcontractaddress> Buyoutsmartcontractaddresses { get; set; }

    public virtual DbSet<BuyoutsmartcontractaddressesNft> BuyoutsmartcontractaddressesNfts { get; set; }

    public virtual DbSet<BuyoutsmartcontractaddressesReceiver> BuyoutsmartcontractaddressesReceivers { get; set; }

    public virtual DbSet<Countedwhitelist> Countedwhitelists { get; set; }

    public virtual DbSet<Countedwhitelistusedaddress> Countedwhitelistusedaddresses { get; set; }

    public virtual DbSet<Country> Countries { get; set; }

    public virtual DbSet<Counttotal> Counttotals { get; set; }

    public virtual DbSet<Custodialwallet> Custodialwallets { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Customeraddress> Customeraddresses { get; set; }

    public virtual DbSet<Customerlogin> Customerlogins { get; set; }

    public virtual DbSet<Customerwallet> Customerwallets { get; set; }

    public virtual DbSet<Defaulttemplate> Defaulttemplates { get; set; }

    public virtual DbSet<Digitalidentity> Digitalidentities { get; set; }

    public virtual DbSet<Directsale> Directsales { get; set; }

    public virtual DbSet<DirectsalesNft> DirectsalesNfts { get; set; }

    public virtual DbSet<Emailtemplate> Emailtemplates { get; set; }

    public virtual DbSet<Faq> Faqs { get; set; }

    public virtual DbSet<Faqcategory> Faqcategories { get; set; }

    public virtual DbSet<Getaccesstokensuser> Getaccesstokensusers { get; set; }

    public virtual DbSet<Getaddressesfordoublepayment> Getaddressesfordoublepayments { get; set; }

    public virtual DbSet<Getallmetadataplaceholder> Getallmetadataplaceholders { get; set; }

    public virtual DbSet<Getidsforpolicycheck> Getidsforpolicychecks { get; set; }

    public virtual DbSet<Getlimit> Getlimits { get; set; }

    public virtual DbSet<Getprojectstatisticsview> Getprojectstatisticsviews { get; set; }

    public virtual DbSet<Getstatecount> Getstatecounts { get; set; }

    public virtual DbSet<Getstatisticsview> Getstatisticsviews { get; set; }

    public virtual DbSet<Gettokensipaddress> Gettokensipaddresses { get; set; }

    public virtual DbSet<Informationtext> Informationtexts { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<Invoicedetail> Invoicedetails { get; set; }

    public virtual DbSet<Ip2locationDb11> Ip2locationDb11s { get; set; }

    public virtual DbSet<Ipfsupload> Ipfsuploads { get; set; }

    public virtual DbSet<Kycmedium> Kycmedia { get; set; }

    public virtual DbSet<Legacyauction> Legacyauctions { get; set; }

    public virtual DbSet<LegacyauctionsNft> LegacyauctionsNfts { get; set; }

    public virtual DbSet<Legacyauctionshistory> Legacyauctionshistories { get; set; }

    public virtual DbSet<Legacydirectsale> Legacydirectsales { get; set; }

    public virtual DbSet<LegacydirectsalesNft> LegacydirectsalesNfts { get; set; }

    public virtual DbSet<Lockedasset> Lockedassets { get; set; }

    public virtual DbSet<Lockedassetstoken> Lockedassetstokens { get; set; }

    public virtual DbSet<Log> Logs { get; set; }

    public virtual DbSet<Loggedinhash> Loggedinhashes { get; set; }

    public virtual DbSet<Manualrefund> Manualrefunds { get; set; }

    public virtual DbSet<Metadata> Metadata { get; set; }

    public virtual DbSet<Metadatafield> Metadatafields { get; set; }

    public virtual DbSet<Metadatatemplate> Metadatatemplates { get; set; }

    public virtual DbSet<Metadatatemplateadditionalfile> Metadatatemplateadditionalfiles { get; set; }

    public virtual DbSet<Mimetype> Mimetypes { get; set; }

    public virtual DbSet<Mintandsend> Mintandsends { get; set; }

    public virtual DbSet<Newrate> Newrates { get; set; }

    public virtual DbSet<Nft> Nfts { get; set; }

    public virtual DbSet<Nftaddress> Nftaddresses { get; set; }

    public virtual DbSet<Nftgroup> Nftgroups { get; set; }

    public virtual DbSet<Nftproject> Nftprojects { get; set; }

    public virtual DbSet<Nftprojectadaaddress> Nftprojectadaaddresses { get; set; }

    public virtual DbSet<NftprojectsView> NftprojectsViews { get; set; }

    public virtual DbSet<Nftprojectsadditionalpayout> Nftprojectsadditionalpayouts { get; set; }

    public virtual DbSet<Nftprojectsalecondition> Nftprojectsaleconditions { get; set; }

    public virtual DbSet<Nftprojectsendpremintedtoken> Nftprojectsendpremintedtokens { get; set; }

    public virtual DbSet<Nftreservation> Nftreservations { get; set; }

    public virtual DbSet<NftsArchive> NftsArchives { get; set; }

    public virtual DbSet<Nfttonftaddress> Nfttonftaddresses { get; set; }

    public virtual DbSet<Nfttonftaddresseshistory> Nfttonftaddresseshistories { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Notificationqueue> Notificationqueues { get; set; }

    public virtual DbSet<Onlinenotification> Onlinenotifications { get; set; }

    public virtual DbSet<Paybuttoncode> Paybuttoncodes { get; set; }

    public virtual DbSet<Payoutrequest> Payoutrequests { get; set; }

    public virtual DbSet<Plugin> Plugins { get; set; }

    public virtual DbSet<Premintednftsaddress> Premintednftsaddresses { get; set; }

    public virtual DbSet<Premintedpromotokenaddress> Premintedpromotokenaddresses { get; set; }

    public virtual DbSet<Preparedpaymenttransaction> Preparedpaymenttransactions { get; set; }

    public virtual DbSet<PreparedpaymenttransactionsCustomproperty> PreparedpaymenttransactionsCustomproperties { get; set; }

    public virtual DbSet<PreparedpaymenttransactionsNft> PreparedpaymenttransactionsNfts { get; set; }

    public virtual DbSet<PreparedpaymenttransactionsNotification> PreparedpaymenttransactionsNotifications { get; set; }

    public virtual DbSet<PreparedpaymenttransactionsSmartcontractOutput> PreparedpaymenttransactionsSmartcontractOutputs { get; set; }

    public virtual DbSet<PreparedpaymenttransactionsSmartcontractOutputsAsset> PreparedpaymenttransactionsSmartcontractOutputsAssets { get; set; }

    public virtual DbSet<PreparedpaymenttransactionsSmartcontractsjson> PreparedpaymenttransactionsSmartcontractsjsons { get; set; }

    public virtual DbSet<PreparedpaymenttransactionsTokenprice> PreparedpaymenttransactionsTokenprices { get; set; }

    public virtual DbSet<Pricelist> Pricelists { get; set; }

    public virtual DbSet<Pricelistdiscount> Pricelistdiscounts { get; set; }

    public virtual DbSet<Projectaddressestxhash> Projectaddressestxhashes { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    public virtual DbSet<Rate> Rates { get; set; }

    public virtual DbSet<Ratelimit> Ratelimits { get; set; }

    public virtual DbSet<Referer> Referers { get; set; }

    public virtual DbSet<Refundlog> Refundlogs { get; set; }

    public virtual DbSet<Registeredtoken> Registeredtokens { get; set; }

    public virtual DbSet<Reservednft> Reservednfts { get; set; }

    public virtual DbSet<Salenumber> Salenumbers { get; set; }

    public virtual DbSet<Serverexception> Serverexceptions { get; set; }

    public virtual DbSet<Setting> Settings { get; set; }

    public virtual DbSet<Sftpgenericfile> Sftpgenericfiles { get; set; }

    public virtual DbSet<Smartcontract> Smartcontracts { get; set; }

    public virtual DbSet<Smartcontractsjsontemplate> Smartcontractsjsontemplates { get; set; }

    public virtual DbSet<Smartcontractsmarketplacesetting> Smartcontractsmarketplacesettings { get; set; }

    public virtual DbSet<Soldnft> Soldnfts { get; set; }

    public virtual DbSet<Splitroyaltyaddress> Splitroyaltyaddresses { get; set; }

    public virtual DbSet<Splitroyaltyaddressessplit> Splitroyaltyaddressessplits { get; set; }

    public virtual DbSet<Splitroyaltyaddressestransaction> Splitroyaltyaddressestransactions { get; set; }

    public virtual DbSet<Splitroyaltyaddressestransactionssplit> Splitroyaltyaddressestransactionssplits { get; set; }

    public virtual DbSet<Stakepoolreward> Stakepoolrewards { get; set; }

    public virtual DbSet<Statistic> Statistics { get; set; }

    public virtual DbSet<Storesetting> Storesettings { get; set; }

    public virtual DbSet<Submission> Submissions { get; set; }

    public virtual DbSet<Tokenreward> Tokenrewards { get; set; }

    public virtual DbSet<Tooltiphelpertext> Tooltiphelpertexts { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<TransactionNft> TransactionNfts { get; set; }

    public virtual DbSet<TransactionsAdditionalpayout> TransactionsAdditionalpayouts { get; set; }

    public virtual DbSet<Transactionstatistic> Transactionstatistics { get; set; }

    public virtual DbSet<Txhashcache> Txhashcaches { get; set; }

    public virtual DbSet<Updateprojectsid> Updateprojectsids { get; set; }

    public virtual DbSet<Usedaddressesonsalecondition> Usedaddressesonsaleconditions { get; set; }

    public virtual DbSet<Validationaddress> Validationaddresses { get; set; }

    public virtual DbSet<Validationamount> Validationamounts { get; set; }

    public virtual DbSet<Vestingoffer> Vestingoffers { get; set; }

    public virtual DbSet<Websitelog> Websitelogs { get; set; }

    public virtual DbSet<Websitesetting> Websitesettings { get; set; }

    public virtual DbSet<Whitelabelstorecollection> Whitelabelstorecollections { get; set; }

    public virtual DbSet<Whitelabelstoresetting> Whitelabelstoresettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Activeblockchain>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("activeblockchains");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Coinname)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("coinname");
            entity.Property(e => e.Collectionaddressmustbefunded).HasColumnName("collectionaddressmustbefunded");
            entity.Property(e => e.Collectionmustbecreatedonnewproject).HasColumnName("collectionmustbecreatedonnewproject");
            entity.Property(e => e.Enabled).HasColumnName("enabled");
            entity.Property(e => e.Explorerurladdress)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("explorerurladdress");
            entity.Property(e => e.Explorerurlcollection)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("explorerurlcollection");
            entity.Property(e => e.Explorerurltx)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("explorerurltx");
            entity.Property(e => e.Factor).HasColumnName("factor");
            entity.Property(e => e.Hasft).HasColumnName("hasft");
            entity.Property(e => e.Hasnft)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("hasnft");
            entity.Property(e => e.Image)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("image");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Smallestentity)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("smallestentity");
        });

        modelBuilder.Entity<Adahandle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("adahandles")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Comment)
                .HasMaxLength(255)
                .HasColumnName("comment");
            entity.Property(e => e.Policyid)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("policyid");
            entity.Property(e => e.Prefix)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("prefix");
        });

        modelBuilder.Entity<Adminlogin>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("adminlogins")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Failedlogon).HasColumnName("failedlogon");
            entity.Property(e => e.Ipaddress)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("ipaddress");
            entity.Property(e => e.Lockeduntil)
                .HasColumnType("datetime")
                .HasColumnName("lockeduntil");
            entity.Property(e => e.Mobilenumber)
                .HasMaxLength(255)
                .HasColumnName("mobilenumber");
            entity.Property(e => e.Password)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Pendingpassword)
                .HasMaxLength(255)
                .HasColumnName("pendingpassword");
            entity.Property(e => e.Pendingpasswordcreated)
                .HasColumnType("datetime")
                .HasColumnName("pendingpasswordcreated");
            entity.Property(e => e.Salt)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("salt");
            entity.Property(e => e.State)
                .IsRequired()
                .HasColumnType("enum('active','notactive','blocked','locked','deleted')")
                .HasColumnName("state");
            entity.Property(e => e.Twofactor)
                .IsRequired()
                .HasColumnType("enum('none','sms','google')")
                .HasColumnName("twofactor");
        });

        modelBuilder.Entity<Adminmintandsendaddress>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("adminmintandsendaddresses")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.Addressblocked).HasColumnName("addressblocked");
            entity.Property(e => e.Blockcounter).HasColumnName("blockcounter");
            entity.Property(e => e.Coin)
                .IsRequired()
                .HasDefaultValueSql("'ADA'")
                .HasColumnType("enum('ADA','SOL','APT','ETH','MATIC','HBAR','BTC')")
                .HasColumnName("coin")
                .UseCollation("latin1_swedish_ci")
                .HasCharSet("latin1");
            entity.Property(e => e.Lastcheckforutxo)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("lastcheckforutxo");
            entity.Property(e => e.Lasttxdate)
                .HasColumnType("datetime")
                .HasColumnName("lasttxdate");
            entity.Property(e => e.Lasttxhash)
                .HasMaxLength(255)
                .HasColumnName("lasttxhash");
            entity.Property(e => e.Lovelace).HasColumnName("lovelace");
            entity.Property(e => e.Privateskey)
                .IsRequired()
                .HasColumnName("privateskey");
            entity.Property(e => e.Privatevkey)
                .IsRequired()
                .HasColumnName("privatevkey");
            entity.Property(e => e.Reservationtoken)
                .HasMaxLength(255)
                .HasColumnName("reservationtoken");
            entity.Property(e => e.Salt)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("salt");
            entity.Property(e => e.Seed)
                .HasColumnType("text")
                .HasColumnName("seed");
        });

        modelBuilder.Entity<Airdrop>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("airdrops");

            entity.HasIndex(e => e.MintandsendId, "airdrops_mintandsend");

            entity.HasIndex(e => e.NftprojectId, "airdrops_nftprojects");

            entity.HasIndex(e => e.NftId, "airdrops_nfts");

            entity.HasIndex(e => e.Uid, "airdrops_uid");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.Message)
                .HasMaxLength(255)
                .HasColumnName("message");
            entity.Property(e => e.MintandsendId).HasColumnName("mintandsend_id");
            entity.Property(e => e.NftId).HasColumnName("nft_id");
            entity.Property(e => e.NftprojectId).HasColumnName("nftproject_id");
            entity.Property(e => e.Recevieraddress)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("recevieraddress");
            entity.Property(e => e.Uid)
                .IsRequired()
                .HasColumnName("uid");

            entity.HasOne(d => d.Mintandsend).WithMany(p => p.Airdrops)
                .HasForeignKey(d => d.MintandsendId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("airdrops_mintandsend");

            entity.HasOne(d => d.Nft).WithMany(p => p.Airdrops)
                .HasForeignKey(d => d.NftId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("airdrops_nfts");

            entity.HasOne(d => d.Nftproject).WithMany(p => p.Airdrops)
                .HasForeignKey(d => d.NftprojectId)
                .HasConstraintName("airdrops_nftprojects");
        });

        modelBuilder.Entity<Apikey>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("apikeys")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.Apikeyhash, "apikey").IsUnique();

            entity.HasIndex(e => e.CustomerId, "apikeys_customers");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Apikeyhash)
                .IsRequired()
                .HasColumnName("apikeyhash");
            entity.Property(e => e.Apikeystartandend)
                .HasMaxLength(255)
                .HasColumnName("apikeystartandend");
            entity.Property(e => e.Checkaddresses).HasColumnName("checkaddresses");
            entity.Property(e => e.Comment)
                .HasMaxLength(255)
                .HasColumnName("comment");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.Createprojects).HasColumnName("createprojects");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Expiration)
                .HasColumnType("datetime")
                .HasColumnName("expiration");
            entity.Property(e => e.Listnft).HasColumnName("listnft");
            entity.Property(e => e.Listprojects).HasColumnName("listprojects");
            entity.Property(e => e.Makepayouts).HasColumnName("makepayouts");
            entity.Property(e => e.Paymenttransactions).HasColumnName("paymenttransactions");
            entity.Property(e => e.Purchaserandomnft).HasColumnName("purchaserandomnft");
            entity.Property(e => e.Purchasespecificnft).HasColumnName("purchasespecificnft");
            entity.Property(e => e.State)
                .HasColumnType("enum('active','revoked','deleted')")
                .HasColumnName("state");
            entity.Property(e => e.Uploadnft).HasColumnName("uploadnft");
            entity.Property(e => e.Walletvalidation).HasColumnName("walletvalidation");

            entity.HasOne(d => d.Customer).WithMany(p => p.Apikeys)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("apikeys_customers");
        });

        modelBuilder.Entity<Apikeyaccess>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("apikeyaccess")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.ApikeyId, "apikeyaccess_apikeys");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Accessfrom)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("accessfrom");
            entity.Property(e => e.ApikeyId).HasColumnName("apikey_id");
            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(255)
                .HasDefaultValueSql("''")
                .HasColumnName("description");
            entity.Property(e => e.Order).HasColumnName("order");
            entity.Property(e => e.State)
                .IsRequired()
                .HasColumnType("enum('allowed','forbidden')")
                .HasColumnName("state");

            entity.HasOne(d => d.Apikey).WithMany(p => p.Apikeyaccesses)
                .HasForeignKey(d => d.ApikeyId)
                .HasConstraintName("apikeyaccess_apikeys");
        });

        modelBuilder.Entity<Apilog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("apilogs")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => new { e.Year, e.Month, e.Day, e.Hour, e.Minute, e.NftprojectId }, "apilog1").IsUnique();

            entity.HasIndex(e => e.NftprojectId, "apilogs_projects");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Apicalls).HasColumnName("apicalls");
            entity.Property(e => e.Apifunction)
                .IsRequired()
                .HasColumnType("enum('reservenftrandom','reservenftspecific','mintandsend','mintandsign','checkaddress')")
                .HasColumnName("apifunction");
            entity.Property(e => e.Day).HasColumnName("day");
            entity.Property(e => e.Hour).HasColumnName("hour");
            entity.Property(e => e.Minute).HasColumnName("minute");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.NftprojectId).HasColumnName("nftproject_id");
            entity.Property(e => e.Ratelimtexceed).HasColumnName("ratelimtexceed");
            entity.Property(e => e.Year).HasColumnName("year");

            entity.HasOne(d => d.Nftproject).WithMany(p => p.Apilogs)
                .HasForeignKey(d => d.NftprojectId)
                .HasConstraintName("apilogs_projects");
        });

        modelBuilder.Entity<Backgroundserver>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("backgroundserver")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Actualprojectid).HasColumnName("actualprojectid");
            entity.Property(e => e.Actualtask)
                .HasMaxLength(255)
                .HasColumnName("actualtask");
            entity.Property(e => e.Block)
                .HasMaxLength(255)
                .HasColumnName("block");
            entity.Property(e => e.Checkbuyinsmartcontractaddresses).HasColumnName("checkbuyinsmartcontractaddresses");
            entity.Property(e => e.Checkcustomeraddresses).HasColumnName("checkcustomeraddresses");
            entity.Property(e => e.Checkcustomeraddressescoin)
                .HasMaxLength(255)
                .HasColumnName("checkcustomeraddressescoin");
            entity.Property(e => e.Checkcustomeraddressessolana).HasColumnName("checkcustomeraddressessolana");
            entity.Property(e => e.Checkcustomerchargeaddresses).HasColumnName("checkcustomerchargeaddresses");
            entity.Property(e => e.Checkdecentralsubmits).HasColumnName("checkdecentralsubmits");
            entity.Property(e => e.Checkdoublepayments).HasColumnName("checkdoublepayments");
            entity.Property(e => e.Checkforburningendpoints).HasColumnName("checkforburningendpoints");
            entity.Property(e => e.Checkfordoublepayments).HasColumnName("checkfordoublepayments");
            entity.Property(e => e.Checkforexpirednfts).HasColumnName("checkforexpirednfts");
            entity.Property(e => e.Checkforfreepaymentaddresses).HasColumnName("checkforfreepaymentaddresses");
            entity.Property(e => e.Checkforpremintedaddresses).HasColumnName("checkforpremintedaddresses");
            entity.Property(e => e.Checklegacyauctions).HasColumnName("checklegacyauctions");
            entity.Property(e => e.Checklegacydirectsales).HasColumnName("checklegacydirectsales");
            entity.Property(e => e.Checkmintandsend).HasColumnName("checkmintandsend");
            entity.Property(e => e.Checkmintandsendcoin)
                .HasMaxLength(255)
                .HasColumnName("checkmintandsendcoin");
            entity.Property(e => e.Checkmintandsendsolana).HasColumnName("checkmintandsendsolana");
            entity.Property(e => e.Checknotificationqueue).HasColumnName("checknotificationqueue");
            entity.Property(e => e.Checkpaymentaddresses).HasColumnName("checkpaymentaddresses");
            entity.Property(e => e.Checkpaymentaddressescoin)
                .HasMaxLength(255)
                .HasColumnName("checkpaymentaddressescoin");
            entity.Property(e => e.Checkpaymentaddressessolana).HasColumnName("checkpaymentaddressessolana");
            entity.Property(e => e.Checkpolicies).HasColumnName("checkpolicies");
            entity.Property(e => e.Checkpoliciescoin)
                .HasMaxLength(255)
                .HasColumnName("checkpoliciescoin");
            entity.Property(e => e.Checkpoliciessolana).HasColumnName("checkpoliciessolana");
            entity.Property(e => e.Checkprojectaddresses).HasColumnName("checkprojectaddresses");
            entity.Property(e => e.Checkrates).HasColumnName("checkrates");
            entity.Property(e => e.Checkroyaltysplitaddresses).HasColumnName("checkroyaltysplitaddresses");
            entity.Property(e => e.Checktransactionconfirmations).HasColumnName("checktransactionconfirmations");
            entity.Property(e => e.Checkvalidationaddresses).HasColumnName("checkvalidationaddresses");
            entity.Property(e => e.Digitaloceanserver).HasColumnName("digitaloceanserver");
            entity.Property(e => e.Epoch)
                .HasMaxLength(255)
                .HasColumnName("epoch");
            entity.Property(e => e.Era)
                .HasMaxLength(255)
                .HasColumnName("era");
            entity.Property(e => e.Executedatabasecommands).HasColumnName("executedatabasecommands");
            entity.Property(e => e.Executepayoutrequests).HasColumnName("executepayoutrequests");
            entity.Property(e => e.Executesubmissions).HasColumnName("executesubmissions");
            entity.Property(e => e.Installedmemory)
                .HasMaxLength(255)
                .HasColumnName("installedmemory");
            entity.Property(e => e.Ipaddress)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("ipaddress");
            entity.Property(e => e.Lastlifesign)
                .HasColumnType("datetime")
                .HasColumnName("lastlifesign");
            entity.Property(e => e.Mintxcheckdoublepayments)
                .HasDefaultValueSql("'10'")
                .HasColumnName("mintxcheckdoublepayments");
            entity.Property(e => e.Monitorthisserver).HasColumnName("monitorthisserver");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Nodeversion)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("nodeversion");
            entity.Property(e => e.Operatingsystem)
                .HasMaxLength(255)
                .HasColumnName("operatingsystem");
            entity.Property(e => e.Pauseserver).HasColumnName("pauseserver");
            entity.Property(e => e.Ratelimitperminute).HasColumnName("ratelimitperminute");
            entity.Property(e => e.Runningversion)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("runningversion");
            entity.Property(e => e.Slot)
                .HasMaxLength(255)
                .HasColumnName("slot");
            entity.Property(e => e.State)
                .HasColumnType("enum('active','notactive')")
                .HasColumnName("state");
            entity.Property(e => e.Stopserver).HasColumnName("stopserver");
            entity.Property(e => e.Syncprogress)
                .HasMaxLength(255)
                .HasColumnName("syncprogress");
            entity.Property(e => e.Url)
                .IsRequired()
                .HasMaxLength(255)
                .HasDefaultValueSql("''")
                .HasColumnName("url");
        });

        modelBuilder.Entity<Backgroundtasklogview>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("backgroundtasklogview");

            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Logmessage)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("logmessage")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<Backgroundtaskslog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("backgroundtaskslog")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Additionaldata).HasColumnName("additionaldata");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.Logmessage)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("logmessage");
        });

        modelBuilder.Entity<Blockedipaddress>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("blockedipaddresses")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.Ipaddress, "ipaddress").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Blockcounter).HasColumnName("blockcounter");
            entity.Property(e => e.Blockeduntil)
                .HasColumnType("datetime")
                .HasColumnName("blockeduntil");
            entity.Property(e => e.Ipaddress)
                .IsRequired()
                .HasMaxLength(50)
                .IsFixedLength()
                .HasColumnName("ipaddress");
        });

        modelBuilder.Entity<Burnigendpoint>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("burnigendpoints")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.NftprojectId, "buningendpoints_nftprojects");

            entity.HasIndex(e => e.Validuntil, "validuntil");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.Blockchain)
                .IsRequired()
                .HasDefaultValueSql("'Cardano'")
                .HasColumnType("enum('Cardano','Solana')")
                .HasColumnName("blockchain");
            entity.Property(e => e.Fixnfts).HasColumnName("fixnfts");
            entity.Property(e => e.Lovelace).HasColumnName("lovelace");
            entity.Property(e => e.NftprojectId).HasColumnName("nftproject_id");
            entity.Property(e => e.Privateskey)
                .IsRequired()
                .HasColumnName("privateskey");
            entity.Property(e => e.Privatevkey)
                .IsRequired()
                .HasColumnName("privatevkey");
            entity.Property(e => e.Salt)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("salt");
            entity.Property(e => e.Shownotification).HasColumnName("shownotification");
            entity.Property(e => e.State)
                .IsRequired()
                .HasDefaultValueSql("'active'")
                .HasColumnType("enum('active','notactive')")
                .HasColumnName("state");
            entity.Property(e => e.Validuntil)
                .HasColumnType("datetime")
                .HasColumnName("validuntil");

            entity.HasOne(d => d.Nftproject).WithMany(p => p.Burnigendpoints)
                .HasForeignKey(d => d.NftprojectId)
                .HasConstraintName("buningendpoints_nftprojects");
        });

        modelBuilder.Entity<Buyoutsmartcontractaddress>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("buyoutsmartcontractaddresses");

            entity.HasIndex(e => e.CustomerId, "buyoutsmartcontractaddresses_customers");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Additionalamount).HasColumnName("additionalamount");
            entity.Property(e => e.Address)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Expiredate)
                .HasColumnType("datetime")
                .HasColumnName("expiredate");
            entity.Property(e => e.Lockamount).HasColumnName("lockamount");
            entity.Property(e => e.Logfile)
                .HasColumnType("text")
                .HasColumnName("logfile");
            entity.Property(e => e.Lovelace).HasColumnName("lovelace");
            entity.Property(e => e.Outgoingtxhash)
                .HasMaxLength(255)
                .HasColumnName("outgoingtxhash");
            entity.Property(e => e.Receiveraddress)
                .HasMaxLength(255)
                .HasColumnName("receiveraddress");
            entity.Property(e => e.Salt)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("salt");
            entity.Property(e => e.Skey)
                .IsRequired()
                .HasColumnType("text")
                .HasColumnName("skey");
            entity.Property(e => e.Smartcontracttxhash)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("smartcontracttxhash");
            entity.Property(e => e.State)
                .IsRequired()
                .HasColumnType("enum('active','payment_received','finished','expired','error','inprogress','refunded')")
                .HasColumnName("state");
            entity.Property(e => e.Transactionid)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("transactionid");
            entity.Property(e => e.Vkey)
                .IsRequired()
                .HasColumnType("text")
                .HasColumnName("vkey");

            entity.HasOne(d => d.Customer).WithMany(p => p.Buyoutsmartcontractaddresses)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("buyoutsmartcontractaddresses_customers");
        });

        modelBuilder.Entity<BuyoutsmartcontractaddressesNft>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("buyoutsmartcontractaddresses_nfts");

            entity.HasIndex(e => e.BuyoutsmartcontractaddressesIid, "buyoutsmartcontractaddresses");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BuyoutsmartcontractaddressesIid).HasColumnName("buyoutsmartcontractaddresses_iid");
            entity.Property(e => e.Policyid)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("policyid");
            entity.Property(e => e.Tokencount).HasColumnName("tokencount");
            entity.Property(e => e.Tokennameinhex)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("tokennameinhex");

            entity.HasOne(d => d.BuyoutsmartcontractaddressesI).WithMany(p => p.BuyoutsmartcontractaddressesNfts)
                .HasForeignKey(d => d.BuyoutsmartcontractaddressesIid)
                .HasConstraintName("buyoutsmartcontractaddresses");
        });

        modelBuilder.Entity<BuyoutsmartcontractaddressesReceiver>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("buyoutsmartcontractaddresses_receivers");

            entity.HasIndex(e => e.BuyoutsmartcontractaddressesId, "buyoutsmartcontractaddresses_2");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BuyoutsmartcontractaddressesId).HasColumnName("buyoutsmartcontractaddresses_id");
            entity.Property(e => e.Lovelace).HasColumnName("lovelace");
            entity.Property(e => e.Pkh)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("pkh");
            entity.Property(e => e.Receiveraddress)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("receiveraddress");

            entity.HasOne(d => d.Buyoutsmartcontractaddresses).WithMany(p => p.BuyoutsmartcontractaddressesReceivers)
                .HasForeignKey(d => d.BuyoutsmartcontractaddressesId)
                .HasConstraintName("buyoutsmartcontractaddresses_2");
        });

        modelBuilder.Entity<Countedwhitelist>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("countedwhitelist");

            entity.HasIndex(e => e.SaleconditionsId, "countedwhitelist_saleconditions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.Created)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.Maxcount)
                .HasDefaultValueSql("'1'")
                .HasColumnName("maxcount");
            entity.Property(e => e.SaleconditionsId).HasColumnName("saleconditions_id");
            entity.Property(e => e.Stakeaddress)
                .HasMaxLength(255)
                .HasColumnName("stakeaddress");

            entity.HasOne(d => d.Saleconditions).WithMany(p => p.Countedwhitelists)
                .HasForeignKey(d => d.SaleconditionsId)
                .HasConstraintName("countedwhitelist_saleconditions");
        });

        modelBuilder.Entity<Countedwhitelistusedaddress>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("countedwhitelistusedaddresses");

            entity.HasIndex(e => e.CountedwhitelistId, "countedwhitelistusedaddresses_countedwhitelist");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CountedwhitelistId).HasColumnName("countedwhitelist_id");
            entity.Property(e => e.Countnft).HasColumnName("countnft");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.Originatoraddress)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("originatoraddress");
            entity.Property(e => e.Transactionid)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("transactionid");
            entity.Property(e => e.Usedaddress)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("usedaddress");

            entity.HasOne(d => d.Countedwhitelist).WithMany(p => p.Countedwhitelistusedaddresses)
                .HasForeignKey(d => d.CountedwhitelistId)
                .HasConstraintName("countedwhitelistusedaddresses_countedwhitelist");
        });

        modelBuilder.Entity<Country>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("countries")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Iso)
                .IsRequired()
                .HasMaxLength(2)
                .IsFixedLength()
                .HasColumnName("iso")
                .UseCollation("latin1_swedish_ci")
                .HasCharSet("latin1");
            entity.Property(e => e.Iso3)
                .HasMaxLength(3)
                .IsFixedLength()
                .HasColumnName("iso3")
                .UseCollation("latin1_swedish_ci")
                .HasCharSet("latin1");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(80)
                .HasColumnName("name")
                .UseCollation("latin1_swedish_ci")
                .HasCharSet("latin1");
            entity.Property(e => e.Nicename)
                .IsRequired()
                .HasMaxLength(80)
                .HasColumnName("nicename")
                .UseCollation("latin1_swedish_ci")
                .HasCharSet("latin1");
            entity.Property(e => e.Numcode).HasColumnName("numcode");
            entity.Property(e => e.Phonecode).HasColumnName("phonecode");
        });

        modelBuilder.Entity<Counttotal>(entity =>
        {
            entity.HasKey(e => e.Counttotal1).HasName("PRIMARY");

            entity
                .ToTable("counttotal")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Counttotal1)
                .ValueGeneratedNever()
                .HasColumnName("counttotal");
        });

        modelBuilder.Entity<Custodialwallet>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("custodialwallets");

            entity.HasIndex(e => e.CustomerId, "custodialwallets_customers");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Lastcheckforutxo)
                .HasColumnType("datetime")
                .HasColumnName("lastcheckforutxo");
            entity.Property(e => e.Pincode)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("pincode");
            entity.Property(e => e.Salt)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("salt");
            entity.Property(e => e.Seedphrase)
                .IsRequired()
                .HasColumnType("text")
                .HasColumnName("seedphrase");
            entity.Property(e => e.Skey)
                .IsRequired()
                .HasColumnType("text")
                .HasColumnName("skey");
            entity.Property(e => e.State)
                .IsRequired()
                .HasColumnType("enum('active','blocked','notactive','deleted')")
                .HasColumnName("state");
            entity.Property(e => e.Uid)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("uid");
            entity.Property(e => e.Vkey)
                .IsRequired()
                .HasColumnType("text")
                .HasColumnName("vkey");
            entity.Property(e => e.Walletname)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("walletname");
            entity.Property(e => e.Wallettype)
                .IsRequired()
                .HasColumnType("enum('enterprise','base')")
                .HasColumnName("wallettype");

            entity.HasOne(d => d.Customer).WithMany(p => p.Custodialwallets)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("custodialwallets_customers");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("customers")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.CountryId, "customers_countries");

            entity.HasIndex(e => e.SubcustomerId, "customers_customers");

            entity.HasIndex(e => e.MarketplacesettingsId, "customers_marketplacesettings");

            entity.HasIndex(e => e.DefaultpromotionId, "customers_promotions");

            entity.HasIndex(e => e.DefaultsettingsId, "customers_settings");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Adaaddress)
                .HasMaxLength(255)
                .HasColumnName("adaaddress");
            entity.Property(e => e.Addressblocked).HasColumnName("addressblocked");
            entity.Property(e => e.Aptaddressblocked).HasColumnName("aptaddressblocked");
            entity.Property(e => e.Aptlastcheckforutxo)
                .HasColumnType("datetime")
                .HasColumnName("aptlastcheckforutxo");
            entity.Property(e => e.Aptosaddress)
                .HasMaxLength(255)
                .HasColumnName("aptosaddress");
            entity.Property(e => e.Aptosprivatekey)
                .HasMaxLength(255)
                .HasColumnName("aptosprivatekey");
            entity.Property(e => e.Aptosseed)
                .HasMaxLength(255)
                .HasColumnName("aptosseed");
            entity.Property(e => e.Avatarid).HasColumnName("avatarid");
            entity.Property(e => e.Blockcounter).HasColumnName("blockcounter");
            entity.Property(e => e.Chargemintandsendcostslovelace).HasColumnName("chargemintandsendcostslovelace");
            entity.Property(e => e.Checkaddressalways).HasColumnName("checkaddressalways");
            entity.Property(e => e.Checkaddresscount).HasColumnName("checkaddresscount");
            entity.Property(e => e.Checkkycstate)
                .IsRequired()
                .HasDefaultValueSql("'untilgreen'")
                .HasColumnType("enum('always','untilgreen','never')")
                .HasColumnName("checkkycstate");
            entity.Property(e => e.City)
                .HasMaxLength(255)
                .HasColumnName("city");
            entity.Property(e => e.Comments).HasColumnName("comments");
            entity.Property(e => e.Company)
                .HasMaxLength(255)
                .HasColumnName("company");
            entity.Property(e => e.Confirmationcode)
                .HasMaxLength(255)
                .HasColumnName("confirmationcode");
            entity.Property(e => e.Connectedwalletchangeaddress)
                .HasMaxLength(255)
                .HasColumnName("connectedwalletchangeaddress");
            entity.Property(e => e.Connectedwallettype)
                .HasMaxLength(255)
                .HasColumnName("connectedwallettype");
            entity.Property(e => e.CountryId).HasColumnName("country_id");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.DefaultpromotionId).HasColumnName("defaultpromotion_id");
            entity.Property(e => e.DefaultsettingsId)
                .HasDefaultValueSql("'3'")
                .HasColumnName("defaultsettings_id");
            entity.Property(e => e.Donotneedtolocktokens).HasColumnName("donotneedtolocktokens");
            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Failedlogon).HasColumnName("failedlogon");
            entity.Property(e => e.Firstname)
                .HasMaxLength(255)
                .HasColumnName("firstname");
            entity.Property(e => e.Ftppassword)
                .HasMaxLength(255)
                .HasColumnName("ftppassword");
            entity.Property(e => e.Internalaccount)
                .HasComment("Mark if this account is uses as internal account")
                .HasColumnName("internalaccount");
            entity.Property(e => e.Ipaddress)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("ipaddress");
            entity.Property(e => e.Kycaccesstoken)
                .HasMaxLength(255)
                .HasColumnName("kycaccesstoken");
            entity.Property(e => e.Kycprocessed)
                .HasColumnType("datetime")
                .HasColumnName("kycprocessed");
            entity.Property(e => e.Kycprovider)
                .HasMaxLength(255)
                .HasComment("Yoti or IAMX")
                .HasColumnName("kycprovider");
            entity.Property(e => e.Kycresultmessage).HasColumnName("kycresultmessage");
            entity.Property(e => e.Kycstatus)
                .HasMaxLength(255)
                .HasColumnName("kycstatus");
            entity.Property(e => e.Lamports).HasColumnName("lamports");
            entity.Property(e => e.Lastcheckforutxo)
                .HasColumnType("datetime")
                .HasColumnName("lastcheckforutxo");
            entity.Property(e => e.Lastname)
                .HasMaxLength(255)
                .HasColumnName("lastname");
            entity.Property(e => e.Lasttxhash)
                .HasMaxLength(255)
                .HasColumnName("lasttxhash");
            entity.Property(e => e.Lockeduntil)
                .HasColumnType("datetime")
                .HasColumnName("lockeduntil");
            entity.Property(e => e.Lovelace).HasColumnName("lovelace");
            entity.Property(e => e.MarketplacesettingsId)
                .HasDefaultValueSql("'1'")
                .HasColumnName("marketplacesettings_id");
            entity.Property(e => e.Mobilenumber)
                .HasMaxLength(255)
                .HasColumnName("mobilenumber");
            entity.Property(e => e.Newpurchasedmints)
                .HasComment("New Mint Coupons with comma. So we can have also 0,5 mint coupons for an update")
                .HasColumnType("float(12,2)")
                .HasColumnName("newpurchasedmints");
            entity.Property(e => e.Octas).HasColumnName("octas");
            entity.Property(e => e.Password)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Pendingpassword)
                .HasMaxLength(255)
                .HasColumnName("pendingpassword");
            entity.Property(e => e.Pendingpasswordcreated)
                .HasColumnType("datetime")
                .HasColumnName("pendingpasswordcreated");
            entity.Property(e => e.Privateskey).HasColumnName("privateskey");
            entity.Property(e => e.Privatevkey).HasColumnName("privatevkey");
            entity.Property(e => e.Purchasedmints)
                .HasComment("Will not used anymore - use newpurchasedmints now")
                .HasColumnName("purchasedmints");
            entity.Property(e => e.Referal)
                .HasMaxLength(255)
                .HasColumnName("referal");
            entity.Property(e => e.Salt)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("salt");
            entity.Property(e => e.Sendmailonlogon).HasColumnName("sendmailonlogon");
            entity.Property(e => e.Sendmailonlogonfailure).HasColumnName("sendmailonlogonfailure");
            entity.Property(e => e.Sendmailonnews).HasColumnName("sendmailonnews");
            entity.Property(e => e.Sendmailonpayout).HasColumnName("sendmailonpayout");
            entity.Property(e => e.Sendmailonsale).HasColumnName("sendmailonsale");
            entity.Property(e => e.Sendmailonservice).HasColumnName("sendmailonservice");
            entity.Property(e => e.Showkycstate)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("showkycstate");
            entity.Property(e => e.Showpayoutbutton).HasColumnName("showpayoutbutton");
            entity.Property(e => e.Soladdressblocked).HasColumnName("soladdressblocked");
            entity.Property(e => e.Solanapublickey)
                .HasMaxLength(255)
                .HasColumnName("solanapublickey");
            entity.Property(e => e.Solanaseedphrase)
                .HasColumnType("text")
                .HasColumnName("solanaseedphrase");
            entity.Property(e => e.Sollastcheckforutxo)
                .HasColumnType("datetime")
                .HasColumnName("sollastcheckforutxo");
            entity.Property(e => e.Splitroyaltyaddressespercentage)
                .HasDefaultValueSql("'200'")
                .HasComment("200 = 2 percent")
                .HasColumnName("splitroyaltyaddressespercentage");
            entity.Property(e => e.Stakeskey)
                .HasColumnType("text")
                .HasColumnName("stakeskey");
            entity.Property(e => e.Stakevkey)
                .HasColumnType("text")
                .HasColumnName("stakevkey");
            entity.Property(e => e.State)
                .IsRequired()
                .HasColumnType("enum('active','notactive','blocked','locked','deleted')")
                .HasColumnName("state");
            entity.Property(e => e.Street)
                .HasMaxLength(255)
                .HasColumnName("street");
            entity.Property(e => e.SubcustomerId).HasColumnName("subcustomer_id");
            entity.Property(e => e.Subcustomerdescription)
                .HasMaxLength(255)
                .HasColumnName("subcustomerdescription");
            entity.Property(e => e.Subcustomerexternalid)
                .HasMaxLength(255)
                .HasColumnName("subcustomerexternalid");
            entity.Property(e => e.Two2facreateapikey)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("two2facreateapikey");
            entity.Property(e => e.Two2facreatewallet)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("two2facreatewallet");
            entity.Property(e => e.Two2fadeleteprojects)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("two2fadeleteprojects");
            entity.Property(e => e.Two2faexportkeys)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("two2faexportkeys");
            entity.Property(e => e.Two2falogin)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("two2falogin");
            entity.Property(e => e.Two2fapaymentsmanagedwallets)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("two2fapaymentsmanagedwallets");
            entity.Property(e => e.Twofactor)
                .IsRequired()
                .HasColumnType("enum('none','sms','google')")
                .HasColumnName("twofactor");
            entity.Property(e => e.Twofactorenabled)
                .HasColumnType("datetime")
                .HasColumnName("twofactorenabled");
            entity.Property(e => e.Ustid)
                .HasMaxLength(255)
                .HasColumnName("ustid");
            entity.Property(e => e.Zip)
                .HasMaxLength(8)
                .HasColumnName("zip");

            entity.HasOne(d => d.Country).WithMany(p => p.Customers)
                .HasForeignKey(d => d.CountryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("customers_countries");

            entity.HasOne(d => d.Defaultpromotion).WithMany(p => p.Customers)
                .HasForeignKey(d => d.DefaultpromotionId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("customers_promotions");

            entity.HasOne(d => d.Defaultsettings).WithMany(p => p.Customers)
                .HasForeignKey(d => d.DefaultsettingsId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("customers_settings");

            entity.HasOne(d => d.Marketplacesettings).WithMany(p => p.Customers)
                .HasForeignKey(d => d.MarketplacesettingsId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("customers_marketplacesettings");

            entity.HasOne(d => d.Subcustomer).WithMany(p => p.InverseSubcustomer)
                .HasForeignKey(d => d.SubcustomerId)
                .HasConstraintName("customers_customers");
        });

        modelBuilder.Entity<Customeraddress>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("customeraddresses");

            entity.HasIndex(e => e.CustomerId, "customeraddresses_customers");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.Blockchain)
                .HasColumnType("enum('Cardano','Solana')")
                .HasColumnName("blockchain");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Lastchecked)
                .HasColumnType("datetime")
                .HasColumnName("lastchecked");
            entity.Property(e => e.Salt)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("salt");
            entity.Property(e => e.Seedphrase)
                .HasColumnType("text")
                .HasColumnName("seedphrase");
            entity.Property(e => e.Skey)
                .HasColumnType("text")
                .HasColumnName("skey");
            entity.Property(e => e.State)
                .HasColumnType("enum('active','notactive')")
                .HasColumnName("state");
            entity.Property(e => e.Vkey)
                .HasColumnType("text")
                .HasColumnName("vkey");

            entity.HasOne(d => d.Customer).WithMany(p => p.Customeraddresses)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("customeraddresses_customers");
        });

        modelBuilder.Entity<Customerlogin>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("customerlogins")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.CustomerId, "customerlogins_customers");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Ipaddress)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("ipaddress");

            entity.HasOne(d => d.Customer).WithMany(p => p.Customerlogins)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("customerlogins_customers");
        });

        modelBuilder.Entity<Customerwallet>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("customerwallets")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.CustomerId, "customerwallets_customers");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Cointype)
                .IsRequired()
                .HasDefaultValueSql("'ADA'")
                .HasColumnType("enum('ADA','ETH','USDC','SOL','APT','HBAR','BTC')")
                .HasColumnName("cointype");
            entity.Property(e => e.Comment)
                .HasMaxLength(255)
                .HasColumnName("comment");
            entity.Property(e => e.Confirmationcode)
                .HasMaxLength(255)
                .HasColumnName("confirmationcode");
            entity.Property(e => e.Confirmationdate)
                .HasColumnType("datetime")
                .HasColumnName("confirmationdate");
            entity.Property(e => e.Confirmationvalid)
                .HasColumnType("datetime")
                .HasColumnName("confirmationvalid");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Hash)
                .HasMaxLength(255)
                .HasColumnName("hash");
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(255)
                .HasColumnName("ipaddress");
            entity.Property(e => e.State)
                .IsRequired()
                .HasColumnType("enum('active','deleted','notactive')")
                .HasColumnName("state");
            entity.Property(e => e.Walletaddress)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("walletaddress");

            entity.HasOne(d => d.Customer).WithMany(p => p.Customerwallets)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("customerwallets_customers");
        });

        modelBuilder.Entity<Defaulttemplate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("defaulttemplates")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Template)
                .IsRequired()
                .HasColumnName("template");
        });

        modelBuilder.Entity<Digitalidentity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("digitalidentities");

            entity.HasIndex(e => e.Policyid, "digitalidentities_policyid");

            entity.HasIndex(e => e.NftprojectId, "digitalidentities_projects");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.Didjsonresult)
                .HasColumnType("text")
                .HasColumnName("didjsonresult");
            entity.Property(e => e.Didprovider)
                .IsRequired()
                .HasColumnType("enum('NMKR','IAMX')")
                .HasColumnName("didprovider");
            entity.Property(e => e.Didresultreceived)
                .HasColumnType("datetime")
                .HasColumnName("didresultreceived");
            entity.Property(e => e.Ipfshash)
                .HasMaxLength(255)
                .HasColumnName("ipfshash");
            entity.Property(e => e.NftprojectId).HasColumnName("nftproject_id");
            entity.Property(e => e.Policyid)
                .IsRequired()
                .HasColumnName("policyid");
            entity.Property(e => e.Resultmessage)
                .HasColumnType("text")
                .HasColumnName("resultmessage");
            entity.Property(e => e.State)
                .IsRequired()
                .HasColumnType("enum('active','notactive','expired','tokencreated','didresultreceived','canceled','error')")
                .HasColumnName("state");
            entity.Property(e => e.Tokenjson)
                .HasColumnType("text")
                .HasColumnName("tokenjson");

            entity.HasOne(d => d.Nftproject).WithMany(p => p.Digitalidentities)
                .HasForeignKey(d => d.NftprojectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("digitalidentities_projects");
        });

        modelBuilder.Entity<Directsale>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("directsales")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.CustomerId, "directsales_customers");

            entity.HasIndex(e => e.NftprojectId, "directsales_nftprojects");

            entity.HasIndex(e => e.SmartcontractId, "directsales_smartcontracts");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Buyer)
                .HasMaxLength(255)
                .HasColumnName("buyer");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Lockamount)
                .HasDefaultValueSql("'2000000'")
                .HasColumnName("lockamount");
            entity.Property(e => e.Locknftstxinhashid)
                .HasMaxLength(255)
                .HasColumnName("locknftstxinhashid");
            entity.Property(e => e.Marketplacefeepercent)
                .HasColumnType("float(12,2)")
                .HasColumnName("marketplacefeepercent");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.NftprojectId).HasColumnName("nftproject_id");
            entity.Property(e => e.Nmkrfeepercent)
                .HasColumnType("float(12,2)")
                .HasColumnName("nmkrfeepercent");
            entity.Property(e => e.Nmkrpaylink)
                .HasMaxLength(255)
                .HasColumnName("nmkrpaylink");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.Refererfeepercent)
                .HasColumnType("float(12,2)")
                .HasColumnName("refererfeepercent");
            entity.Property(e => e.Royaltyaddress)
                .HasMaxLength(255)
                .HasColumnName("royaltyaddress");
            entity.Property(e => e.Royaltyfeespercent)
                .HasColumnType("float(12,2)")
                .HasColumnName("royaltyfeespercent");
            entity.Property(e => e.Selleraddress)
                .HasMaxLength(255)
                .HasColumnName("selleraddress");
            entity.Property(e => e.SmartcontractId).HasColumnName("smartcontract_id");
            entity.Property(e => e.Solddate)
                .HasColumnType("datetime")
                .HasColumnName("solddate");
            entity.Property(e => e.State)
                .IsRequired()
                .HasColumnType("enum('deleted','prepared','waitingforbid','sold','canceled','readytosignbyseller','readytosignbybuyer','auctionexpired','waitingforsale','waitingforlocknft','submitted','confirmed','readytosignbysellercancel')")
                .HasColumnName("state");
            entity.Property(e => e.Transactionid)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("transactionid");

            entity.HasOne(d => d.Customer).WithMany(p => p.Directsales)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("directsales_ibfk_1");

            entity.HasOne(d => d.Nftproject).WithMany(p => p.Directsales)
                .HasForeignKey(d => d.NftprojectId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("directsales_ibfk_2");

            entity.HasOne(d => d.Smartcontract).WithMany(p => p.Directsales)
                .HasForeignKey(d => d.SmartcontractId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("directsales_ibfk_3");
        });

        modelBuilder.Entity<DirectsalesNft>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("directsales_nfts")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.DirectsaleId, "directsales_nfts_directsales");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DirectsaleId).HasColumnName("directsale_id");
            entity.Property(e => e.Ipfshash)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("ipfshash");
            entity.Property(e => e.Metadata)
                .HasColumnType("text")
                .HasColumnName("metadata");
            entity.Property(e => e.Policyid)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("policyid");
            entity.Property(e => e.Tokencount).HasColumnName("tokencount");
            entity.Property(e => e.Tokennamehex)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("tokennamehex");

            entity.HasOne(d => d.Directsale).WithMany(p => p.DirectsalesNfts)
                .HasForeignKey(d => d.DirectsaleId)
                .HasConstraintName("directsales_nfts_directsales");
        });

        modelBuilder.Entity<Emailtemplate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("emailtemplates")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Emailsubject)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("emailsubject");
            entity.Property(e => e.Htmlemail).HasColumnName("htmlemail");
            entity.Property(e => e.Language)
                .IsRequired()
                .HasMaxLength(2)
                .HasColumnName("language");
            entity.Property(e => e.Templatename)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("templatename");
            entity.Property(e => e.Textemail).HasColumnName("textemail");
        });

        modelBuilder.Entity<Faq>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("faq")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.FaqcategoryId, "faq_faqcategories");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Answer)
                .IsRequired()
                .HasColumnName("answer");
            entity.Property(e => e.FaqcategoryId).HasColumnName("faqcategory_id");
            entity.Property(e => e.Language)
                .IsRequired()
                .HasMaxLength(2)
                .HasColumnName("language");
            entity.Property(e => e.Question)
                .IsRequired()
                .HasColumnName("question");
            entity.Property(e => e.State)
                .IsRequired()
                .HasColumnType("enum('active','notactive')")
                .HasColumnName("state");

            entity.HasOne(d => d.Faqcategory).WithMany(p => p.Faqs)
                .HasForeignKey(d => d.FaqcategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("faq_faqcategories");
        });

        modelBuilder.Entity<Faqcategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("faqcategories")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Categoryname)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("categoryname");
            entity.Property(e => e.Language)
                .IsRequired()
                .HasMaxLength(2)
                .HasColumnName("language");
        });

        modelBuilder.Entity<Getaccesstokensuser>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("getaccesstokensuser")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.CustomerId, "getaccesstokensuser_customers");

            entity.HasIndex(e => e.Secret, "secret").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Friendlyname)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("friendlyname")
                .UseCollation("latin1_swedish_ci")
                .HasCharSet("latin1");
            entity.Property(e => e.Secret)
                .IsRequired()
                .HasColumnName("secret")
                .UseCollation("latin1_swedish_ci")
                .HasCharSet("latin1");
            entity.Property(e => e.State)
                .HasColumnType("enum('active','notactive')")
                .HasColumnName("state")
                .UseCollation("latin1_swedish_ci")
                .HasCharSet("latin1");

            entity.HasOne(d => d.Customer).WithMany(p => p.Getaccesstokensusers)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("getaccesstokensuser_customers");
        });

        modelBuilder.Entity<Getaddressesfordoublepayment>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("getaddressesfordoublepayment");

            entity.Property(e => e.Address)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("address")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Checkfordoublepayment).HasColumnName("checkfordoublepayment");
            entity.Property(e => e.Coin)
                .IsRequired()
                .HasDefaultValueSql("'ADA'")
                .HasColumnType("enum('ADA','SOL','APT','HBAR')")
                .HasColumnName("coin")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Lastcheckforutxo)
                .HasColumnType("datetime")
                .HasColumnName("lastcheckforutxo");
            entity.Property(e => e.Paydate)
                .HasColumnType("datetime")
                .HasColumnName("paydate");
            entity.Property(e => e.State)
                .HasMaxLength(255)
                .HasColumnName("state")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<Getallmetadataplaceholder>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("getallmetadataplaceholder");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255)
                .HasComment("Tokenprefix (from Projects) + Name = Assetname")
                .HasColumnName("name")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Placeholdername)
                .HasMaxLength(255)
                .HasColumnName("placeholdername")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Placeholdervalue)
                .HasColumnName("placeholdervalue")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<Getidsforpolicycheck>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("getidsforpolicycheck");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Mintedonblockchain)
                .HasDefaultValueSql("'Cardano'")
                .HasColumnType("enum('Solana','Cardano','Aptos','Bitcoin')")
                .HasColumnName("mintedonblockchain")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<Getlimit>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("getlimit");

            entity.Property(e => e.Apikey)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("apikey")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Rate).HasColumnName("rate");
        });

        modelBuilder.Entity<Getprojectstatisticsview>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("getprojectstatisticsview");

            entity.Property(e => e.Coin)
                .HasDefaultValueSql("'ADA'")
                .HasColumnType("enum('ADA','SOL','APT','BTC')")
                .HasColumnName("coin")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.NftprojectId).HasColumnName("nftproject_id");
            entity.Property(e => e.Projectname)
                .HasMaxLength(255)
                .HasColumnName("projectname")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Totalfees)
                .HasPrecision(45, 4)
                .HasColumnName("totalfees");
            entity.Property(e => e.Totalfeeseuro)
                .HasColumnType("double(22,8)")
                .HasColumnName("totalfeeseuro");
            entity.Property(e => e.Totalmintingcosts)
                .HasPrecision(45, 4)
                .HasColumnName("totalmintingcosts");
            entity.Property(e => e.Totalmintingcostseuro)
                .HasColumnType("double(22,8)")
                .HasColumnName("totalmintingcostseuro");
            entity.Property(e => e.Totalpayout)
                .HasPrecision(45, 4)
                .HasColumnName("totalpayout");
            entity.Property(e => e.Totalpayouteuro)
                .HasColumnType("double(22,8)")
                .HasColumnName("totalpayouteuro");
            entity.Property(e => e.Totalsendbacktousers)
                .HasPrecision(45, 4)
                .HasColumnName("totalsendbacktousers");
            entity.Property(e => e.Totalsendbacktouserseuro)
                .HasColumnType("double(22,8)")
                .HasColumnName("totalsendbacktouserseuro");
            entity.Property(e => e.Totaltransactions).HasColumnName("totaltransactions");
            entity.Property(e => e.Transactiontype)
                .IsRequired()
                .HasColumnType("enum('paidonftaddress','mintfromcustomeraddress','paidtocustomeraddress','paidfromnftaddress','consolitecustomeraddress','paidfailedtransactiontocustomeraddress','doublepaymentsendbacktobuyer','paidonprojectaddress','fiatpayment','mintfromnftmakeraddress','burning','decentralmintandsend','decentralmintandsale','royaltsplit','unknown','directsale','auction','buymints','refundmints')")
                .HasColumnName("transactiontype")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<Getstatecount>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("getstatecounts");

            entity.Property(e => e.C).HasColumnName("c");
            entity.Property(e => e.NftprojectId).HasColumnName("nftproject_id");
            entity.Property(e => e.State)
                .IsRequired()
                .HasColumnType("enum('free','sold','reserved','deleted','error','signed','burned','blocked')")
                .HasColumnName("state")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Tokenserror)
                .HasPrecision(41)
                .HasColumnName("tokenserror");
            entity.Property(e => e.Tokensreserved)
                .HasPrecision(41)
                .HasColumnName("tokensreserved");
            entity.Property(e => e.Tokenssold)
                .HasPrecision(41)
                .HasColumnName("tokenssold");
            entity.Property(e => e.Total).HasColumnName("total");
        });

        modelBuilder.Entity<Getstatisticsview>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("getstatisticsview");

            entity.Property(e => e.Coin)
                .HasDefaultValueSql("'ADA'")
                .HasColumnType("enum('ADA','SOL','APT','BTC')")
                .HasColumnName("coin")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Countryselect)
                .IsRequired()
                .HasMaxLength(3)
                .HasDefaultValueSql("''")
                .HasColumnName("countryselect");
            entity.Property(e => e.Totalfees)
                .HasPrecision(45, 4)
                .HasColumnName("totalfees");
            entity.Property(e => e.Totalfeeseuro)
                .HasColumnType("double(22,8)")
                .HasColumnName("totalfeeseuro");
            entity.Property(e => e.Totalmintingcosts)
                .HasPrecision(45, 4)
                .HasColumnName("totalmintingcosts");
            entity.Property(e => e.Totalmintingcostseuro)
                .HasColumnType("double(22,8)")
                .HasColumnName("totalmintingcostseuro");
            entity.Property(e => e.Totalnfts).HasColumnName("totalnfts");
            entity.Property(e => e.Totalnmkrcosts)
                .HasPrecision(45, 4)
                .HasColumnName("totalnmkrcosts");
            entity.Property(e => e.Totalnmkrcostseuro)
                .HasColumnType("double(22,8)")
                .HasColumnName("totalnmkrcostseuro");
            entity.Property(e => e.Totalpayout)
                .HasPrecision(45, 4)
                .HasColumnName("totalpayout");
            entity.Property(e => e.Totalpayouteuro)
                .HasColumnType("double(22,8)")
                .HasColumnName("totalpayouteuro");
            entity.Property(e => e.Totalsendbacktousers)
                .HasPrecision(45, 4)
                .HasColumnName("totalsendbacktousers");
            entity.Property(e => e.Totalsendbacktouserseuro)
                .HasColumnType("double(22,8)")
                .HasColumnName("totalsendbacktouserseuro");
            entity.Property(e => e.Totaltransactions).HasColumnName("totaltransactions");
            entity.Property(e => e.Transactiontype)
                .IsRequired()
                .HasColumnType("enum('paidonftaddress','mintfromcustomeraddress','paidtocustomeraddress','paidfromnftaddress','consolitecustomeraddress','paidfailedtransactiontocustomeraddress','doublepaymentsendbacktobuyer','paidonprojectaddress','fiatpayment','mintfromnftmakeraddress','burning','decentralmintandsend','decentralmintandsale','royaltsplit','unknown','directsale','auction','buymints','refundmints')")
                .HasColumnName("transactiontype")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<Gettokensipaddress>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("gettokensipaddresses")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Friendlyname)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("friendlyname")
                .UseCollation("latin1_swedish_ci")
                .HasCharSet("latin1");
            entity.Property(e => e.Ipaddress)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("ipaddress")
                .UseCollation("latin1_swedish_ci")
                .HasCharSet("latin1");
        });

        modelBuilder.Entity<Informationtext>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("informationtexts")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.Informationtext1)
                .IsRequired()
                .HasColumnName("informationtext");
            entity.Property(e => e.State)
                .HasColumnType("enum('active','notactive')")
                .HasColumnName("state");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("invoices")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.CountryId, "invoices_countries");

            entity.HasIndex(e => e.CustomerId, "invoices_customers");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Adarate)
                .HasColumnType("double(20,6)")
                .HasColumnName("adarate");
            entity.Property(e => e.Billingperiod)
                .HasMaxLength(255)
                .HasColumnName("billingperiod");
            entity.Property(e => e.City)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("city");
            entity.Property(e => e.Company)
                .HasMaxLength(255)
                .HasColumnName("company");
            entity.Property(e => e.CountryId).HasColumnName("country_id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Discounteur)
                .HasColumnType("double(20,2)")
                .HasColumnName("discounteur");
            entity.Property(e => e.Discountpercent)
                .HasColumnType("double(12,2)")
                .HasColumnName("discountpercent");
            entity.Property(e => e.Firstname)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("firstname");
            entity.Property(e => e.Grosseur)
                .HasColumnType("double(20,2)")
                .HasColumnName("grosseur");
            entity.Property(e => e.Invoicedate)
                .HasColumnType("datetime")
                .HasColumnName("invoicedate");
            entity.Property(e => e.Invoiceno).HasColumnName("invoiceno");
            entity.Property(e => e.Lastname)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("lastname");
            entity.Property(e => e.Netada)
                .HasColumnType("double(20,6)")
                .HasColumnName("netada");
            entity.Property(e => e.Neteur)
                .HasColumnType("double(20,2)")
                .HasColumnName("neteur");
            entity.Property(e => e.Street)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("street");
            entity.Property(e => e.Taxrate)
                .HasColumnType("double(12,2)")
                .HasColumnName("taxrate");
            entity.Property(e => e.Usteur)
                .HasColumnType("double(20,2)")
                .HasColumnName("usteur");
            entity.Property(e => e.Ustid)
                .HasMaxLength(255)
                .HasColumnName("ustid");
            entity.Property(e => e.Zip)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("zip");

            entity.HasOne(d => d.Country).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.CountryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("invoices_countries");

            entity.HasOne(d => d.Customer).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("invoices_customers");
        });

        modelBuilder.Entity<Invoicedetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("invoicedetails")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.InvoiceId, "invoicedetails");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Averageadarate)
                .HasColumnType("double(20,6)")
                .HasColumnName("averageadarate");
            entity.Property(e => e.Count).HasColumnName("count");
            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.InvoiceId).HasColumnName("invoice_id");
            entity.Property(e => e.Mintcostsada)
                .HasColumnType("double(20,6)")
                .HasColumnName("mintcostsada");
            entity.Property(e => e.Mintcostseur)
                .HasColumnType("double(20,6)")
                .HasColumnName("mintcostseur");
            entity.Property(e => e.Pricesingleada)
                .HasColumnType("double(20,6)")
                .HasColumnName("pricesingleada");
            entity.Property(e => e.Pricesingleeur)
                .HasColumnType("double(20,2)")
                .HasColumnName("pricesingleeur");
            entity.Property(e => e.Pricetotalada)
                .HasColumnType("double(20,6)")
                .HasColumnName("pricetotalada");
            entity.Property(e => e.Pricetotaleur)
                .HasColumnType("double(20,2)")
                .HasColumnName("pricetotaleur");

            entity.HasOne(d => d.Invoice).WithMany(p => p.Invoicedetails)
                .HasForeignKey(d => d.InvoiceId)
                .HasConstraintName("invoicedetails");
        });

        modelBuilder.Entity<Ip2locationDb11>(entity =>
        {
            entity.HasKey(e => new { e.IpFrom, e.IpTo })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity
                .ToTable("ip2location_db11")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_bin");

            entity.HasIndex(e => e.IpFrom, "idx_ip_from");

            entity.HasIndex(e => new { e.IpFrom, e.IpTo }, "idx_ip_from_to");

            entity.HasIndex(e => e.IpTo, "idx_ip_to");

            entity.Property(e => e.IpFrom).HasColumnName("ip_from");
            entity.Property(e => e.IpTo).HasColumnName("ip_to");
            entity.Property(e => e.CityName)
                .HasMaxLength(128)
                .HasColumnName("city_name");
            entity.Property(e => e.CountryCode)
                .HasMaxLength(2)
                .IsFixedLength()
                .HasColumnName("country_code");
            entity.Property(e => e.CountryName)
                .HasMaxLength(64)
                .HasColumnName("country_name");
            entity.Property(e => e.Latitude).HasColumnName("latitude");
            entity.Property(e => e.Longitude).HasColumnName("longitude");
            entity.Property(e => e.RegionName)
                .HasMaxLength(128)
                .HasColumnName("region_name");
            entity.Property(e => e.TimeZone)
                .HasMaxLength(8)
                .HasColumnName("time_zone");
            entity.Property(e => e.ZipCode)
                .HasMaxLength(30)
                .HasColumnName("zip_code");
        });

        modelBuilder.Entity<Ipfsupload>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("ipfsuploads");

            entity.HasIndex(e => e.CustomerId, "ipfsuploads_customers");

            entity.HasIndex(e => new { e.Ipfshash, e.CustomerId }, "ipfsuploads_ipfshash");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Filesize).HasColumnName("filesize");
            entity.Property(e => e.Ipfshash)
                .IsRequired()
                .HasColumnName("ipfshash");
            entity.Property(e => e.Mimetype)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("mimetype");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("name");

            entity.HasOne(d => d.Customer).WithMany(p => p.Ipfsuploads)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("ipfsuploads_customers");
        });

        modelBuilder.Entity<Kycmedium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("kycmedia");

            entity.HasIndex(e => e.CustomerId, "kycmedia_customers");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Base64uri)
                .HasColumnName("base64uri")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Content)
                .HasMaxLength(1)
                .IsFixedLength()
                .HasColumnName("content");
            entity.Property(e => e.Contenttext)
                .HasColumnType("text")
                .HasColumnName("contenttext");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Documenttype)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("documenttype");
            entity.Property(e => e.Mimetype)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("mimetype");

            entity.HasOne(d => d.Customer).WithMany(p => p.Kycmedia)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("kycmedia_customers");
        });

        modelBuilder.Entity<Legacyauction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("legacyauctions")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.CustomerId, "legacyauctions_customers");

            entity.HasIndex(e => e.NftprojectId, "legacyauctions_nftprojects");

            entity.HasIndex(e => e.Uid, "legacyauctions_uid").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Actualbet).HasColumnName("actualbet");
            entity.Property(e => e.Address)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.Auctionname)
                .HasMaxLength(255)
                .HasColumnName("auctionname");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Highestbidder)
                .HasMaxLength(255)
                .HasColumnName("highestbidder");
            entity.Property(e => e.Locknftstxinhashid)
                .HasMaxLength(255)
                .HasColumnName("locknftstxinhashid");
            entity.Property(e => e.Log)
                .HasColumnType("text")
                .HasColumnName("log");
            entity.Property(e => e.Marketplacefeepercent)
                .HasColumnType("float(12,2)")
                .HasColumnName("marketplacefeepercent");
            entity.Property(e => e.Minbet).HasColumnName("minbet");
            entity.Property(e => e.NftprojectId).HasColumnName("nftproject_id");
            entity.Property(e => e.Royaltyaddress)
                .HasMaxLength(255)
                .HasColumnName("royaltyaddress");
            entity.Property(e => e.Royaltyfeespercent)
                .HasColumnType("float(12,2)")
                .HasColumnName("royaltyfeespercent");
            entity.Property(e => e.Runsuntil)
                .HasColumnType("datetime")
                .HasColumnName("runsuntil");
            entity.Property(e => e.Salt)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("salt");
            entity.Property(e => e.Selleraddress)
                .HasMaxLength(255)
                .HasColumnName("selleraddress");
            entity.Property(e => e.Skey)
                .IsRequired()
                .HasColumnType("text")
                .HasColumnName("skey");
            entity.Property(e => e.State)
                .IsRequired()
                .HasComment("Active=Address will monitored, Notactive=Adress not monitored, Finished=Auction finnished, but still monitoring, Ended=Auction finished, not any longer monitoring")
                .HasColumnType("enum('active','notactive','finished','ended','deleted','canceled','waitforlock')")
                .HasColumnName("state");
            entity.Property(e => e.Uid)
                .HasMaxLength(32)
                .HasColumnName("uid");
            entity.Property(e => e.Vkey)
                .IsRequired()
                .HasColumnType("text")
                .HasColumnName("vkey");

            entity.HasOne(d => d.Customer).WithMany(p => p.Legacyauctions)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("legacyauctions_customers");

            entity.HasOne(d => d.Nftproject).WithMany(p => p.Legacyauctions)
                .HasForeignKey(d => d.NftprojectId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("legacyauctions_nftprojects");
        });

        modelBuilder.Entity<LegacyauctionsNft>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("legacyauctions_nfts")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.LegacyauctionId, "legaceauctionsnfts_legacyauctions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Ipfshash)
                .IsRequired()
                .HasMaxLength(255)
                .HasDefaultValueSql("''")
                .HasColumnName("ipfshash");
            entity.Property(e => e.LegacyauctionId).HasColumnName("legacyauction_id");
            entity.Property(e => e.Metadata)
                .HasColumnType("text")
                .HasColumnName("metadata");
            entity.Property(e => e.Policyid)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("policyid");
            entity.Property(e => e.Tokencount).HasColumnName("tokencount");
            entity.Property(e => e.Tokennamehex)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("tokennamehex");

            entity.HasOne(d => d.Legacyauction).WithMany(p => p.LegacyauctionsNfts)
                .HasForeignKey(d => d.LegacyauctionId)
                .HasConstraintName("legaceauctionsnfts_legacyauctions");
        });

        modelBuilder.Entity<Legacyauctionshistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("legacyauctionshistory")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.LegacyauctionId, "legacyauctionshistory_legcyauctions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Bidamount).HasColumnName("bidamount");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.LegacyauctionId).HasColumnName("legacyauction_id");
            entity.Property(e => e.Returntxhash)
                .HasMaxLength(255)
                .HasColumnName("returntxhash");
            entity.Property(e => e.Senderaddress)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("senderaddress");
            entity.Property(e => e.State)
                .IsRequired()
                .HasColumnType("enum('seller','outbid','invalid','expired','buyer')")
                .HasColumnName("state");
            entity.Property(e => e.Txhash)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("txhash");

            entity.HasOne(d => d.Legacyauction).WithMany(p => p.Legacyauctionshistories)
                .HasForeignKey(d => d.LegacyauctionId)
                .HasConstraintName("legacyauctionshistory_legcyauctions");
        });

        modelBuilder.Entity<Legacydirectsale>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("legacydirectsales")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.CustomerId, "legacyauctions_customers");

            entity.HasIndex(e => e.NftprojectId, "legacyauctions_nftprojects");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.Buyer)
                .HasMaxLength(255)
                .HasColumnName("buyer");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Lockamount)
                .HasDefaultValueSql("'2000000'")
                .HasColumnName("lockamount");
            entity.Property(e => e.Locknftstxinhashid)
                .HasMaxLength(255)
                .HasColumnName("locknftstxinhashid");
            entity.Property(e => e.Marketplacefeepercent)
                .HasColumnType("float(12,2)")
                .HasColumnName("marketplacefeepercent");
            entity.Property(e => e.NftprojectId).HasColumnName("nftproject_id");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.Royaltyaddress)
                .HasMaxLength(255)
                .HasColumnName("royaltyaddress");
            entity.Property(e => e.Royaltyfeespercent)
                .HasColumnType("float(12,2)")
                .HasColumnName("royaltyfeespercent");
            entity.Property(e => e.Salt)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("salt");
            entity.Property(e => e.Selleraddress)
                .HasMaxLength(255)
                .HasColumnName("selleraddress");
            entity.Property(e => e.Skey)
                .IsRequired()
                .HasColumnType("text")
                .HasColumnName("skey");
            entity.Property(e => e.Solddate)
                .HasColumnType("datetime")
                .HasColumnName("solddate");
            entity.Property(e => e.State)
                .IsRequired()
                .HasComment("Active=Address will monitored, Notactive=Adress not monitored, Finished=Auction finnished, but still monitoring, Ended=Auction finished, not any longer monitoring")
                .HasColumnType("enum('active','notactive','finished','ended')")
                .HasColumnName("state");
            entity.Property(e => e.Vkey)
                .IsRequired()
                .HasColumnType("text")
                .HasColumnName("vkey");

            entity.HasOne(d => d.Customer).WithMany(p => p.Legacydirectsales)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("legacydirectsales_ibfk_1");

            entity.HasOne(d => d.Nftproject).WithMany(p => p.Legacydirectsales)
                .HasForeignKey(d => d.NftprojectId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("legacydirectsales_ibfk_2");
        });

        modelBuilder.Entity<LegacydirectsalesNft>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("legacydirectsales_nfts")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.LegacydirectsaleId, "legacydirectsales_nfts_legacy_directsales");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Ipfshash)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("ipfshash");
            entity.Property(e => e.LegacydirectsaleId).HasColumnName("legacydirectsale_id");
            entity.Property(e => e.Metadata)
                .HasColumnType("text")
                .HasColumnName("metadata");
            entity.Property(e => e.Policyid)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("policyid");
            entity.Property(e => e.Tokencount).HasColumnName("tokencount");
            entity.Property(e => e.Tokennamehex)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("tokennamehex");

            entity.HasOne(d => d.Legacydirectsale).WithMany(p => p.LegacydirectsalesNfts)
                .HasForeignKey(d => d.LegacydirectsaleId)
                .HasConstraintName("legacydirectsales_nfts_legacy_directsales");
        });

        modelBuilder.Entity<Lockedasset>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("lockedassets");

            entity.HasIndex(e => e.CustomerId, "lockedassets_customers");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Changeaddress)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("changeaddress");
            entity.Property(e => e.Confirmedlock).HasColumnName("confirmedlock");
            entity.Property(e => e.Confirmedunlock).HasColumnName("confirmedunlock");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Lockassetaddress)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("lockassetaddress");
            entity.Property(e => e.Lockeduntil)
                .HasColumnType("datetime")
                .HasColumnName("lockeduntil");
            entity.Property(e => e.Lockslot).HasColumnName("lockslot");
            entity.Property(e => e.Locktxid)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("locktxid");
            entity.Property(e => e.Lockwalletpkh)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("lockwalletpkh");
            entity.Property(e => e.Lovelace).HasColumnName("lovelace");
            entity.Property(e => e.Policyscript)
                .HasColumnType("text")
                .HasColumnName("policyscript");
            entity.Property(e => e.State)
                .IsRequired()
                .HasDefaultValueSql("'active'")
                .HasColumnType("enum('active','deleted')")
                .HasColumnName("state");
            entity.Property(e => e.Unlocked)
                .HasColumnType("datetime")
                .HasColumnName("unlocked");
            entity.Property(e => e.Unlocktxid)
                .HasMaxLength(255)
                .HasColumnName("unlocktxid");
            entity.Property(e => e.Walletname)
                .HasMaxLength(255)
                .HasColumnName("walletname");

            entity.HasOne(d => d.Customer).WithMany(p => p.Lockedassets)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("lockedassets_customers");
        });

        modelBuilder.Entity<Lockedassetstoken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("lockedassetstokens");

            entity.HasIndex(e => e.LockedassetsId, "lockedassetstokens_lockedassets");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Count).HasColumnName("count");
            entity.Property(e => e.LockedassetsId).HasColumnName("lockedassets_id");
            entity.Property(e => e.Policyid)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("policyid");
            entity.Property(e => e.Tokennameinhex)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("tokennameinhex");

            entity.HasOne(d => d.Lockedassets).WithMany(p => p.Lockedassetstokens)
                .HasForeignKey(d => d.LockedassetsId)
                .HasConstraintName("lockedassetstokens_lockedassets");
        });

        modelBuilder.Entity<Log>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("log")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Created)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.Ipaddress)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("ipaddress");
            entity.Property(e => e.Logtext)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("logtext");
        });

        modelBuilder.Entity<Loggedinhash>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("loggedinhashes")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => new { e.Hash, e.Ipaddress }, "loggedinhashes");

            entity.HasIndex(e => e.CustomerId, "loggedinhashes_customers");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Hash)
                .IsRequired()
                .HasColumnName("hash");
            entity.Property(e => e.Ipaddress)
                .IsRequired()
                .HasColumnName("ipaddress");
            entity.Property(e => e.Lastlifesign)
                .HasColumnType("datetime")
                .HasColumnName("lastlifesign");
            entity.Property(e => e.Validuntil)
                .HasColumnType("datetime")
                .HasColumnName("validuntil");

            entity.HasOne(d => d.Customer).WithMany(p => p.Loggedinhashes)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("loggedinhashes_customers");
        });

        modelBuilder.Entity<Manualrefund>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("manualrefunds")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Log).HasColumnName("log");
            entity.Property(e => e.Lovelace).HasColumnName("lovelace");
            entity.Property(e => e.Senderaddress)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("senderaddress");
            entity.Property(e => e.Sendout).HasColumnName("sendout");
            entity.Property(e => e.Transactionid)
                .HasMaxLength(255)
                .HasColumnName("transactionid");
            entity.Property(e => e.Txin)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("txin");
            entity.Property(e => e.Txindate)
                .HasColumnType("datetime")
                .HasColumnName("txindate");
        });

        modelBuilder.Entity<Metadata>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("metadata")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.NftId, "metadata_nft");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.NftId).HasColumnName("nft_id");
            entity.Property(e => e.Placeholdername)
                .HasMaxLength(255)
                .HasColumnName("placeholdername");
            entity.Property(e => e.Placeholdervalue).HasColumnName("placeholdervalue");

            entity.HasOne(d => d.Nft).WithMany(p => p.Metadata)
                .HasForeignKey(d => d.NftId)
                .HasConstraintName("metadata_nft");
        });

        modelBuilder.Entity<Metadatafield>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("metadatafields")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.NftprojectId, "metadatafields_nftprojects");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Metadataname)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("metadataname");
            entity.Property(e => e.Metadatatype)
                .IsRequired()
                .HasColumnType("enum('string','arrayofstring','int','arrayofint','ipfslink','sha256hash','mediatype')")
                .HasColumnName("metadatatype");
            entity.Property(e => e.NftprojectId).HasColumnName("nftproject_id");

            entity.HasOne(d => d.Nftproject).WithMany(p => p.Metadatafields)
                .HasForeignKey(d => d.NftprojectId)
                .HasConstraintName("metadatafields_nftprojects");
        });

        modelBuilder.Entity<Metadatatemplate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("metadatatemplate")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.Logo)
                .HasMaxLength(255)
                .HasColumnName("logo");
            entity.Property(e => e.Metadatatemplate1)
                .IsRequired()
                .HasColumnName("metadatatemplate");
            entity.Property(e => e.Projecttype)
                .IsRequired()
                .HasDefaultValueSql("'nft'")
                .HasColumnType("enum('nft','ft','misc')")
                .HasColumnName("projecttype");
            entity.Property(e => e.State)
                .IsRequired()
                .HasColumnType("enum('active','notactive')")
                .HasColumnName("state");
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("title");
        });

        modelBuilder.Entity<Metadatatemplateadditionalfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("metadatatemplateadditionalfiles");

            entity.HasIndex(e => e.MetadatatemplateId, "metadatatemplateadditionalfiles_metadatatemplates");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Filename)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("filename");
            entity.Property(e => e.Filetypes)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("filetypes");
            entity.Property(e => e.MetadatatemplateId).HasColumnName("metadatatemplate_id");

            entity.HasOne(d => d.Metadatatemplate).WithMany(p => p.Metadatatemplateadditionalfiles)
                .HasForeignKey(d => d.MetadatatemplateId)
                .HasConstraintName("metadatatemplateadditionalfiles_metadatatemplates");
        });

        modelBuilder.Entity<Mimetype>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("mimetypes")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Allowedasmain).HasColumnName("allowedasmain");
            entity.Property(e => e.Fileextensions)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("fileextensions");
            entity.Property(e => e.Mimetype1)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("mimetype");
            entity.Property(e => e.Placeholderfile)
                .HasMaxLength(255)
                .HasColumnName("placeholderfile");
        });

        modelBuilder.Entity<Mintandsend>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("mintandsend")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.NftprojectId, "mintandsend_nftprojects");

            entity.HasIndex(e => new { e.CustomerId, e.State }, "mintandsendstate");

            entity.HasIndex(e => e.Receiveraddress, "receiveraddress");

            entity.HasIndex(e => e.Reservationtoken, "reservationtoken");

            entity.HasIndex(e => e.State, "state");

            entity.HasIndex(e => e.Transactionid, "transactionid");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Buildtransaction).HasColumnName("buildtransaction");
            entity.Property(e => e.Coin)
                .IsRequired()
                .HasDefaultValueSql("'ADA'")
                .HasColumnType("enum('ADA','SOL','APT','HBAR','MATIC','SONY','BTC')")
                .HasColumnName("coin");
            entity.Property(e => e.Confirmed).HasColumnName("confirmed");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Executed)
                .HasColumnType("datetime")
                .HasColumnName("executed");
            entity.Property(e => e.NftprojectId).HasColumnName("nftproject_id");
            entity.Property(e => e.Onlinenotification).HasColumnName("onlinenotification");
            entity.Property(e => e.Receiveraddress)
                .IsRequired()
                .HasColumnName("receiveraddress");
            entity.Property(e => e.Remintandburn).HasColumnName("remintandburn");
            entity.Property(e => e.Requiredcoupons)
                .HasDefaultValueSql("'1'")
                .HasColumnName("requiredcoupons");
            entity.Property(e => e.Reservationtoken)
                .IsRequired()
                .HasColumnName("reservationtoken");
            entity.Property(e => e.Reservelovelace).HasColumnName("reservelovelace");
            entity.Property(e => e.Retry).HasColumnName("retry");
            entity.Property(e => e.State)
                .IsRequired()
                .HasColumnType("enum('execute','success','error','canceled','inprogress')")
                .HasColumnName("state");
            entity.Property(e => e.Transactionid).HasColumnName("transactionid");
            entity.Property(e => e.Usecustomerwallet)
                .HasDefaultValueSql("'1'")
                .HasColumnName("usecustomerwallet");

            entity.HasOne(d => d.Customer).WithMany(p => p.Mintandsends)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("mintandsend_customers");

            entity.HasOne(d => d.Nftproject).WithMany(p => p.Mintandsends)
                .HasForeignKey(d => d.NftprojectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("mintandsend_nftprojects");
        });

        modelBuilder.Entity<Newrate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("newrates");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Coin)
                .IsRequired()
                .HasMaxLength(10)
                .HasColumnName("coin");
            entity.Property(e => e.Currency)
                .IsRequired()
                .HasColumnType("enum('EUR','USD','JPY','BTC')")
                .HasColumnName("currency");
            entity.Property(e => e.Effectivedate)
                .HasColumnType("datetime")
                .HasColumnName("effectivedate");
            entity.Property(e => e.Price)
                .HasColumnType("double(20,10)")
                .HasColumnName("price");
        });

        modelBuilder.Entity<Nft>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("nfts")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.Assetid, "assetid");

            entity.HasIndex(e => e.Checkpolicyid, "checkpolicyid");

            entity.HasIndex(e => new { e.State, e.Fingerprint, e.MainnftId }, "fingerprint");

            entity.HasIndex(e => new { e.Fingerprint, e.Soldcount }, "fingerprint_soldcount");

            entity.HasIndex(e => e.Ipfshash, "ipfs");

            entity.HasIndex(e => e.Name, "name");

            entity.HasIndex(e => e.NftprojectId, "nftprojectid");

            entity.HasIndex(e => new { e.NftprojectId, e.State }, "nftprojectstate");

            entity.HasIndex(e => new { e.NftprojectId, e.State, e.MainnftId }, "nftprojectstate2");

            entity.HasIndex(e => e.NftgroupId, "nfts_nftgroups");

            entity.HasIndex(e => e.MainnftId, "nfts_nfts");

            entity.HasIndex(e => e.InstockpremintedaddressId, "nfts_premintedaddresses");

            entity.HasIndex(e => e.Verifiedcollectionsolana, "nfts_verifiedcollectionsolana");

            entity.HasIndex(e => e.MetadatatemplateId, "ntfs_metadatatemplates");

            entity.HasIndex(e => e.Reservationtoken, "reservationtoken");

            entity.HasIndex(e => e.Uid, "uid").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Assetid)
                .HasComment("Value from Blockfrost")
                .HasColumnName("assetid");
            entity.Property(e => e.Assetname)
                .HasMaxLength(255)
                .HasComment("Value from Blockfrost")
                .HasColumnName("assetname");
            entity.Property(e => e.Buildtransaction).HasColumnName("buildtransaction");
            entity.Property(e => e.Burncount).HasColumnName("burncount");
            entity.Property(e => e.Checkpolicyid)
                .HasComment("When true - The program searches for the policyid/fingerprint on blockforst")
                .HasColumnName("checkpolicyid");
            entity.Property(e => e.Cipversion)
                .HasDefaultValueSql("'unknown'")
                .HasColumnType("enum('unknown','cip20','cip68','cip25')")
                .HasColumnName("cipversion");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.Detaildata).HasColumnName("detaildata");
            entity.Property(e => e.Displayname)
                .HasMaxLength(255)
                .HasColumnName("displayname");
            entity.Property(e => e.Errorcount).HasColumnName("errorcount");
            entity.Property(e => e.Filename)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("filename");
            entity.Property(e => e.Filesize).HasColumnName("filesize");
            entity.Property(e => e.Fingerprint)
                .HasComment("Value from Blockfrost")
                .HasColumnName("fingerprint");
            entity.Property(e => e.Iagonid)
                .HasMaxLength(255)
                .HasColumnName("iagonid");
            entity.Property(e => e.Iagonuploadresult)
                .HasColumnType("text")
                .HasColumnName("iagonuploadresult");
            entity.Property(e => e.Initialminttxhash)
                .HasMaxLength(255)
                .HasComment("Value from Blockfrost")
                .HasColumnName("initialminttxhash");
            entity.Property(e => e.InstockpremintedaddressId)
                .HasComment("When the NFT is already minted and in Stock - here is the ID of the Address where it is")
                .HasColumnName("instockpremintedaddress_id");
            entity.Property(e => e.Ipfshash)
                .IsRequired()
                .HasColumnName("ipfshash");
            entity.Property(e => e.Isroyaltytoken).HasColumnName("isroyaltytoken");
            entity.Property(e => e.Lastpolicycheck)
                .HasColumnType("datetime")
                .HasColumnName("lastpolicycheck");
            entity.Property(e => e.MainnftId)
                .HasComment("If not Null, it is the second (High Resolution Image of the Main Pic) - Used in the Unsig Project")
                .HasColumnName("mainnft_id");
            entity.Property(e => e.Markedaserror)
                .HasColumnType("datetime")
                .HasColumnName("markedaserror");
            entity.Property(e => e.Metadataoverride).HasColumnName("metadataoverride");
            entity.Property(e => e.Metadataoverridecip68).HasColumnName("metadataoverridecip68");
            entity.Property(e => e.MetadatatemplateId).HasColumnName("metadatatemplate_id");
            entity.Property(e => e.Mimetype)
                .HasMaxLength(255)
                .HasDefaultValueSql("'image/png'")
                .HasColumnName("mimetype");
            entity.Property(e => e.Minted)
                .HasComment("Shows, if the NFT is already minted")
                .HasColumnName("minted");
            entity.Property(e => e.Mintedonblockchain)
                .HasDefaultValueSql("'Cardano'")
                .HasColumnType("enum('Solana','Cardano','Aptos','Bitcoin')")
                .HasColumnName("mintedonblockchain");
            entity.Property(e => e.Mintingfees).HasColumnName("mintingfees");
            entity.Property(e => e.Mintingfeespaymentaddress)
                .HasMaxLength(255)
                .HasColumnName("mintingfeespaymentaddress");
            entity.Property(e => e.Mintingfeestransactionid)
                .HasMaxLength(255)
                .HasColumnName("mintingfeestransactionid");
            entity.Property(e => e.Multiplier)
                .HasDefaultValueSql("'1'")
                .HasColumnName("multiplier");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasComment("Tokenprefix (from Projects) + Name = Assetname")
                .HasColumnName("name");
            entity.Property(e => e.NftgroupId).HasColumnName("nftgroup_id");
            entity.Property(e => e.NftprojectId).HasColumnName("nftproject_id");
            entity.Property(e => e.Policyid)
                .HasMaxLength(255)
                .HasComment("The Policy ID should be the same as in the Project - but we load it from Blockfrost to verify")
                .HasColumnName("policyid");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.Priceaptos).HasColumnName("priceaptos");
            entity.Property(e => e.Pricemidnight).HasColumnName("pricemidnight");
            entity.Property(e => e.Pricesolana).HasColumnName("pricesolana");
            entity.Property(e => e.Receiveraddress)
                .HasMaxLength(255)
                .HasColumnName("receiveraddress");
            entity.Property(e => e.Reservationtoken).HasColumnName("reservationtoken");
            entity.Property(e => e.Reservedcount).HasColumnName("reservedcount");
            entity.Property(e => e.Reserveduntil)
                .HasColumnType("datetime")
                .HasColumnName("reserveduntil");
            entity.Property(e => e.Selldate)
                .HasColumnType("datetime")
                .HasColumnName("selldate");
            entity.Property(e => e.Series)
                .HasMaxLength(255)
                .HasComment("Value from Blockfrost")
                .HasColumnName("series");
            entity.Property(e => e.Solanacollectionnft)
                .HasMaxLength(255)
                .HasColumnName("solanacollectionnft");
            entity.Property(e => e.Solanatokenhash)
                .HasMaxLength(255)
                .HasColumnName("solanatokenhash");
            entity.Property(e => e.Soldby)
                .HasColumnType("enum('normal','manual','coupon')")
                .HasColumnName("soldby");
            entity.Property(e => e.Soldcount).HasColumnName("soldcount");
            entity.Property(e => e.State)
                .IsRequired()
                .HasColumnType("enum('free','sold','reserved','deleted','error','signed','burned','blocked')")
                .HasColumnName("state");
            entity.Property(e => e.Testmarker).HasColumnName("testmarker");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasComment("Value from Blockfrost")
                .HasColumnName("title");
            entity.Property(e => e.Transactionid)
                .HasMaxLength(255)
                .HasColumnName("transactionid");
            entity.Property(e => e.Uid)
                .IsRequired()
                .HasMaxLength(40)
                .HasColumnName("uid");
            entity.Property(e => e.Uploadedtonftstorage).HasColumnName("uploadedtonftstorage");
            entity.Property(e => e.Uploadsource)
                .HasMaxLength(255)
                .HasColumnName("uploadsource");
            entity.Property(e => e.Verifiedcollectionsignature)
                .HasMaxLength(255)
                .HasColumnName("verifiedcollectionsignature");
            entity.Property(e => e.Verifiedcollectionsolana)
                .HasColumnType("enum('mustbeadded','success','error','nocollection')")
                .HasColumnName("verifiedcollectionsolana");

            entity.HasOne(d => d.Instockpremintedaddress).WithMany(p => p.Nfts)
                .HasForeignKey(d => d.InstockpremintedaddressId)
                .HasConstraintName("nfts_premintedaddresses");

            entity.HasOne(d => d.Mainnft).WithMany(p => p.InverseMainnft)
                .HasForeignKey(d => d.MainnftId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("nfts_nfts");

            entity.HasOne(d => d.Nftgroup).WithMany(p => p.Nfts)
                .HasForeignKey(d => d.NftgroupId)
                .HasConstraintName("nfts_nftgroups");

            entity.HasOne(d => d.Nftproject).WithMany(p => p.Nfts)
                .HasForeignKey(d => d.NftprojectId)
                .HasConstraintName("nfts_nftprojects");
        });

        modelBuilder.Entity<Nftaddress>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("nftaddresses")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.Created, "created");

            entity.HasIndex(e => e.Lastcheckforutxo, "lastcheckforutxo");

            entity.HasIndex(e => e.Address, "nftaddresses").IsUnique();

            entity.HasIndex(e => new { e.Address, e.NftprojectId }, "nftaddresses2").IsUnique();

            entity.HasIndex(e => e.NftprojectId, "nftaddresses_nftprojects");

            entity.HasIndex(e => e.PreparedpaymenttransactionsId, "nftaddresses_preparedpaymenttransactions");

            entity.HasIndex(e => e.PromotionId, "nftaddresses_promotion");

            entity.HasIndex(e => e.RefererId, "nftaddresses_referer");

            entity.HasIndex(e => new { e.State, e.NftprojectId }, "nftaddressesstate");

            entity.HasIndex(e => new { e.State, e.Serverid }, "nftaddressstate2");

            entity.HasIndex(e => e.Reservationtoken, "reservationtoken").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .IsRequired()
                .HasColumnName("address");
            entity.Property(e => e.Addresscheckedcounter).HasColumnName("addresscheckedcounter");
            entity.Property(e => e.Addresstype)
                .IsRequired()
                .HasDefaultValueSql("'enterprise'")
                .HasColumnType("enum('base','enterprise')")
                .HasColumnName("addresstype");
            entity.Property(e => e.Checkfordoublepayment).HasColumnName("checkfordoublepayment");
            entity.Property(e => e.Checkonlybyserverid).HasColumnName("checkonlybyserverid");
            entity.Property(e => e.Coin)
                .IsRequired()
                .HasDefaultValueSql("'ADA'")
                .HasColumnType("enum('ADA','SOL','APT','HBAR')")
                .HasColumnName("coin");
            entity.Property(e => e.Countnft).HasColumnName("countnft");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.Customproperty)
                .HasMaxLength(255)
                .HasColumnName("customproperty");
            entity.Property(e => e.Discount).HasColumnName("discount");
            entity.Property(e => e.Errormessage)
                .HasMaxLength(255)
                .HasColumnName("errormessage");
            entity.Property(e => e.Expires)
                .HasColumnType("datetime")
                .HasColumnName("expires");
            entity.Property(e => e.Foundinslot).HasColumnName("foundinslot");
            entity.Property(e => e.Freemint).HasColumnName("freemint");
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(40)
                .HasColumnName("ipaddress");
            entity.Property(e => e.Lastcheckforutxo)
                .HasColumnType("datetime")
                .HasColumnName("lastcheckforutxo");
            entity.Property(e => e.Lovelace).HasColumnName("lovelace");
            entity.Property(e => e.Lovelaceamountmustbeexact)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("lovelaceamountmustbeexact");
            entity.Property(e => e.NftprojectId).HasColumnName("nftproject_id");
            entity.Property(e => e.Optionalreceiveraddress)
                .HasMaxLength(255)
                .HasComment("This stores the recevieraddress if specified (but it is optional)")
                .HasColumnName("optionalreceiveraddress");
            entity.Property(e => e.Outgoingtxhash)
                .HasMaxLength(255)
                .HasColumnName("outgoingtxhash");
            entity.Property(e => e.Paydate)
                .HasColumnType("datetime")
                .HasColumnName("paydate");
            entity.Property(e => e.Paymentmethod)
                .IsRequired()
                .HasDefaultValueSql("'ADA'")
                .HasColumnType("enum('ADA','ETH','SOL','FIAT','APT','HBAR')")
                .HasColumnName("paymentmethod");
            entity.Property(e => e.PreparedpaymenttransactionsId).HasColumnName("preparedpaymenttransactions_id");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.Priceintoken).HasColumnName("priceintoken");
            entity.Property(e => e.Privateskey).HasColumnName("privateskey");
            entity.Property(e => e.Privatevkey).HasColumnName("privatevkey");
            entity.Property(e => e.PromotionId).HasColumnName("promotion_id");
            entity.Property(e => e.Promotionmultiplier).HasColumnName("promotionmultiplier");
            entity.Property(e => e.RefererId).HasColumnName("referer_id");
            entity.Property(e => e.Refererstring)
                .HasMaxLength(255)
                .HasColumnName("refererstring");
            entity.Property(e => e.Refundreceiveraddress)
                .HasMaxLength(255)
                .HasColumnName("refundreceiveraddress");
            entity.Property(e => e.Rejectparameter)
                .HasMaxLength(255)
                .HasColumnName("rejectparameter");
            entity.Property(e => e.Rejectreason)
                .HasMaxLength(255)
                .HasColumnName("rejectreason");
            entity.Property(e => e.Reservationtoken).HasColumnName("reservationtoken");
            entity.Property(e => e.Reservationtype)
                .HasColumnType("enum('random','specific')")
                .HasColumnName("reservationtype");
            entity.Property(e => e.Salt)
                .HasMaxLength(255)
                .HasColumnName("salt");
            entity.Property(e => e.Seedphrase)
                .HasColumnType("text")
                .HasColumnName("seedphrase");
            entity.Property(e => e.Sendbacktouser).HasColumnName("sendbacktouser");
            entity.Property(e => e.Senderaddress)
                .HasMaxLength(255)
                .HasColumnName("senderaddress");
            entity.Property(e => e.Serverid).HasColumnName("serverid");
            entity.Property(e => e.Stakereward).HasColumnName("stakereward");
            entity.Property(e => e.Stakeskey).HasColumnName("stakeskey");
            entity.Property(e => e.Stakevkey).HasColumnName("stakevkey");
            entity.Property(e => e.State).HasColumnName("state");
            entity.Property(e => e.Submissionresult).HasColumnName("submissionresult");
            entity.Property(e => e.Tokenassetid)
                .HasMaxLength(255)
                .HasColumnName("tokenassetid");
            entity.Property(e => e.Tokencount)
                .HasDefaultValueSql("'1'")
                .HasComment("not used at the moment")
                .HasColumnName("tokencount");
            entity.Property(e => e.Tokenmultiplier)
                .HasDefaultValueSql("'1'")
                .HasColumnName("tokenmultiplier");
            entity.Property(e => e.Tokenpolicyid)
                .HasMaxLength(255)
                .HasColumnName("tokenpolicyid");
            entity.Property(e => e.Tokenreward).HasColumnName("tokenreward");
            entity.Property(e => e.Txid)
                .HasMaxLength(255)
                .HasColumnName("txid");
            entity.Property(e => e.Utxo).HasColumnName("utxo");

            entity.HasOne(d => d.Nftproject).WithMany(p => p.Nftaddresses)
                .HasForeignKey(d => d.NftprojectId)
                .HasConstraintName("nftaddresses_nftprojects");

            entity.HasOne(d => d.Preparedpaymenttransactions).WithMany(p => p.Nftaddresses)
                .HasForeignKey(d => d.PreparedpaymenttransactionsId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("nftaddresses_preparedpaymenttransactions");

            entity.HasOne(d => d.Promotion).WithMany(p => p.Nftaddresses)
                .HasForeignKey(d => d.PromotionId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("nftaddresses_promotion");

            entity.HasOne(d => d.Referer).WithMany(p => p.Nftaddresses)
                .HasForeignKey(d => d.RefererId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("nftaddresses_referer");
        });

        modelBuilder.Entity<Nftgroup>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("nftgroups")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.NftprojectId, "groups_nftprojects");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Groupname)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("groupname")
                .UseCollation("latin1_swedish_ci")
                .HasCharSet("latin1");
            entity.Property(e => e.NftprojectId).HasColumnName("nftproject_id");
            entity.Property(e => e.Tokensreserved1).HasColumnName("tokensreserved1");
            entity.Property(e => e.Tokenssold1).HasColumnName("tokenssold1");
            entity.Property(e => e.Totaltokens1).HasColumnName("totaltokens1");

            entity.HasOne(d => d.Nftproject).WithMany(p => p.Nftgroups)
                .HasForeignKey(d => d.NftprojectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("groups_nftprojects");
        });

        modelBuilder.Entity<Nftproject>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("nftprojects")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => new { e.CustomerId, e.State }, "customerstate");

            entity.HasIndex(e => e.AptoscustomerwalletId, "nftproject_customerwallets_aptos");

            entity.HasIndex(e => e.CustomerId, "nftprojects_customers");

            entity.HasIndex(e => e.CustomerwalletId, "nftprojects_customerwallets");

            entity.HasIndex(e => e.BitcoincustomerwalletId, "nftprojects_customerwallets_bitcoin");

            entity.HasIndex(e => e.SolanacustomerwalletId, "nftprojects_customerwallets_solana");

            entity.HasIndex(e => e.DefaultpromotionId, "nftprojects_promotions");

            entity.HasIndex(e => e.SettingsId, "nftprojects_settings");

            entity.HasIndex(e => e.Cip68smartcontractId, "nftprojects_smartcontract");

            entity.HasIndex(e => e.SmartcontractssettingsId, "nftprojects_smartcontractssettings");

            entity.HasIndex(e => e.Solanacollectiontransaction, "nftprojects_solanacollectiontransaction");

            entity.HasIndex(e => e.UsdcwalletId, "nftprojects_usdcwallet");

            entity.HasIndex(e => e.Policyid, "policyid");

            entity.HasIndex(e => e.Uid, "uid").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Activatepayinaddress).HasColumnName("activatepayinaddress");
            entity.Property(e => e.Addrefereramounttopaymenttransactions)
                .HasColumnType("double(12,2)")
                .HasColumnName("addrefereramounttopaymenttransactions");
            entity.Property(e => e.Alternativeaddress)
                .HasMaxLength(255)
                .HasComment("This address wil be used, if the policyaddress is already used by an other project with the same policy id")
                .HasColumnName("alternativeaddress");
            entity.Property(e => e.Alternativepayskey).HasColumnName("alternativepayskey");
            entity.Property(e => e.Alternativepayvkey).HasColumnName("alternativepayvkey");
            entity.Property(e => e.Aptosaddress)
                .HasMaxLength(255)
                .HasColumnName("aptosaddress");
            entity.Property(e => e.Aptoscollectioncreated)
                .HasColumnType("datetime")
                .HasColumnName("aptoscollectioncreated");
            entity.Property(e => e.Aptoscollectionimage)
                .HasMaxLength(255)
                .HasColumnName("aptoscollectionimage");
            entity.Property(e => e.Aptoscollectionimagemimetype)
                .HasMaxLength(255)
                .HasColumnName("aptoscollectionimagemimetype");
            entity.Property(e => e.Aptoscollectionname)
                .HasMaxLength(255)
                .HasColumnName("aptoscollectionname");
            entity.Property(e => e.Aptoscollectiontransaction)
                .HasMaxLength(255)
                .HasColumnName("aptoscollectiontransaction");
            entity.Property(e => e.AptoscustomerwalletId).HasColumnName("aptoscustomerwallet_id");
            entity.Property(e => e.Aptospublickey)
                .HasMaxLength(255)
                .HasColumnName("aptospublickey");
            entity.Property(e => e.Aptosseedphrase)
                .HasColumnType("text")
                .HasColumnName("aptosseedphrase");
            entity.Property(e => e.Bitcoinaddress)
                .HasMaxLength(255)
                .HasColumnName("bitcoinaddress");
            entity.Property(e => e.BitcoincustomerwalletId).HasColumnName("bitcoincustomerwallet_id");
            entity.Property(e => e.Bitcoinprivatekey)
                .HasMaxLength(255)
                .HasColumnName("bitcoinprivatekey");
            entity.Property(e => e.Bitcoinpublickey)
                .HasMaxLength(255)
                .HasColumnName("bitcoinpublickey");
            entity.Property(e => e.Bitcoinseedphrase)
                .HasMaxLength(255)
                .HasColumnName("bitcoinseedphrase");
            entity.Property(e => e.Blocked1)
                .HasDefaultValueSql("'0'")
                .HasColumnName("blocked1");
            entity.Property(e => e.Checkcsv).HasColumnName("checkcsv");
            entity.Property(e => e.Checkfiat).HasColumnName("checkfiat");
            entity.Property(e => e.Cip68).HasColumnName("cip68");
            entity.Property(e => e.Cip68extrafield).HasColumnName("cip68extrafield");
            entity.Property(e => e.Cip68referenceaddress)
                .HasMaxLength(255)
                .HasColumnName("cip68referenceaddress");
            entity.Property(e => e.Cip68smartcontractId).HasColumnName("cip68smartcontract_id");
            entity.Property(e => e.Countprices1).HasColumnName("countprices1");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.Crossmintcollectionid)
                .HasMaxLength(255)
                .HasColumnName("crossmintcollectionid");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.CustomerwalletId).HasColumnName("customerwallet_id");
            entity.Property(e => e.DefaultpromotionId).HasColumnName("defaultpromotion_id");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Disablemanualmintingbutton).HasColumnName("disablemanualmintingbutton");
            entity.Property(e => e.Disablerandomsales).HasColumnName("disablerandomsales");
            entity.Property(e => e.Disablespecificsales).HasColumnName("disablespecificsales");
            entity.Property(e => e.Discordurl)
                .HasMaxLength(255)
                .HasColumnName("discordurl");
            entity.Property(e => e.Donotarchive).HasColumnName("donotarchive");
            entity.Property(e => e.Donotdisablepayinaddressautomatically).HasColumnName("donotdisablepayinaddressautomatically");
            entity.Property(e => e.Enablecardano)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasComment("obsolete")
                .HasColumnName("enablecardano");
            entity.Property(e => e.Enablecrosssaleonpaywindow)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasComment("This field indicates, if we enable the cross sale feature on the paywindow (eg the NMKR Token)")
                .HasColumnName("enablecrosssaleonpaywindow");
            entity.Property(e => e.Enabledcoins)
                .HasMaxLength(255)
                .HasDefaultValueSql("'ADA'")
                .HasComment("New field for all Blockchains as List of the Coins (eg: SOL APT ADA)")
                .HasColumnName("enabledcoins");
            entity.Property(e => e.Enabledecentralpayments).HasColumnName("enabledecentralpayments");
            entity.Property(e => e.Enablefiat).HasColumnName("enablefiat");
            entity.Property(e => e.Enablesolana)
                .HasComment("obsolete")
                .HasColumnName("enablesolana");
            entity.Property(e => e.Error1).HasColumnName("error1");
            entity.Property(e => e.Expiretime)
                .HasDefaultValueSql("'20'")
                .HasColumnName("expiretime");
            entity.Property(e => e.Free1).HasColumnName("free1");
            entity.Property(e => e.Hasidentity).HasColumnName("hasidentity");
            entity.Property(e => e.Hasroyality).HasColumnName("hasroyality");
            entity.Property(e => e.IntegratecardanopolicyIdinmetadata).HasColumnName("integratecardanopolicyIdinmetadata");
            entity.Property(e => e.Integratesolanacollectionaddressinmetadata).HasColumnName("integratesolanacollectionaddressinmetadata");
            entity.Property(e => e.Isarchived).HasColumnName("isarchived");
            entity.Property(e => e.Lastcheckforutxo)
                .HasColumnType("datetime")
                .HasColumnName("lastcheckforutxo");
            entity.Property(e => e.Lastinputonaddress)
                .HasColumnType("datetime")
                .HasColumnName("lastinputonaddress");
            entity.Property(e => e.Lastupdate)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("lastupdate");
            entity.Property(e => e.Lockslot)
                .HasComment("When will the policy expire - the slot")
                .HasColumnName("lockslot");
            entity.Property(e => e.Marketplacewhitelabelfee)
                .HasColumnType("float(12,2)")
                .HasColumnName("marketplacewhitelabelfee");
            entity.Property(e => e.Maxcountmintandsend)
                .HasDefaultValueSql("'15'")
                .HasColumnName("maxcountmintandsend");
            entity.Property(e => e.Maxsupply)
                .HasDefaultValueSql("'1'")
                .HasColumnName("maxsupply");
            entity.Property(e => e.Metadata).HasColumnName("metadata");
            entity.Property(e => e.Metadatatemplatename)
                .HasMaxLength(255)
                .HasColumnName("metadatatemplatename");
            entity.Property(e => e.Mintandsendminutxo)
                .IsRequired()
                .HasDefaultValueSql("'minutxo'")
                .HasColumnType("enum('twoadaeverynft','minutxo')")
                .HasColumnName("mintandsendminutxo");
            entity.Property(e => e.Minutxo)
                .HasColumnType("enum('twoadaall5nft','twoadaeverynft','minutxo')")
                .HasColumnName("minutxo");
            entity.Property(e => e.Multiplier)
                .HasDefaultValueSql("'1'")
                .HasColumnName("multiplier");
            entity.Property(e => e.Nftsblocked).HasColumnName("nftsblocked");
            entity.Property(e => e.Nmkraccountoptions)
                .IsRequired()
                .HasDefaultValueSql("'none'")
                .HasColumnType("enum('none','accountnecessary','accountandkycnecessary')")
                .HasColumnName("nmkraccountoptions");
            entity.Property(e => e.Oldmetadatascheme)
                .HasDefaultValueSql("'0'")
                .HasColumnName("oldmetadatascheme");
            entity.Property(e => e.Password)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Paymentgatewaysalestart)
                .HasComment("When the PGW starts to be active")
                .HasColumnType("datetime")
                .HasColumnName("paymentgatewaysalestart");
            entity.Property(e => e.Payoutaddress)
                .HasMaxLength(255)
                .HasColumnName("payoutaddress");
            entity.Property(e => e.Placeholdercsv).HasColumnName("placeholdercsv");
            entity.Property(e => e.Policyaddress)
                .HasMaxLength(255)
                .HasComment("This address is the pay in address of the project")
                .HasColumnName("policyaddress");
            entity.Property(e => e.Policyexpire)
                .HasColumnType("datetime")
                .HasColumnName("policyexpire");
            entity.Property(e => e.Policyid).HasColumnName("policyid");
            entity.Property(e => e.Policyscript).HasColumnName("policyscript");
            entity.Property(e => e.Policyskey).HasColumnName("policyskey");
            entity.Property(e => e.Policyvkey).HasColumnName("policyvkey");
            entity.Property(e => e.Projectlogo)
                .HasMaxLength(255)
                .HasColumnName("projectlogo");
            entity.Property(e => e.Projectname)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("projectname");
            entity.Property(e => e.Projecttype)
                .IsRequired()
                .HasDefaultValueSql("'nft-project'")
                .HasColumnType("enum('nft-project','marketplace-whitelabel')")
                .HasColumnName("projecttype");
            entity.Property(e => e.Projecturl)
                .HasMaxLength(255)
                .HasColumnName("projecturl");
            entity.Property(e => e.Publishmintto3rdpartywebsites).HasColumnName("publishmintto3rdpartywebsites");
            entity.Property(e => e.Referenceaddress)
                .HasMaxLength(255)
                .HasColumnName("referenceaddress");
            entity.Property(e => e.Referenceskey).HasColumnName("referenceskey");
            entity.Property(e => e.Referencevkey).HasColumnName("referencevkey");
            entity.Property(e => e.Reserved1).HasColumnName("reserved1");
            entity.Property(e => e.Royalityaddress)
                .HasMaxLength(255)
                .HasColumnName("royalityaddress");
            entity.Property(e => e.Royalitypercent)
                .HasColumnType("float(12,2)")
                .HasColumnName("royalitypercent");
            entity.Property(e => e.Royaltiycreated)
                .HasColumnType("datetime")
                .HasColumnName("royaltiycreated");
            entity.Property(e => e.SellerFeeBasisPoints).HasColumnName("sellerFeeBasisPoints");
            entity.Property(e => e.SettingsId).HasColumnName("settings_id");
            entity.Property(e => e.SmartcontractssettingsId)
                .HasDefaultValueSql("'1'")
                .HasColumnName("smartcontractssettings_id");
            entity.Property(e => e.Solanacollectioncreated)
                .HasColumnType("datetime")
                .HasColumnName("solanacollectioncreated");
            entity.Property(e => e.Solanacollectionfamily)
                .HasMaxLength(255)
                .HasColumnName("solanacollectionfamily");
            entity.Property(e => e.Solanacollectionimage)
                .HasMaxLength(255)
                .HasColumnName("solanacollectionimage");
            entity.Property(e => e.Solanacollectionimagemimetype)
                .HasMaxLength(255)
                .HasColumnName("solanacollectionimagemimetype");
            entity.Property(e => e.Solanacollectiontransaction).HasColumnName("solanacollectiontransaction");
            entity.Property(e => e.SolanacustomerwalletId).HasColumnName("solanacustomerwallet_id");
            entity.Property(e => e.Solanapublickey)
                .HasMaxLength(255)
                .HasColumnName("solanapublickey");
            entity.Property(e => e.Solanaseedphrase)
                .HasColumnType("text")
                .HasColumnName("solanaseedphrase");
            entity.Property(e => e.Solanasymbol)
                .HasMaxLength(255)
                .HasColumnName("solanasymbol");
            entity.Property(e => e.Sold1).HasColumnName("sold1");
            entity.Property(e => e.State)
                .IsRequired()
                .HasColumnType("enum('active','notactive','deleted','finished')")
                .HasColumnName("state");
            entity.Property(e => e.Storage)
                .IsRequired()
                .HasDefaultValueSql("'ipfs'")
                .HasColumnType("enum('ipfs','iagon')")
                .HasColumnName("storage");
            entity.Property(e => e.Tokennameprefix)
                .HasMaxLength(20)
                .HasDefaultValueSql("''")
                .HasColumnName("tokennameprefix");
            entity.Property(e => e.Tokensreserved1).HasColumnName("tokensreserved1");
            entity.Property(e => e.Tokenssold1).HasColumnName("tokenssold1");
            entity.Property(e => e.Total1).HasColumnName("total1");
            entity.Property(e => e.Totaltokens1).HasColumnName("totaltokens1");
            entity.Property(e => e.Twitterhandle)
                .HasMaxLength(255)
                .HasColumnName("twitterhandle");
            entity.Property(e => e.Twitterurl)
                .HasMaxLength(255)
                .HasColumnName("twitterurl");
            entity.Property(e => e.Uid)
                .IsRequired()
                .HasColumnName("uid");
            entity.Property(e => e.UsdcwalletId).HasColumnName("usdcwallet_id");
            entity.Property(e => e.Usedstorage).HasColumnName("usedstorage");
            entity.Property(e => e.Usefrankenprotection).HasColumnName("usefrankenprotection");
            entity.Property(e => e.Version)
                .HasMaxLength(255)
                .HasColumnName("version");

            entity.HasOne(d => d.Aptoscustomerwallet).WithMany(p => p.NftprojectAptoscustomerwallets)
                .HasForeignKey(d => d.AptoscustomerwalletId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("nftproject_customerwallets_aptos");

            entity.HasOne(d => d.Bitcoincustomerwallet).WithMany(p => p.NftprojectBitcoincustomerwallets)
                .HasForeignKey(d => d.BitcoincustomerwalletId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("nftprojects_customerwallets_bitcoin");

            entity.HasOne(d => d.Cip68smartcontract).WithMany(p => p.Nftprojects)
                .HasForeignKey(d => d.Cip68smartcontractId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("nftprojects_smartcontract");

            entity.HasOne(d => d.Customer).WithMany(p => p.Nftprojects)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("nftprojects_customers");

            entity.HasOne(d => d.Customerwallet).WithMany(p => p.NftprojectCustomerwallets)
                .HasForeignKey(d => d.CustomerwalletId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("nftprojects_customerwallets");

            entity.HasOne(d => d.Defaultpromotion).WithMany(p => p.Nftprojects)
                .HasForeignKey(d => d.DefaultpromotionId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("nftprojects_promotions");

            entity.HasOne(d => d.Settings).WithMany(p => p.Nftprojects)
                .HasForeignKey(d => d.SettingsId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("nftprojects_settings");

            entity.HasOne(d => d.Smartcontractssettings).WithMany(p => p.Nftprojects)
                .HasForeignKey(d => d.SmartcontractssettingsId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("nftprojects_smartcontractssettings");

            entity.HasOne(d => d.Solanacustomerwallet).WithMany(p => p.NftprojectSolanacustomerwallets)
                .HasForeignKey(d => d.SolanacustomerwalletId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("nftprojects_customerwallets_solana");

            entity.HasOne(d => d.Usdcwallet).WithMany(p => p.NftprojectUsdcwallets)
                .HasForeignKey(d => d.UsdcwalletId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("nftprojects_usdcwallet");
        });

        modelBuilder.Entity<Nftprojectadaaddress>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("nftprojectadaaddresses")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.NftprojectsId, "nftprojectsadaaddresses_nftprojects");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.Lovelage).HasColumnName("lovelage");
            entity.Property(e => e.NftprojectsId).HasColumnName("nftprojects_id");
            entity.Property(e => e.Privateskey).HasColumnName("privateskey");
            entity.Property(e => e.Privatevkey).HasColumnName("privatevkey");

            entity.HasOne(d => d.Nftprojects).WithMany(p => p.Nftprojectadaaddresses)
                .HasForeignKey(d => d.NftprojectsId)
                .HasConstraintName("nftprojectsadaaddresses_nftprojects");
        });

        modelBuilder.Entity<NftprojectsView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("nftprojects_view");

            entity.Property(e => e.Activatepayinaddress)
                .HasDefaultValueSql("'0'")
                .HasColumnName("activatepayinaddress");
            entity.Property(e => e.Countprices).HasColumnName("countprices");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.CustomerwalletId).HasColumnName("customerwallet_id");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Error).HasColumnName("error");
            entity.Property(e => e.Expiretime)
                .HasDefaultValueSql("'20'")
                .HasColumnName("expiretime");
            entity.Property(e => e.Free).HasColumnName("free");
            entity.Property(e => e.Hasroyality)
                .HasDefaultValueSql("'0'")
                .HasColumnName("hasroyality");
            entity.Property(e => e.Id)
                .HasDefaultValueSql("'0'")
                .HasColumnName("id");
            entity.Property(e => e.Lastupdate)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("lastupdate");
            entity.Property(e => e.Maxsupply)
                .HasDefaultValueSql("'1'")
                .HasColumnName("maxsupply");
            entity.Property(e => e.Metadata)
                .HasColumnName("metadata")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Minutxo)
                .HasColumnType("enum('twoadaall5nft','twoadaeverynft','minutxo')")
                .HasColumnName("minutxo")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Oldmetadatascheme)
                .HasDefaultValueSql("'0'")
                .HasColumnName("oldmetadatascheme");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Payoutaddress)
                .HasMaxLength(255)
                .HasColumnName("payoutaddress")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Policyaddress)
                .HasMaxLength(255)
                .HasComment("This address is the pay in address of the project")
                .HasColumnName("policyaddress")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Policyexpire)
                .HasColumnType("datetime")
                .HasColumnName("policyexpire");
            entity.Property(e => e.Policyid)
                .HasMaxLength(255)
                .HasColumnName("policyid")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Policyscript)
                .HasColumnName("policyscript")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Policyskey)
                .HasColumnName("policyskey")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Policyvkey)
                .HasColumnName("policyvkey")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Projectname)
                .HasMaxLength(255)
                .HasColumnName("projectname")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Projecturl)
                .HasMaxLength(255)
                .HasColumnName("projecturl")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Reserved).HasColumnName("reserved");
            entity.Property(e => e.Royalityaddress)
                .HasMaxLength(255)
                .HasColumnName("royalityaddress")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Royalitypercent)
                .HasDefaultValueSql("'0.00'")
                .HasColumnType("float(12,2)")
                .HasColumnName("royalitypercent");
            entity.Property(e => e.Royaltiycreated)
                .HasColumnType("datetime")
                .HasColumnName("royaltiycreated");
            entity.Property(e => e.SettingsId).HasColumnName("settings_id");
            entity.Property(e => e.Sold).HasColumnName("sold");
            entity.Property(e => e.State)
                .HasColumnType("enum('active','notactive','deleted','finished')")
                .HasColumnName("state")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Tokennameprefix)
                .HasMaxLength(20)
                .HasDefaultValueSql("''")
                .HasColumnName("tokennameprefix")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Tokensreserved)
                .HasPrecision(41)
                .HasColumnName("tokensreserved");
            entity.Property(e => e.Tokenssold)
                .HasPrecision(41)
                .HasColumnName("tokenssold");
            entity.Property(e => e.Total).HasColumnName("total");
            entity.Property(e => e.Totaltokens).HasColumnName("totaltokens");
            entity.Property(e => e.Version)
                .HasMaxLength(255)
                .HasColumnName("version")
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        modelBuilder.Entity<Nftprojectsadditionalpayout>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("nftprojectsadditionalpayouts")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.NftprojectId, "nftprojectsadditionalpayouts_nftprojects");

            entity.HasIndex(e => e.WalletId, "nftprojectsadditionalpayouts_wallets");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Coin)
                .IsRequired()
                .HasDefaultValueSql("'ADA'")
                .HasColumnType("enum('ADA','SOL','APT','ETH','BTC')")
                .HasColumnName("coin");
            entity.Property(e => e.Custompropertycondition)
                .HasMaxLength(255)
                .HasColumnName("custompropertycondition");
            entity.Property(e => e.NftprojectId).HasColumnName("nftproject_id");
            entity.Property(e => e.Valuepercent)
                .HasColumnType("double(12,2)")
                .HasColumnName("valuepercent");
            entity.Property(e => e.Valuetotal).HasColumnName("valuetotal");
            entity.Property(e => e.WalletId).HasColumnName("wallet_id");

            entity.HasOne(d => d.Nftproject).WithMany(p => p.Nftprojectsadditionalpayouts)
                .HasForeignKey(d => d.NftprojectId)
                .HasConstraintName("nftprojectsadditionalpayouts_nftprojects");

            entity.HasOne(d => d.Wallet).WithMany(p => p.Nftprojectsadditionalpayouts)
                .HasForeignKey(d => d.WalletId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("nftprojectsadditionalpayouts_wallets");
        });

        modelBuilder.Entity<Nftprojectsalecondition>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("nftprojectsaleconditions")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.NftprojectId, "nftprojectsaleconditions_nftprojects");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Blacklistedaddresses).HasColumnName("blacklistedaddresses");
            entity.Property(e => e.Blockchain)
                .IsRequired()
                .HasDefaultValueSql("'Cardano'")
                .HasColumnType("enum('Cardano','Solana','Aptos')")
                .HasColumnName("blockchain");
            entity.Property(e => e.Condition)
                .IsRequired()
                .HasColumnType("enum('walletcontainspolicyid','walletdoesnotcontainpolicyid','walletdoescontainmaxpolicyid','walletcontainsminpolicyid','walletmustcontainminofpolicyid','whitlistedaddresses','stakeonpool','blacklistedaddresses','countedwhitelistedaddresses','onlyonesale')")
                .HasColumnName("condition");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Maxvalue).HasColumnName("maxvalue");
            entity.Property(e => e.NftprojectId).HasColumnName("nftproject_id");
            entity.Property(e => e.Onlyonesaleperwhitlistaddress).HasColumnName("onlyonesaleperwhitlistaddress");
            entity.Property(e => e.Operator)
                .IsRequired()
                .HasDefaultValueSql("'AND'")
                .HasColumnType("enum('AND','OR')")
                .HasColumnName("operator");
            entity.Property(e => e.Policyid)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("policyid");
            entity.Property(e => e.Policyid10)
                .HasMaxLength(255)
                .HasColumnName("policyid10");
            entity.Property(e => e.Policyid11)
                .HasMaxLength(255)
                .HasColumnName("policyid11");
            entity.Property(e => e.Policyid12)
                .HasMaxLength(255)
                .HasColumnName("policyid12");
            entity.Property(e => e.Policyid13)
                .HasMaxLength(255)
                .HasColumnName("policyid13");
            entity.Property(e => e.Policyid14)
                .HasMaxLength(255)
                .HasColumnName("policyid14");
            entity.Property(e => e.Policyid15)
                .HasMaxLength(255)
                .HasColumnName("policyid15");
            entity.Property(e => e.Policyid2)
                .HasMaxLength(255)
                .HasColumnName("policyid2");
            entity.Property(e => e.Policyid3)
                .HasMaxLength(255)
                .HasColumnName("policyid3");
            entity.Property(e => e.Policyid4)
                .HasMaxLength(255)
                .HasColumnName("policyid4");
            entity.Property(e => e.Policyid5)
                .HasMaxLength(255)
                .HasColumnName("policyid5");
            entity.Property(e => e.Policyid6)
                .HasMaxLength(255)
                .HasColumnName("policyid6");
            entity.Property(e => e.Policyid7)
                .HasMaxLength(255)
                .HasColumnName("policyid7");
            entity.Property(e => e.Policyid8)
                .HasMaxLength(255)
                .HasColumnName("policyid8");
            entity.Property(e => e.Policyid9)
                .HasMaxLength(255)
                .HasColumnName("policyid9");
            entity.Property(e => e.Policyprojectname)
                .HasMaxLength(255)
                .HasColumnName("policyprojectname");
            entity.Property(e => e.State)
                .IsRequired()
                .HasColumnType("enum('active','notactive')")
                .HasColumnName("state")
                .UseCollation("latin1_swedish_ci")
                .HasCharSet("latin1");
            entity.Property(e => e.Usedwhitelistaddresses).HasColumnName("usedwhitelistaddresses");
            entity.Property(e => e.Whitlistaddresses).HasColumnName("whitlistaddresses");

            entity.HasOne(d => d.Nftproject).WithMany(p => p.Nftprojectsaleconditions)
                .HasForeignKey(d => d.NftprojectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("nftprojectsaleconditions_nftprojects");
        });

        modelBuilder.Entity<Nftprojectsendpremintedtoken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("nftprojectsendpremintedtokens");

            entity.HasIndex(e => e.BlockchainId, "nftprojectsendpremintedtokens_blockchains");

            entity.HasIndex(e => e.NftprojectId, "nftprojectsendpremintedtokens_nftprojects");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BlockchainId).HasColumnName("blockchain_id");
            entity.Property(e => e.Countokenstosend).HasColumnName("countokenstosend");
            entity.Property(e => e.NftprojectId).HasColumnName("nftproject_id");
            entity.Property(e => e.PolicyidOrCollection)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("policyid_or_collection");
            entity.Property(e => e.Sendwithapiaddresses).HasColumnName("sendwithapiaddresses");
            entity.Property(e => e.Sendwithmintandsend).HasColumnName("sendwithmintandsend");
            entity.Property(e => e.Sendwithmultisigtransactions).HasColumnName("sendwithmultisigtransactions");
            entity.Property(e => e.Sendwithpayinaddresses).HasColumnName("sendwithpayinaddresses");
            entity.Property(e => e.State)
                .IsRequired()
                .HasColumnType("enum('active','notactive')")
                .HasColumnName("state");
            entity.Property(e => e.Tokenname)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("tokenname");

            entity.HasOne(d => d.Blockchain).WithMany(p => p.Nftprojectsendpremintedtokens)
                .HasForeignKey(d => d.BlockchainId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("nftprojectsendpremintedtokens_blockchains");

            entity.HasOne(d => d.Nftproject).WithMany(p => p.Nftprojectsendpremintedtokens)
                .HasForeignKey(d => d.NftprojectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("nftprojectsendpremintedtokens_nftprojects");
        });

        modelBuilder.Entity<Nftreservation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("nftreservations")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => new { e.Reservationtoken, e.NftId }, "nftreservations1").IsUnique();

            entity.HasIndex(e => e.NftId, "nftreservations_nfts");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Mintandsendcommand).HasColumnName("mintandsendcommand");
            entity.Property(e => e.Multiplier)
                .HasDefaultValueSql("'1'")
                .HasComment("not used at the moment")
                .HasColumnName("multiplier");
            entity.Property(e => e.NftId).HasColumnName("nft_id");
            entity.Property(e => e.Reservationdate)
                .HasColumnType("datetime")
                .HasColumnName("reservationdate");
            entity.Property(e => e.Reservationtime)
                .HasDefaultValueSql("'60'")
                .HasColumnName("reservationtime");
            entity.Property(e => e.Reservationtoken)
                .IsRequired()
                .HasColumnName("reservationtoken");
            entity.Property(e => e.Serverid).HasColumnName("serverid");
            entity.Property(e => e.Tc).HasColumnName("tc");

            entity.HasOne(d => d.Nft).WithMany(p => p.Nftreservations)
                .HasForeignKey(d => d.NftId)
                .HasConstraintName("nftreservations_nfts");
        });

        modelBuilder.Entity<NftsArchive>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("nfts_archive")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.Assetid, "assetid");

            entity.HasIndex(e => new { e.State, e.Fingerprint, e.MainnftId }, "fingerprint");

            entity.HasIndex(e => new { e.Ipfshash, e.NftprojectId, e.InstockpremintedaddressId }, "ipfs");

            entity.HasIndex(e => e.Name, "name");

            entity.HasIndex(e => e.NftprojectId, "nftprojectid");

            entity.HasIndex(e => new { e.NftprojectId, e.State }, "nftprojectstate");

            entity.HasIndex(e => new { e.NftprojectId, e.State, e.MainnftId }, "nftprojectstate2");

            entity.HasIndex(e => e.NftgroupId, "nfts_nftgroups");

            entity.HasIndex(e => e.MainnftId, "nfts_nfts");

            entity.HasIndex(e => e.InstockpremintedaddressId, "nfts_premintedaddresses");

            entity.HasIndex(e => e.MetadatatemplateId, "ntfs_metadatatemplates");

            entity.HasIndex(e => e.Uid, "uid").IsUnique();

            entity.HasIndex(e => e.Uploadedtonftstorage, "uploaded");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Assetid)
                .HasComment("Value from Blockfrost")
                .HasColumnName("assetid");
            entity.Property(e => e.Assetname)
                .HasMaxLength(255)
                .HasComment("Value from Blockfrost")
                .HasColumnName("assetname");
            entity.Property(e => e.Buildtransaction).HasColumnName("buildtransaction");
            entity.Property(e => e.Burncount).HasColumnName("burncount");
            entity.Property(e => e.Checkpolicyid)
                .HasComment("When true - The program searches for the policyid/fingerprint on blockforst")
                .HasColumnName("checkpolicyid");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.Detaildata).HasColumnName("detaildata");
            entity.Property(e => e.Displayname)
                .HasMaxLength(255)
                .HasColumnName("displayname");
            entity.Property(e => e.Errorcount).HasColumnName("errorcount");
            entity.Property(e => e.Filename)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("filename");
            entity.Property(e => e.Filesize).HasColumnName("filesize");
            entity.Property(e => e.Fingerprint)
                .HasComment("Value from Blockfrost")
                .HasColumnName("fingerprint");
            entity.Property(e => e.Initialminttxhash)
                .HasMaxLength(255)
                .HasComment("Value from Blockfrost")
                .HasColumnName("initialminttxhash");
            entity.Property(e => e.InstockpremintedaddressId)
                .HasComment("When the NFT is already minted and in Stock - here is the ID of the Address where it is")
                .HasColumnName("instockpremintedaddress_id");
            entity.Property(e => e.Ipfshash)
                .IsRequired()
                .HasColumnName("ipfshash");
            entity.Property(e => e.Isroyaltytoken).HasColumnName("isroyaltytoken");
            entity.Property(e => e.Lastpolicycheck)
                .HasColumnType("datetime")
                .HasColumnName("lastpolicycheck");
            entity.Property(e => e.MainnftId)
                .HasComment("If not Null, it is the second (High Resolution Image of the Main Pic) - Used in the Unsig Project")
                .HasColumnName("mainnft_id");
            entity.Property(e => e.Markedaserror)
                .HasColumnType("datetime")
                .HasColumnName("markedaserror");
            entity.Property(e => e.Metadataoverride).HasColumnName("metadataoverride");
            entity.Property(e => e.MetadatatemplateId).HasColumnName("metadatatemplate_id");
            entity.Property(e => e.Mimetype)
                .HasMaxLength(255)
                .HasDefaultValueSql("'image/png'")
                .HasColumnName("mimetype");
            entity.Property(e => e.Minted)
                .HasComment("Shows, if the NFT is already minted")
                .HasColumnName("minted");
            entity.Property(e => e.Mintingfees).HasColumnName("mintingfees");
            entity.Property(e => e.Mintingfeespaymentaddress)
                .HasMaxLength(255)
                .HasColumnName("mintingfeespaymentaddress");
            entity.Property(e => e.Mintingfeestransactionid)
                .HasMaxLength(255)
                .HasColumnName("mintingfeestransactionid");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasComment("Tokenprefix (from Projects) + Name = Assetname")
                .HasColumnName("name");
            entity.Property(e => e.NftgroupId).HasColumnName("nftgroup_id");
            entity.Property(e => e.NftprojectId).HasColumnName("nftproject_id");
            entity.Property(e => e.Policyid)
                .HasMaxLength(255)
                .HasComment("The Policy ID should be the same as in the Project - but we load it from Blockfrost to verify")
                .HasColumnName("policyid");
            entity.Property(e => e.Price).HasColumnName("price");
            entity.Property(e => e.Receiveraddress)
                .HasMaxLength(255)
                .HasColumnName("receiveraddress");
            entity.Property(e => e.Reservedcount).HasColumnName("reservedcount");
            entity.Property(e => e.Reserveduntil)
                .HasColumnType("datetime")
                .HasColumnName("reserveduntil");
            entity.Property(e => e.Selldate)
                .HasColumnType("datetime")
                .HasColumnName("selldate");
            entity.Property(e => e.Series)
                .HasMaxLength(255)
                .HasComment("Value from Blockfrost")
                .HasColumnName("series");
            entity.Property(e => e.Soldby)
                .HasColumnType("enum('normal','manual','coupon')")
                .HasColumnName("soldby");
            entity.Property(e => e.Soldcount).HasColumnName("soldcount");
            entity.Property(e => e.State)
                .IsRequired()
                .HasColumnType("enum('free','sold','reserved','deleted','error','signed','burned')")
                .HasColumnName("state");
            entity.Property(e => e.Testmarker).HasColumnName("testmarker");
            entity.Property(e => e.Title)
                .HasMaxLength(255)
                .HasComment("Value from Blockfrost")
                .HasColumnName("title");
            entity.Property(e => e.Transactionid)
                .HasMaxLength(255)
                .HasColumnName("transactionid");
            entity.Property(e => e.Uid)
                .IsRequired()
                .HasMaxLength(40)
                .HasColumnName("uid");
            entity.Property(e => e.Uploadedtonftstorage).HasColumnName("uploadedtonftstorage");

            entity.HasOne(d => d.Mainnft).WithMany(p => p.NftsArchives)
                .HasForeignKey(d => d.MainnftId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("nfts_archive_ibfk_3");

            entity.HasOne(d => d.Metadatatemplate).WithMany(p => p.NftsArchives)
                .HasForeignKey(d => d.MetadatatemplateId)
                .HasConstraintName("nfts_archive_ibfk_5");

            entity.HasOne(d => d.Nftgroup).WithMany(p => p.NftsArchives)
                .HasForeignKey(d => d.NftgroupId)
                .HasConstraintName("nfts_archive_ibfk_1");

            entity.HasOne(d => d.Nftproject).WithMany(p => p.NftsArchives)
                .HasForeignKey(d => d.NftprojectId)
                .HasConstraintName("nfts_archive_ibfk_2");
        });

        modelBuilder.Entity<Nfttonftaddress>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("nfttonftaddresses")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.NftId, "nftid");

            entity.HasIndex(e => new { e.NftId, e.NftaddressesId }, "nfttoaddresses").IsUnique();

            entity.HasIndex(e => e.NftaddressesId, "nfttonftaddresses_nftaddresses");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.NftId).HasColumnName("nft_id");
            entity.Property(e => e.NftaddressesId).HasColumnName("nftaddresses_id");
            entity.Property(e => e.Tokencount)
                .HasDefaultValueSql("'1'")
                .HasColumnName("tokencount");

            entity.HasOne(d => d.Nft).WithMany(p => p.Nfttonftaddresses)
                .HasForeignKey(d => d.NftId)
                .HasConstraintName("afttoaftaddresses_nfts");

            entity.HasOne(d => d.Nftaddresses).WithMany(p => p.Nfttonftaddresses)
                .HasForeignKey(d => d.NftaddressesId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("nfttonftaddresses_nftaddresses");
        });

        modelBuilder.Entity<Nfttonftaddresseshistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("nfttonftaddresseshistory")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.NftId, "nftid");

            entity.HasIndex(e => new { e.NftId, e.NftaddressesId }, "nfttoaddresses");

            entity.HasIndex(e => e.NftaddressesId, "nfttonftaddresses_nftaddresses");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.NftId).HasColumnName("nft_id");
            entity.Property(e => e.NftaddressesId).HasColumnName("nftaddresses_id");
            entity.Property(e => e.Tokencount)
                .HasDefaultValueSql("'1'")
                .HasColumnName("tokencount");

            entity.HasOne(d => d.Nft).WithMany(p => p.Nfttonftaddresseshistories)
                .HasForeignKey(d => d.NftId)
                .HasConstraintName("afttoaftaddresseshistory_nfts");

            entity.HasOne(d => d.Nftaddresses).WithMany(p => p.Nfttonftaddresseshistories)
                .HasForeignKey(d => d.NftaddressesId)
                .HasConstraintName("nfttonftaddresseshistory_nftaddresses");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("notifications")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.NftprojectId, "notifications_nftprojects");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.NftprojectId).HasColumnName("nftproject_id");
            entity.Property(e => e.Notificationtype)
                .IsRequired()
                .HasColumnType("enum('webhook','email')")
                .HasColumnName("notificationtype");
            entity.Property(e => e.Secret)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("secret");
            entity.Property(e => e.State)
                .IsRequired()
                .HasColumnType("enum('active','notactive','deleted')")
                .HasColumnName("state");

            entity.HasOne(d => d.Nftproject).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.NftprojectId)
                .HasConstraintName("notifications_nftprojects");
        });

        modelBuilder.Entity<Notificationqueue>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("notificationqueue")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.State, "notificationstate");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Counterrors).HasColumnName("counterrors");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.Lasterror)
                .HasColumnType("datetime")
                .HasColumnName("lasterror");
            entity.Property(e => e.Notificationendpoint)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("notificationendpoint");
            entity.Property(e => e.Notificationtype)
                .IsRequired()
                .HasColumnType("enum('email','webhook')")
                .HasColumnName("notificationtype");
            entity.Property(e => e.Payload)
                .IsRequired()
                .HasColumnType("text")
                .HasColumnName("payload");
            entity.Property(e => e.Processed)
                .HasColumnType("datetime")
                .HasColumnName("processed");
            entity.Property(e => e.Result)
                .HasMaxLength(255)
                .HasColumnName("result");
            entity.Property(e => e.ServerId).HasColumnName("server_id");
            entity.Property(e => e.State)
                .IsRequired()
                .HasColumnType("enum('active','processing','successful','error')")
                .HasColumnName("state");
        });

        modelBuilder.Entity<Onlinenotification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("onlinenotifications")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.CustomerId, "onlinenotifications_customers");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Color)
                .HasMaxLength(255)
                .HasColumnName("color");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Notificationmessage)
                .IsRequired()
                .HasColumnName("notificationmessage");
            entity.Property(e => e.State)
                .IsRequired()
                .HasColumnType("enum('new','hasread')")
                .HasColumnName("state");

            entity.HasOne(d => d.Customer).WithMany(p => p.Onlinenotifications)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("onlinenotifications_customers");
        });

        modelBuilder.Entity<Paybuttoncode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("paybuttoncode")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .IsRequired()
                .HasColumnName("code");
            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("description");
        });

        modelBuilder.Entity<Payoutrequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("payoutrequests")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.CustomerId, "payoutrequests_customers");

            entity.HasIndex(e => e.WalletId, "payoutrequests_customerwallets");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Ada).HasColumnName("ada");
            entity.Property(e => e.Confirmationcode)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("confirmationcode");
            entity.Property(e => e.Confirmationexpire)
                .HasColumnType("datetime")
                .HasColumnName("confirmationexpire");
            entity.Property(e => e.Confirmationipaddress)
                .HasMaxLength(255)
                .HasColumnName("confirmationipaddress");
            entity.Property(e => e.Confirmationtime)
                .HasColumnType("datetime")
                .HasColumnName("confirmationtime");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Executiontime)
                .HasColumnType("datetime")
                .HasColumnName("executiontime");
            entity.Property(e => e.Logfile).HasColumnName("logfile");
            entity.Property(e => e.Payoutinitiator)
                .IsRequired()
                .HasColumnType("enum('website','api')")
                .HasColumnName("payoutinitiator");
            entity.Property(e => e.State)
                .IsRequired()
                .HasColumnType("enum('active','expired','executed','execute','error')")
                .HasColumnName("state");
            entity.Property(e => e.Transactionid)
                .HasMaxLength(255)
                .HasColumnName("transactionid");
            entity.Property(e => e.WalletId).HasColumnName("wallet_id");

            entity.HasOne(d => d.Customer).WithMany(p => p.Payoutrequests)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("payoutrequests_customers");

            entity.HasOne(d => d.Wallet).WithMany(p => p.Payoutrequests)
                .HasForeignKey(d => d.WalletId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("payoutrequests_customerwallets");
        });

        modelBuilder.Entity<Plugin>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("plugins");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Buttonlink)
                .HasMaxLength(255)
                .HasColumnName("buttonlink");
            entity.Property(e => e.Buttontext)
                .HasMaxLength(255)
                .HasColumnName("buttontext");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Header)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("header");
            entity.Property(e => e.Image)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("image");
            entity.Property(e => e.State)
                .IsRequired()
                .HasColumnType("enum('active','notactive')")
                .HasColumnName("state");
            entity.Property(e => e.Subtitle)
                .HasMaxLength(255)
                .HasColumnName("subtitle");
        });

        modelBuilder.Entity<Premintednftsaddress>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("premintednftsaddresses")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.NftprojectId, "premintednftsaddresses_nftprojects");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.Expires)
                .HasColumnType("datetime")
                .HasColumnName("expires");
            entity.Property(e => e.Lastcheckforutxo)
                .HasColumnType("datetime")
                .HasColumnName("lastcheckforutxo");
            entity.Property(e => e.Lovelace).HasColumnName("lovelace");
            entity.Property(e => e.NftprojectId).HasColumnName("nftproject_id");
            entity.Property(e => e.Privateskey)
                .IsRequired()
                .HasColumnName("privateskey");
            entity.Property(e => e.Privatevkey)
                .IsRequired()
                .HasColumnName("privatevkey");
            entity.Property(e => e.Salt)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("salt");
            entity.Property(e => e.State)
                .IsRequired()
                .HasColumnType("enum('free','reserved','inuse','send','error')")
                .HasColumnName("state");

            entity.HasOne(d => d.Nftproject).WithMany(p => p.Premintednftsaddresses)
                .HasForeignKey(d => d.NftprojectId)
                .HasConstraintName("premintednftsaddresses_nftprojects");
        });

        modelBuilder.Entity<Premintedpromotokenaddress>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("premintedpromotokenaddresses");

            entity.HasIndex(e => e.BlockchainId, "promotokenaddresses_blockchains");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.BlockchainId).HasColumnName("blockchain_id");
            entity.Property(e => e.Blockdate)
                .HasColumnType("datetime")
                .HasColumnName("blockdate");
            entity.Property(e => e.Lastcheck)
                .HasColumnType("datetime")
                .HasColumnName("lastcheck");
            entity.Property(e => e.Lasttxhash)
                .HasMaxLength(255)
                .HasColumnName("lasttxhash");
            entity.Property(e => e.PolicyidOrCollection)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("policyid_or_collection");
            entity.Property(e => e.Privatekey)
                .HasColumnType("text")
                .HasColumnName("privatekey");
            entity.Property(e => e.Publickey)
                .HasColumnType("text")
                .HasColumnName("publickey");
            entity.Property(e => e.Reservationtoken)
                .HasMaxLength(255)
                .HasColumnName("reservationtoken");
            entity.Property(e => e.Salt)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("salt");
            entity.Property(e => e.Seedphrase)
                .HasColumnType("text")
                .HasColumnName("seedphrase");
            entity.Property(e => e.State)
                .IsRequired()
                .HasColumnType("enum('active','blocked','empty','disabled')")
                .HasColumnName("state");
            entity.Property(e => e.Tokenname)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("tokenname");
            entity.Property(e => e.Totaltokens).HasColumnName("totaltokens");

            entity.HasOne(d => d.Blockchain).WithMany(p => p.Premintedpromotokenaddresses)
                .HasForeignKey(d => d.BlockchainId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("promotokenaddresses_blockchains");
        });

        modelBuilder.Entity<Preparedpaymenttransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("preparedpaymenttransactions")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.BuyoutaddressesId, "preparedpaymenttransactions_buyoutaddresses");

            entity.HasIndex(e => e.LegacyauctionsId, "preparedpaymenttransactions_legacyauctions");

            entity.HasIndex(e => e.LegacydirectsalesId, "preparedpaymenttransactions_legacydirectsales");

            entity.HasIndex(e => e.MintandsendId, "preparedpaymenttransactions_mintandsend");

            entity.HasIndex(e => e.NftaddressesId, "preparedpaymenttransactions_nftaddresses");

            entity.HasIndex(e => e.ReferencedprepearedtransactionId, "preparedpaymenttransactions_preparedpaymenttransactions");

            entity.HasIndex(e => e.NftprojectId, "preparedpaymenttransactions_projects");

            entity.HasIndex(e => e.PromotionId, "preparedpaymenttransactions_promotions");

            entity.HasIndex(e => e.ReservationId, "preparedpaymenttransactions_reservations");

            entity.HasIndex(e => e.SmartcontractsId, "preparedpaymenttransactions_smartcontracts");

            entity.HasIndex(e => e.TransactionId, "preparedpaymenttransactions_transactions");

            entity.HasIndex(e => e.Transactionuid, "preparedpaymenttransactionsuid").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Auctionduration).HasColumnName("auctionduration");
            entity.Property(e => e.Auctionminprice).HasColumnName("auctionminprice");
            entity.Property(e => e.Buyeraddress)
                .HasMaxLength(255)
                .HasColumnName("buyeraddress");
            entity.Property(e => e.Buyeraddresses)
                .HasColumnType("text")
                .HasColumnName("buyeraddresses");
            entity.Property(e => e.Buyerpkh)
                .HasMaxLength(255)
                .HasColumnName("buyerpkh");
            entity.Property(e => e.BuyoutaddressesId).HasColumnName("buyoutaddresses_id");
            entity.Property(e => e.Cachedresultgetpaymentaddress)
                .HasColumnType("text")
                .HasColumnName("cachedresultgetpaymentaddress");
            entity.Property(e => e.Cbor)
                .HasColumnType("text")
                .HasColumnName("cbor");
            entity.Property(e => e.Changeaddress)
                .HasMaxLength(255)
                .HasColumnName("changeaddress");
            entity.Property(e => e.Command)
                .HasColumnType("text")
                .HasColumnName("command");
            entity.Property(e => e.Confirmeddate)
                .HasColumnType("datetime")
                .HasColumnName("confirmeddate");
            entity.Property(e => e.Countnft).HasColumnName("countnft");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.Createroyaltytokenaddress)
                .HasMaxLength(255)
                .HasColumnName("createroyaltytokenaddress");
            entity.Property(e => e.Createroyaltytokenpercentage)
                .HasColumnType("float(12,2)")
                .HasColumnName("createroyaltytokenpercentage");
            entity.Property(e => e.Customeripaddress)
                .HasMaxLength(255)
                .HasColumnName("customeripaddress");
            entity.Property(e => e.Discount).HasColumnName("discount");
            entity.Property(e => e.Estimatedfees).HasColumnName("estimatedfees");
            entity.Property(e => e.Expires)
                .HasColumnType("datetime")
                .HasColumnName("expires");
            entity.Property(e => e.Fee).HasColumnName("fee");
            entity.Property(e => e.LegacyauctionsId).HasColumnName("legacyauctions_id");
            entity.Property(e => e.LegacydirectsalesId).HasColumnName("legacydirectsales_id");
            entity.Property(e => e.Lockamount).HasColumnName("lockamount");
            entity.Property(e => e.Logfile).HasColumnName("logfile");
            entity.Property(e => e.Lovelace).HasColumnName("lovelace");
            entity.Property(e => e.MintandsendId).HasColumnName("mintandsend_id");
            entity.Property(e => e.NftaddressesId).HasColumnName("nftaddresses_id");
            entity.Property(e => e.NftprojectId).HasColumnName("nftproject_id");
            entity.Property(e => e.Optionalreceiveraddress)
                .HasMaxLength(255)
                .HasColumnName("optionalreceiveraddress");
            entity.Property(e => e.Overridemarketplaceaddress)
                .HasMaxLength(255)
                .HasColumnName("overridemarketplaceaddress");
            entity.Property(e => e.Overridemarketplacefee)
                .HasColumnType("float(12,2)")
                .HasColumnName("overridemarketplacefee");
            entity.Property(e => e.Paymentgatewaystate)
                .HasColumnType("enum('prepared','sold','canceled','readytosignbybuyer','signedbybuyer','submitted')")
                .HasColumnName("paymentgatewaystate");
            entity.Property(e => e.Policyid)
                .HasMaxLength(255)
                .HasColumnName("policyid");
            entity.Property(e => e.PromotionId).HasColumnName("promotion_id");
            entity.Property(e => e.Promotionmultiplier).HasColumnName("promotionmultiplier");
            entity.Property(e => e.ReferencedprepearedtransactionId).HasColumnName("referencedprepearedtransaction_id");
            entity.Property(e => e.Referer)
                .HasMaxLength(255)
                .HasColumnName("referer");
            entity.Property(e => e.Rejectparameter)
                .HasMaxLength(255)
                .HasColumnName("rejectparameter");
            entity.Property(e => e.Rejectreason)
                .HasMaxLength(255)
                .HasColumnName("rejectreason");
            entity.Property(e => e.ReservationId).HasColumnName("reservation_id");
            entity.Property(e => e.Reservationtoken)
                .HasMaxLength(255)
                .HasColumnName("reservationtoken");
            entity.Property(e => e.Selleraddress)
                .HasMaxLength(255)
                .HasColumnName("selleraddress");
            entity.Property(e => e.Selleraddresses)
                .HasColumnType("text")
                .HasColumnName("selleraddresses");
            entity.Property(e => e.Sellerpkh)
                .HasMaxLength(255)
                .HasColumnName("sellerpkh");
            entity.Property(e => e.Signedcbor)
                .HasColumnType("text")
                .HasColumnName("signedcbor");
            entity.Property(e => e.SmartcontractsId).HasColumnName("smartcontracts_id");
            entity.Property(e => e.SmartcontractsmarketplaceId).HasColumnName("smartcontractsmarketplace_id");
            entity.Property(e => e.Smartcontractstate)
                .HasColumnType("enum('prepared','waitingforbid','sold','canceled','readytosignbyseller','readytosignbybuyer','auctionexpired','waitingforsale','waitingforlocknft','submitted','confirmed','readytosignbysellercancel','waitingforlockada','readytosignbybuyercancel')")
                .HasColumnName("smartcontractstate");
            entity.Property(e => e.Stakerewards).HasColumnName("stakerewards");
            entity.Property(e => e.State)
                .HasColumnType("enum('active','expired','finished','prepared','error','canceled','rejected')")
                .HasColumnName("state");
            entity.Property(e => e.Submitteddate)
                .HasColumnType("datetime")
                .HasColumnName("submitteddate");
            entity.Property(e => e.Tokencount).HasColumnName("tokencount");
            entity.Property(e => e.Tokenname)
                .HasMaxLength(255)
                .HasColumnName("tokenname");
            entity.Property(e => e.Tokenrewards).HasColumnName("tokenrewards");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.Transactiontype)
                .IsRequired()
                .HasColumnType("enum('paymentgateway_nft_specific','paymentgateway_nft_random','smartcontract_directsale','smartcontract_auction','legacy_auction','legacy_directsale','decentral_mintandsend_random','decentral_mintandsend_specific','paymentgateway_mintandsend_random','paymentgateway_mintandsend_specific','decentral_mintandsale_random','decentral_mintandsale_specific','nmkr_pay_random','nmkr_pay_specific','smartcontract_directsale_offer','paymentgateway_buyout_smartcontract')")
                .HasColumnName("transactiontype");
            entity.Property(e => e.Transactionuid)
                .IsRequired()
                .HasColumnName("transactionuid");
            entity.Property(e => e.Txhash)
                .HasMaxLength(255)
                .HasColumnName("txhash");
            entity.Property(e => e.Txinforalreadylockedtransactions)
                .HasMaxLength(255)
                .HasColumnName("txinforalreadylockedtransactions");

            entity.HasOne(d => d.Buyoutaddresses).WithMany(p => p.Preparedpaymenttransactions)
                .HasForeignKey(d => d.BuyoutaddressesId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("preparedpaymenttransactions_buyoutaddresses");

            entity.HasOne(d => d.Legacyauctions).WithMany(p => p.Preparedpaymenttransactions)
                .HasForeignKey(d => d.LegacyauctionsId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("preparedpaymenttransactions_legacyauctions");

            entity.HasOne(d => d.Legacydirectsales).WithMany(p => p.Preparedpaymenttransactions)
                .HasForeignKey(d => d.LegacydirectsalesId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("preparedpaymenttransactions_legacydirectsales");

            entity.HasOne(d => d.Mintandsend).WithMany(p => p.Preparedpaymenttransactions)
                .HasForeignKey(d => d.MintandsendId)
                .HasConstraintName("preparedpaymenttransactions_mintandsend");

            entity.HasOne(d => d.NftaddressesNavigation).WithMany(p => p.PreparedpaymenttransactionsNavigation)
                .HasForeignKey(d => d.NftaddressesId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("preparedpaymenttransactions_nftaddresses");

            entity.HasOne(d => d.Nftproject).WithMany(p => p.Preparedpaymenttransactions)
                .HasForeignKey(d => d.NftprojectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("preparedpaymenttransactions_projects");

            entity.HasOne(d => d.Promotion).WithMany(p => p.Preparedpaymenttransactions)
                .HasForeignKey(d => d.PromotionId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("preparedpaymenttransactions_promotions");

            entity.HasOne(d => d.Referencedprepearedtransaction).WithMany(p => p.InverseReferencedprepearedtransaction)
                .HasForeignKey(d => d.ReferencedprepearedtransactionId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("preparedpaymenttransactions_preparedpaymenttransactions");

            entity.HasOne(d => d.Reservation).WithMany(p => p.Preparedpaymenttransactions)
                .HasForeignKey(d => d.ReservationId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("preparedpaymenttransactions_reservations");

            entity.HasOne(d => d.Smartcontracts).WithMany(p => p.Preparedpaymenttransactions)
                .HasForeignKey(d => d.SmartcontractsId)
                .HasConstraintName("preparedpaymenttransactions_smartcontracts");

            entity.HasOne(d => d.Transaction).WithMany(p => p.Preparedpaymenttransactions)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("preparedpaymenttransactions_transactions");
        });

        modelBuilder.Entity<PreparedpaymenttransactionsCustomproperty>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("preparedpaymenttransactions_customproperties")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.PreparedpaymenttransactionsId, "preparedpaymenttransactions_customproperties");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Key)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("key");
            entity.Property(e => e.PreparedpaymenttransactionsId).HasColumnName("preparedpaymenttransactions_id");
            entity.Property(e => e.Value)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("value");

            entity.HasOne(d => d.Preparedpaymenttransactions).WithMany(p => p.PreparedpaymenttransactionsCustomproperties)
                .HasForeignKey(d => d.PreparedpaymenttransactionsId)
                .HasConstraintName("preparedpaymenttransactions_customproperties");
        });

        modelBuilder.Entity<PreparedpaymenttransactionsNft>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("preparedpaymenttransactions_nfts")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.PreparedpaymenttransactionsId, "preparedpaymenttransactions_preparedpaymenttransactionsnfts");

            entity.HasIndex(e => e.NftId, "preparedpaymenttransactionsnfts_nfts");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Count).HasColumnName("count");
            entity.Property(e => e.Lovelace).HasColumnName("lovelace");
            entity.Property(e => e.NftId).HasColumnName("nft_id");
            entity.Property(e => e.Nftuid)
                .HasMaxLength(255)
                .HasColumnName("nftuid");
            entity.Property(e => e.Policyid)
                .HasMaxLength(255)
                .HasColumnName("policyid");
            entity.Property(e => e.PreparedpaymenttransactionsId).HasColumnName("preparedpaymenttransactions_id");
            entity.Property(e => e.Tokenname)
                .HasMaxLength(255)
                .HasColumnName("tokenname");
            entity.Property(e => e.Tokennamehex)
                .HasMaxLength(255)
                .HasColumnName("tokennamehex");

            entity.HasOne(d => d.Nft).WithMany(p => p.PreparedpaymenttransactionsNfts)
                .HasForeignKey(d => d.NftId)
                .HasConstraintName("preparedpaymenttransactionsnfts_nfts");

            entity.HasOne(d => d.Preparedpaymenttransactions).WithMany(p => p.PreparedpaymenttransactionsNfts)
                .HasForeignKey(d => d.PreparedpaymenttransactionsId)
                .HasConstraintName("preparedpaymenttransactions_preparedpaymenttransactionsnfts");
        });

        modelBuilder.Entity<PreparedpaymenttransactionsNotification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("preparedpaymenttransactions_notifications")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.PreparedpaymenttransactionsId, "notifications_preparedpaymenttransactions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Notificationendpoint)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("notificationendpoint");
            entity.Property(e => e.Notificationtype)
                .IsRequired()
                .HasColumnType("enum('webhook','email')")
                .HasColumnName("notificationtype");
            entity.Property(e => e.PreparedpaymenttransactionsId).HasColumnName("preparedpaymenttransactions_id");
            entity.Property(e => e.Secret)
                .IsRequired()
                .HasMaxLength(255)
                .HasDefaultValueSql("''")
                .HasColumnName("secret");

            entity.HasOne(d => d.Preparedpaymenttransactions).WithMany(p => p.PreparedpaymenttransactionsNotifications)
                .HasForeignKey(d => d.PreparedpaymenttransactionsId)
                .HasConstraintName("notifications_preparedpaymenttransactions");
        });

        modelBuilder.Entity<PreparedpaymenttransactionsSmartcontractOutput>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("preparedpaymenttransactions_smartcontract_outputs");

            entity.HasIndex(e => e.PreparedpaymenttransactionsId, "preparedpaymenttransactions_smartcontract_outputs");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.Lovelace).HasColumnName("lovelace");
            entity.Property(e => e.Pkh)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("pkh");
            entity.Property(e => e.PreparedpaymenttransactionsId).HasColumnName("preparedpaymenttransactions_id");
            entity.Property(e => e.Type)
                .IsRequired()
                .HasDefaultValueSql("'unknown'")
                .HasColumnType("enum('seller','buyer','marketplace','royalties','referer','unknown','nmkr')")
                .HasColumnName("type");

            entity.HasOne(d => d.Preparedpaymenttransactions).WithMany(p => p.PreparedpaymenttransactionsSmartcontractOutputs)
                .HasForeignKey(d => d.PreparedpaymenttransactionsId)
                .HasConstraintName("preparedpaymenttransactions_smartcontract_outputs");
        });

        modelBuilder.Entity<PreparedpaymenttransactionsSmartcontractOutputsAsset>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("preparedpaymenttransactions_smartcontract_outputs_assets");

            entity.HasIndex(e => e.PreparedpaymenttransactionsSmartcontractOutputsId, "preparedpaymenttransactions_smartcontract_outputs_assets");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.Policyid)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("policyid");
            entity.Property(e => e.PreparedpaymenttransactionsSmartcontractOutputsId).HasColumnName("preparedpaymenttransactions_smartcontract_outputs_id");
            entity.Property(e => e.Tokennameinhex)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("tokennameinhex");

            entity.HasOne(d => d.PreparedpaymenttransactionsSmartcontractOutputs).WithMany(p => p.PreparedpaymenttransactionsSmartcontractOutputsAssets)
                .HasForeignKey(d => d.PreparedpaymenttransactionsSmartcontractOutputsId)
                .HasConstraintName("preparedpaymenttransactions_smartcontract_outputs_assets");
        });

        modelBuilder.Entity<PreparedpaymenttransactionsSmartcontractsjson>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("preparedpaymenttransactions_smartcontractsjsons")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.Txid, "smartcontractjsons_txid");

            entity.HasIndex(e => e.PreparedpaymenttransactionsId, "smartcontractsjsons_preparedpaymenttransactions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.Bidamount).HasColumnName("bidamount");
            entity.Property(e => e.Checkforconfirmdate)
                .HasColumnType("datetime")
                .HasColumnName("checkforconfirmdate");
            entity.Property(e => e.Confirmed).HasColumnName("confirmed");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.Fee).HasColumnName("fee");
            entity.Property(e => e.Hash)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("hash");
            entity.Property(e => e.Json)
                .IsRequired()
                .HasColumnType("text")
                .HasColumnName("json");
            entity.Property(e => e.Logfile).HasColumnName("logfile");
            entity.Property(e => e.PreparedpaymenttransactionsId).HasColumnName("preparedpaymenttransactions_id");
            entity.Property(e => e.Rawtx)
                .HasColumnType("text")
                .HasColumnName("rawtx");
            entity.Property(e => e.Redeemer)
                .HasColumnType("text")
                .HasColumnName("redeemer");
            entity.Property(e => e.Signed)
                .HasColumnType("datetime")
                .HasColumnName("signed");
            entity.Property(e => e.Signedandsubmitted).HasColumnName("signedandsubmitted");
            entity.Property(e => e.Signedcbr)
                .HasColumnType("text")
                .HasColumnName("signedcbr");
            entity.Property(e => e.Signinguid)
                .HasMaxLength(255)
                .HasColumnName("signinguid");
            entity.Property(e => e.Submitted)
                .HasColumnType("datetime")
                .HasColumnName("submitted");
            entity.Property(e => e.Templatetype)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("templatetype");
            entity.Property(e => e.Txid).HasColumnName("txid");

            entity.HasOne(d => d.Preparedpaymenttransactions).WithMany(p => p.PreparedpaymenttransactionsSmartcontractsjsons)
                .HasForeignKey(d => d.PreparedpaymenttransactionsId)
                .HasConstraintName("smartcontractsjsons_preparedpaymenttransactions");
        });

        modelBuilder.Entity<PreparedpaymenttransactionsTokenprice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("preparedpaymenttransactions_tokenprice");

            entity.HasIndex(e => e.PreparedpaymenttransactionId, "tokenprice_preparedpaymenttransactions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Assetname)
                .HasMaxLength(255)
                .HasColumnName("assetname");
            entity.Property(e => e.Assetnamehex)
                .HasMaxLength(255)
                .HasColumnName("assetnamehex");
            entity.Property(e => e.Decimals).HasColumnName("decimals");
            entity.Property(e => e.Multiplier)
                .HasDefaultValueSql("'1'")
                .HasColumnName("multiplier");
            entity.Property(e => e.Policyid)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("policyid");
            entity.Property(e => e.PreparedpaymenttransactionId).HasColumnName("preparedpaymenttransaction_id");
            entity.Property(e => e.Tokencount).HasColumnName("tokencount");
            entity.Property(e => e.Totalcount).HasColumnName("totalcount");

            entity.HasOne(d => d.Preparedpaymenttransaction).WithMany(p => p.PreparedpaymenttransactionsTokenprices)
                .HasForeignKey(d => d.PreparedpaymenttransactionId)
                .HasConstraintName("tokenprice_preparedpaymenttransactions");
        });

        modelBuilder.Entity<Pricelist>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("pricelist")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.NftgroupId, "pricelist_nftgroups");

            entity.HasIndex(e => e.NftprojectId, "pricelist_nftprojects");

            entity.HasIndex(e => e.PromotionId, "pricelist_promotions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Assetnamehex)
                .HasMaxLength(255)
                .HasColumnName("assetnamehex");
            entity.Property(e => e.Changeaddresswhenpaywithtokens)
                .HasColumnType("enum('seller','buyer')")
                .HasColumnName("changeaddresswhenpaywithtokens");
            entity.Property(e => e.Countnftortoken).HasColumnName("countnftortoken");
            entity.Property(e => e.Currency)
                .IsRequired()
                .HasDefaultValueSql("'ADA'")
                .HasColumnType("enum('ADA','EUR','USD','JPY','SOL','APT','BTC')")
                .HasColumnName("currency");
            entity.Property(e => e.NftgroupId).HasColumnName("nftgroup_id");
            entity.Property(e => e.NftprojectId).HasColumnName("nftproject_id");
            entity.Property(e => e.Priceinlovelace).HasColumnName("priceinlovelace");
            entity.Property(e => e.Priceintoken).HasColumnName("priceintoken");
            entity.Property(e => e.PromotionId).HasColumnName("promotion_id");
            entity.Property(e => e.Promotionmultiplier).HasColumnName("promotionmultiplier");
            entity.Property(e => e.State)
                .HasColumnType("enum('active','notactive')")
                .HasColumnName("state");
            entity.Property(e => e.Tokenassetid)
                .HasMaxLength(255)
                .HasColumnName("tokenassetid");
            entity.Property(e => e.Tokenmultiplier)
                .HasDefaultValueSql("'1'")
                .HasColumnName("tokenmultiplier");
            entity.Property(e => e.Tokenpolicyid)
                .HasMaxLength(255)
                .HasColumnName("tokenpolicyid");
            entity.Property(e => e.Validfrom)
                .HasColumnType("datetime")
                .HasColumnName("validfrom");
            entity.Property(e => e.Validto)
                .HasColumnType("datetime")
                .HasColumnName("validto");

            entity.HasOne(d => d.Nftgroup).WithMany(p => p.Pricelists)
                .HasForeignKey(d => d.NftgroupId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("pricelist_nftgroups");

            entity.HasOne(d => d.Nftproject).WithMany(p => p.Pricelists)
                .HasForeignKey(d => d.NftprojectId)
                .HasConstraintName("pricelist_nftprojects");

            entity.HasOne(d => d.Promotion).WithMany(p => p.Pricelists)
                .HasForeignKey(d => d.PromotionId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("pricelist_promotions");
        });

        modelBuilder.Entity<Pricelistdiscount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("pricelistdiscounts")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.NftprojectId, "pricelistdiscounts_nftprojects");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Blockchain)
                .IsRequired()
                .HasDefaultValueSql("'Cardano'")
                .HasColumnType("enum('Cardano','Solana','Aptos')")
                .HasColumnName("blockchain");
            entity.Property(e => e.Condition)
                .IsRequired()
                .HasColumnType("enum('walletcontainsminofpolicyid','whitlistedaddresses','stakeonpool','referercode','couponcode')")
                .HasColumnName("condition");
            entity.Property(e => e.Couponcode)
                .HasMaxLength(255)
                .HasColumnName("couponcode");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Minvalue).HasColumnName("minvalue");
            entity.Property(e => e.Minvalue2).HasColumnName("minvalue2");
            entity.Property(e => e.Minvalue3).HasColumnName("minvalue3");
            entity.Property(e => e.Minvalue4).HasColumnName("minvalue4");
            entity.Property(e => e.Minvalue5).HasColumnName("minvalue5");
            entity.Property(e => e.NftprojectId).HasColumnName("nftproject_id");
            entity.Property(e => e.Operator)
                .IsRequired()
                .HasDefaultValueSql("'OR'")
                .HasColumnType("enum('AND','OR')")
                .HasColumnName("operator");
            entity.Property(e => e.Policyid)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("policyid");
            entity.Property(e => e.Policyid2)
                .HasMaxLength(255)
                .HasColumnName("policyid2");
            entity.Property(e => e.Policyid3)
                .HasMaxLength(255)
                .HasColumnName("policyid3");
            entity.Property(e => e.Policyid4)
                .HasMaxLength(255)
                .HasColumnName("policyid4");
            entity.Property(e => e.Policyid5)
                .HasMaxLength(255)
                .HasColumnName("policyid5");
            entity.Property(e => e.Policyprojectname)
                .HasMaxLength(255)
                .HasColumnName("policyprojectname");
            entity.Property(e => e.Referercode)
                .HasMaxLength(255)
                .HasColumnName("referercode");
            entity.Property(e => e.Sendbackdiscount)
                .HasColumnType("float(12,2)")
                .HasColumnName("sendbackdiscount");
            entity.Property(e => e.State)
                .IsRequired()
                .HasColumnType("enum('active','notactive')")
                .HasColumnName("state");
            entity.Property(e => e.Whitlistaddresses).HasColumnName("whitlistaddresses");

            entity.HasOne(d => d.Nftproject).WithMany(p => p.Pricelistdiscounts)
                .HasForeignKey(d => d.NftprojectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("pricelistdiscounts_nftprojects");
        });

        modelBuilder.Entity<Projectaddressestxhash>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("projectaddressestxhashes")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.Txhash, "txhash").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .IsRequired()
                .HasMaxLength(255)
                .HasDefaultValueSql("''")
                .HasColumnName("address");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.Lovelace).HasColumnName("lovelace");
            entity.Property(e => e.Tokens)
                .IsRequired()
                .HasMaxLength(255)
                .HasDefaultValueSql("''")
                .HasColumnName("tokens");
            entity.Property(e => e.Txhash)
                .IsRequired()
                .HasColumnName("txhash");
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("promotions");

            entity.HasIndex(e => e.NftprojectId, "promotions_nftprojects");

            entity.HasIndex(e => e.NftId, "promotions_nfts");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Count).HasColumnName("count");
            entity.Property(e => e.Enddate)
                .HasColumnType("datetime")
                .HasColumnName("enddate");
            entity.Property(e => e.Maxcount).HasColumnName("maxcount");
            entity.Property(e => e.NftId).HasColumnName("nft_id");
            entity.Property(e => e.NftprojectId).HasColumnName("nftproject_id");
            entity.Property(e => e.Soldcount).HasColumnName("soldcount");
            entity.Property(e => e.Startdate)
                .HasColumnType("datetime")
                .HasColumnName("startdate");
            entity.Property(e => e.State)
                .IsRequired()
                .HasColumnType("enum('active','notactive')")
                .HasColumnName("state");

            entity.HasOne(d => d.Nft).WithMany(p => p.Promotions)
                .HasForeignKey(d => d.NftId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("promotions_nfts");

            entity.HasOne(d => d.Nftproject).WithMany(p => p.Promotions)
                .HasForeignKey(d => d.NftprojectId)
                .HasConstraintName("promotions_nftprojects");
        });

        modelBuilder.Entity<Rate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("rates")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Btcrate)
                .HasColumnType("float(20,10)")
                .HasColumnName("btcrate");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.Eurorate)
                .HasColumnType("float(12,4)")
                .HasColumnName("eurorate");
            entity.Property(e => e.Jpyrate)
                .HasColumnType("float(12,4)")
                .HasColumnName("jpyrate");
            entity.Property(e => e.Solbtcrate)
                .HasColumnType("float(20,10)")
                .HasColumnName("solbtcrate");
            entity.Property(e => e.Soleurorate)
                .HasColumnType("float(12,4)")
                .HasColumnName("soleurorate");
            entity.Property(e => e.Soljpyrate)
                .HasColumnType("float(12,4)")
                .HasColumnName("soljpyrate");
            entity.Property(e => e.Solusdrate)
                .HasColumnType("float(12,4)")
                .HasColumnName("solusdrate");
            entity.Property(e => e.Usdrate)
                .HasColumnType("float(12,4)")
                .HasColumnName("usdrate");
        });

        modelBuilder.Entity<Ratelimit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("ratelimit")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.Apikey, "apikey");

            entity.HasIndex(e => e.Created, "created");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Apikey)
                .IsRequired()
                .HasColumnName("apikey");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
        });

        modelBuilder.Entity<Referer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("referer");

            entity.HasIndex(e => e.ReferercustomerId, "referer_customers");

            entity.HasIndex(e => e.PayoutwalletId, "referer_customerwallets");

            entity.HasIndex(e => e.Referertoken, "referertoken").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Commisionpercent)
                .HasColumnType("float(12,2)")
                .HasColumnName("commisionpercent");
            entity.Property(e => e.PayoutwalletId).HasColumnName("payoutwallet_id");
            entity.Property(e => e.ReferercustomerId).HasColumnName("referercustomer_id");
            entity.Property(e => e.Referertoken)
                .IsRequired()
                .HasColumnName("referertoken");
            entity.Property(e => e.State)
                .IsRequired()
                .HasColumnType("enum('active','notactive')")
                .HasColumnName("state");

            entity.HasOne(d => d.Payoutwallet).WithMany(p => p.Referers)
                .HasForeignKey(d => d.PayoutwalletId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("referer_customerwallets");

            entity.HasOne(d => d.Referercustomer).WithMany(p => p.Referers)
                .HasForeignKey(d => d.ReferercustomerId)
                .HasConstraintName("referer_customers");
        });

        modelBuilder.Entity<Refundlog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("refundlogs")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.NftprojectId, "refundlogs_nftprojects");

            entity.HasIndex(e => e.Senderaddress, "refundslog1");

            entity.HasIndex(e => e.Receiveraddress, "refundslog2");

            entity.HasIndex(e => e.Txhash, "refundslog3");

            entity.HasIndex(e => e.Outgoingtxhash, "refundslog4");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Coin)
                .IsRequired()
                .HasDefaultValueSql("'ADA'")
                .HasColumnType("enum('ADA','SOL')")
                .HasColumnName("coin");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.Fee).HasColumnName("fee");
            entity.Property(e => e.Log)
                .HasColumnType("text")
                .HasColumnName("log");
            entity.Property(e => e.Lovelace).HasColumnName("lovelace");
            entity.Property(e => e.NftprojectId).HasColumnName("nftproject_id");
            entity.Property(e => e.Nmkrcosts).HasColumnName("nmkrcosts");
            entity.Property(e => e.Outgoingtxhash).HasColumnName("outgoingtxhash");
            entity.Property(e => e.Receiveraddress)
                .IsRequired()
                .HasColumnName("receiveraddress");
            entity.Property(e => e.Refundreason)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("refundreason");
            entity.Property(e => e.Senderaddress)
                .IsRequired()
                .HasColumnName("senderaddress");
            entity.Property(e => e.State)
                .IsRequired()
                .HasColumnType("enum('successful','failed')")
                .HasColumnName("state");
            entity.Property(e => e.Txhash)
                .IsRequired()
                .HasColumnName("txhash");

            entity.HasOne(d => d.Nftproject).WithMany(p => p.Refundlogs)
                .HasForeignKey(d => d.NftprojectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("refundlogs_nftprojects");
        });

        modelBuilder.Entity<Registeredtoken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("registeredtokens");

            entity.HasIndex(e => e.Policyid, "policyid");

            entity.HasIndex(e => e.Subject, "subject");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.Decimals).HasColumnName("decimals");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Logo).HasColumnName("logo");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Policyid).HasColumnName("policyid");
            entity.Property(e => e.Subject)
                .IsRequired()
                .HasColumnName("subject");
            entity.Property(e => e.Ticker)
                .HasMaxLength(255)
                .HasColumnName("ticker");
            entity.Property(e => e.Url)
                .HasMaxLength(255)
                .HasColumnName("url");
        });

        modelBuilder.Entity<Reservednft>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("reservednfts")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.NftId, "reservednfts_nfts");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.NftId).HasColumnName("nft_id");
            entity.Property(e => e.Reservedcount).HasColumnName("reservedcount");
            entity.Property(e => e.Reservedforaddress)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("reservedforaddress");
            entity.Property(e => e.Reserveduntil)
                .HasColumnType("datetime")
                .HasColumnName("reserveduntil");

            entity.HasOne(d => d.Nft).WithMany(p => p.Reservednfts)
                .HasForeignKey(d => d.NftId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("reservednfts_nfts");
        });

        modelBuilder.Entity<Salenumber>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("salenumbers");

            entity.Property(e => e.Ada)
                .HasPrecision(45, 4)
                .HasColumnName("ada");
            entity.Property(e => e.Eurorate)
                .HasColumnType("float(12,4)")
                .HasColumnName("eurorate");
            entity.Property(e => e.Sold).HasColumnName("sold");
            entity.Property(e => e.Soldnfts).HasColumnName("soldnfts");
        });

        modelBuilder.Entity<Serverexception>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("serverexceptions")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.ServerId, "exceptions_backgroundserver");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.Data).HasColumnName("data");
            entity.Property(e => e.Logmessage)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("logmessage");
            entity.Property(e => e.ServerId).HasColumnName("server_id");

            entity.HasOne(d => d.Server).WithMany(p => p.Serverexceptions)
                .HasForeignKey(d => d.ServerId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("exceptions_backgroundserver");
        });

        modelBuilder.Entity<Setting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("settings")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.MastersettingsId, "settings_settings");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Feespercent)
                .HasDefaultValueSql("'1.00'")
                .HasColumnType("float(10,2)")
                .HasColumnName("feespercent");
            entity.Property(e => e.MastersettingsId).HasColumnName("mastersettings_id");
            entity.Property(e => e.Metadatascaffold)
                .IsRequired()
                .HasColumnName("metadatascaffold");
            entity.Property(e => e.Minfees).HasColumnName("minfees");
            entity.Property(e => e.Minimumtxcount).HasColumnName("minimumtxcount");
            entity.Property(e => e.Mintandsendcoupons)
                .HasDefaultValueSql("'1'")
                .HasColumnName("mintandsendcoupons");
            entity.Property(e => e.Mintingaddress)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("mintingaddress");
            entity.Property(e => e.Mintingaddressbitcoin)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("mintingaddressbitcoin");
            entity.Property(e => e.Mintingaddressdescription)
                .HasMaxLength(255)
                .HasColumnName("mintingaddressdescription");
            entity.Property(e => e.Mintingaddresssaptos)
                .IsRequired()
                .HasMaxLength(255);
            entity.Property(e => e.Mintingaddresssolana)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("mintingaddresssolana");
            entity.Property(e => e.Mintingcosts).HasColumnName("mintingcosts");
            entity.Property(e => e.Mintingcostsaptos).HasColumnName("mintingcostsaptos");
            entity.Property(e => e.Mintingcostsbitcoin).HasColumnName("mintingcostsbitcoin");
            entity.Property(e => e.Mintingcostssolana).HasColumnName("mintingcostssolana");
            entity.Property(e => e.Minutxo).HasColumnName("minutxo");
            entity.Property(e => e.Pricemintcoupons).HasColumnName("pricemintcoupons");
            entity.Property(e => e.Pricemintcouponsaptos).HasColumnName("pricemintcouponsaptos");
            entity.Property(e => e.Pricemintcouponssolana).HasColumnName("pricemintcouponssolana");
            entity.Property(e => e.Priceupdatenfts)
                .HasColumnType("float(12,2)")
                .HasColumnName("priceupdatenfts");
            entity.Property(e => e.Uploadsourceforceprice)
                .HasMaxLength(255)
                .HasComment("When an upload source was  passed by the api function (uploadNft), we will set the price settings of the project to this setting")
                .HasColumnName("uploadsourceforceprice");

            entity.HasOne(d => d.Mastersettings).WithMany(p => p.InverseMastersettings)
                .HasForeignKey(d => d.MastersettingsId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("settings_settings");
        });

        modelBuilder.Entity<Sftpgenericfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("sftpgenericfiles")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Content)
                .IsRequired()
                .HasColumnName("content")
                .UseCollation("latin1_swedish_ci")
                .HasCharSet("latin1");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.Mimetype)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("mimetype")
                .UseCollation("latin1_swedish_ci")
                .HasCharSet("latin1");
            entity.Property(e => e.State)
                .IsRequired()
                .HasColumnType("enum('active','notactive')")
                .HasColumnName("state")
                .UseCollation("latin1_swedish_ci")
                .HasCharSet("latin1");
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("title")
                .UseCollation("latin1_swedish_ci")
                .HasCharSet("latin1");
        });

        modelBuilder.Entity<Smartcontract>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("smartcontracts")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.DefaultprojectId, "smartcontracts_projects");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.DefaultprojectId)
                .HasComment("The project we will use if no other project is specified. The project is only for the settings")
                .HasColumnName("defaultproject_id");
            entity.Property(e => e.Filename)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("filename");
            entity.Property(e => e.Hashaddress)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("hashaddress");
            entity.Property(e => e.Memvalue).HasColumnName("memvalue");
            entity.Property(e => e.Plutus).HasColumnName("plutus");
            entity.Property(e => e.Smartcontractname)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("smartcontractname");
            entity.Property(e => e.Sourcecode).HasColumnName("sourcecode");
            entity.Property(e => e.State)
                .IsRequired()
                .HasColumnType("enum('active','notactive')")
                .HasColumnName("state");
            entity.Property(e => e.Timevalue).HasColumnName("timevalue");
            entity.Property(e => e.Type)
                .IsRequired()
                .HasColumnType("enum('auction','directsale','directsaleV2','directsaleoffer','cip68')")
                .HasColumnName("type");

            entity.HasOne(d => d.Defaultproject).WithMany(p => p.Smartcontracts)
                .HasForeignKey(d => d.DefaultprojectId)
                .HasConstraintName("smartcontracts_projects");
        });

        modelBuilder.Entity<Smartcontractsjsontemplate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("smartcontractsjsontemplates")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.SmartcontractsId, "smartcontractsjsontemplates_smartcontracts");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Jsontemplate)
                .IsRequired()
                .HasColumnType("text")
                .HasColumnName("jsontemplate");
            entity.Property(e => e.Recipienttemplate)
                .IsRequired()
                .HasColumnType("text")
                .HasColumnName("recipienttemplate");
            entity.Property(e => e.Redeemertemplate)
                .HasColumnType("text")
                .HasColumnName("redeemertemplate");
            entity.Property(e => e.SmartcontractsId).HasColumnName("smartcontracts_id");
            entity.Property(e => e.Templatetype)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("templatetype");

            entity.HasOne(d => d.Smartcontracts).WithMany(p => p.Smartcontractsjsontemplates)
                .HasForeignKey(d => d.SmartcontractsId)
                .HasConstraintName("smartcontractsjsontemplates_smartcontracts");
        });

        modelBuilder.Entity<Smartcontractsmarketplacesetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("smartcontractsmarketplacesettings")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.Collateral)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("collateral");
            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Fakesignaddress)
                .HasMaxLength(255)
                .HasColumnName("fakesignaddress");
            entity.Property(e => e.Fakesignskey)
                .HasMaxLength(255)
                .HasColumnName("fakesignskey");
            entity.Property(e => e.Fakesignvkey)
                .HasMaxLength(255)
                .HasColumnName("fakesignvkey");
            entity.Property(e => e.Percentage)
                .HasColumnType("float(11,2)")
                .HasColumnName("percentage");
            entity.Property(e => e.Pkh)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("pkh");
            entity.Property(e => e.Salt)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("salt");
            entity.Property(e => e.Skey)
                .IsRequired()
                .HasColumnType("text")
                .HasColumnName("skey");
            entity.Property(e => e.Vkey)
                .IsRequired()
                .HasColumnType("text")
                .HasColumnName("vkey");
        });

        modelBuilder.Entity<Soldnft>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("soldnft")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.ServerId, "soldnft_backgroundserver");

            entity.HasIndex(e => e.NftId, "soldnft_nfts");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.NftId).HasColumnName("nft_id");
            entity.Property(e => e.ServerId).HasColumnName("server_id");
            entity.Property(e => e.Tokencount).HasColumnName("tokencount");

            entity.HasOne(d => d.Nft).WithMany(p => p.Soldnfts)
                .HasForeignKey(d => d.NftId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("soldnft_nfts");

            entity.HasOne(d => d.Server).WithMany(p => p.Soldnfts)
                .HasForeignKey(d => d.ServerId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("soldnft_backgroundserver");
        });

        modelBuilder.Entity<Splitroyaltyaddress>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("splitroyaltyaddresses");

            entity.HasIndex(e => e.CustomerId, "splitroyalityaddresses_customers");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.Comment)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("comment");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Lastcheck)
                .HasColumnType("datetime")
                .HasColumnName("lastcheck");
            entity.Property(e => e.Lovelace).HasColumnName("lovelace");
            entity.Property(e => e.Minthreshold).HasColumnName("minthreshold");
            entity.Property(e => e.Salt)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("salt");
            entity.Property(e => e.Skey)
                .IsRequired()
                .HasColumnType("text")
                .HasColumnName("skey");
            entity.Property(e => e.State)
                .IsRequired()
                .HasColumnType("enum('active','notactive','deleted')")
                .HasColumnName("state");
            entity.Property(e => e.Vkey)
                .IsRequired()
                .HasColumnType("text")
                .HasColumnName("vkey");

            entity.HasOne(d => d.Customer).WithMany(p => p.Splitroyaltyaddresses)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("splitroyalityaddresses_customers");
        });

        modelBuilder.Entity<Splitroyaltyaddressessplit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("splitroyaltyaddressessplits");

            entity.HasIndex(e => e.SplitroyaltyaddressesId, "splitroyaltyaddressessplits_splitroyaltyaddresses");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Activefrom)
                .HasColumnType("datetime")
                .HasColumnName("activefrom");
            entity.Property(e => e.Activeto)
                .HasColumnType("datetime")
                .HasColumnName("activeto");
            entity.Property(e => e.Address)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.IsMainReceiver)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("isMainReceiver");
            entity.Property(e => e.Percentage)
                .HasComment("percentage * 100 / so 10 percent = 1000")
                .HasColumnName("percentage");
            entity.Property(e => e.SplitroyaltyaddressesId).HasColumnName("splitroyaltyaddresses_id");
            entity.Property(e => e.State)
                .IsRequired()
                .HasColumnType("enum('active','notactive')")
                .HasColumnName("state");

            entity.HasOne(d => d.Splitroyaltyaddresses).WithMany(p => p.Splitroyaltyaddressessplits)
                .HasForeignKey(d => d.SplitroyaltyaddressesId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("splitroyaltyaddressessplits_splitroyaltyaddresses");
        });

        modelBuilder.Entity<Splitroyaltyaddressestransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("splitroyaltyaddressestransactions");

            entity.HasIndex(e => e.SplitroyaltyaddressesId, "splitroyaltyaddressestransactions_splitroyaltyaddresses");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.Changeaddress)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("changeaddress");
            entity.Property(e => e.Costs).HasColumnName("costs");
            entity.Property(e => e.Costsaddress)
                .HasMaxLength(255)
                .HasColumnName("costsaddress");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.Fee).HasColumnName("fee");
            entity.Property(e => e.SplitroyaltyaddressesId).HasColumnName("splitroyaltyaddresses_id");
            entity.Property(e => e.Txid)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("txid");

            entity.HasOne(d => d.Splitroyaltyaddresses).WithMany(p => p.Splitroyaltyaddressestransactions)
                .HasForeignKey(d => d.SplitroyaltyaddressesId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("splitroyaltyaddressestransactions_splitroyaltyaddresses");
        });

        modelBuilder.Entity<Splitroyaltyaddressestransactionssplit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("splitroyaltyaddressestransactionssplits");

            entity.HasIndex(e => e.SplitroyaltyaddressestransactionsId, "royaltyaddressestransactionssplits_royaltyaddressestransactions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.Percentage).HasColumnName("percentage");
            entity.Property(e => e.Splitaddress)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("splitaddress");
            entity.Property(e => e.SplitroyaltyaddressestransactionsId).HasColumnName("splitroyaltyaddressestransactions_id");

            entity.HasOne(d => d.Splitroyaltyaddressestransactions).WithMany(p => p.Splitroyaltyaddressestransactionssplits)
                .HasForeignKey(d => d.SplitroyaltyaddressestransactionsId)
                .HasConstraintName("royaltyaddressestransactionssplits_royaltyaddressestransactions");
        });

        modelBuilder.Entity<Stakepoolreward>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("stakepoolrewards")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Reward).HasColumnName("reward");
            entity.Property(e => e.Stakepoolid)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("stakepoolid");
            entity.Property(e => e.Stakepoolname)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("stakepoolname");
        });

        modelBuilder.Entity<Statistic>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("statistics")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.CustomerId, "statistics_customers");

            entity.HasIndex(e => e.NftprojectId, "statistics_nftprojects");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasColumnType("double(20,2)")
                .HasColumnName("amount");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Day).HasColumnName("day");
            entity.Property(e => e.Mintingcosts)
                .HasColumnType("double(20,2)")
                .HasColumnName("mintingcosts");
            entity.Property(e => e.Minutxo)
                .HasColumnType("double(20,2)")
                .HasColumnName("minutxo");
            entity.Property(e => e.Month).HasColumnName("month");
            entity.Property(e => e.NftprojectId).HasColumnName("nftproject_id");
            entity.Property(e => e.Sales).HasColumnName("sales");
            entity.Property(e => e.Transactionfees)
                .HasColumnType("double(20,2)")
                .HasColumnName("transactionfees");
            entity.Property(e => e.Year).HasColumnName("year");

            entity.HasOne(d => d.Customer).WithMany(p => p.Statistics)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("statistics_customers");

            entity.HasOne(d => d.Nftproject).WithMany(p => p.Statistics)
                .HasForeignKey(d => d.NftprojectId)
                .HasConstraintName("statistics_nftprojects");
        });

        modelBuilder.Entity<Storesetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("storesettings");

            entity.HasIndex(e => e.Settingsname, "storesettings").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Allowedfiletypes)
                .HasMaxLength(255)
                .HasColumnName("allowedfiletypes");
            entity.Property(e => e.Category)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("category");
            entity.Property(e => e.Description)
                .IsRequired()
                .HasColumnType("text")
                .HasColumnName("description");
            entity.Property(e => e.Humanreadablesettingsname)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("humanreadablesettingsname");
            entity.Property(e => e.Listvalues)
                .HasColumnType("text")
                .HasColumnName("listvalues");
            entity.Property(e => e.Mandantory).HasColumnName("mandantory");
            entity.Property(e => e.Maxheight).HasColumnName("maxheight");
            entity.Property(e => e.Maxlength).HasColumnName("maxlength");
            entity.Property(e => e.Maxwidth).HasColumnName("maxwidth");
            entity.Property(e => e.Page).HasColumnName("page");
            entity.Property(e => e.Settingsname)
                .IsRequired()
                .HasColumnName("settingsname");
            entity.Property(e => e.Settingstype)
                .IsRequired()
                .HasColumnType("enum('string','int','color','url','email','twitterhandle','boolean','collectionlist','image','favicon','list','fontlist')")
                .HasColumnName("settingstype");
            entity.Property(e => e.Sortorder).HasColumnName("sortorder");
            entity.Property(e => e.Subcategory)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("subcategory");
        });

        modelBuilder.Entity<Submission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("submissions")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.ProcessedbyserverId, "submissions_backgroundserver");

            entity.HasIndex(e => e.NftprojectId, "submissions_nftproject");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.Matxsigned)
                .IsRequired()
                .HasColumnType("text")
                .HasColumnName("matxsigned");
            entity.Property(e => e.NftprojectId).HasColumnName("nftproject_id");
            entity.Property(e => e.ProcessedbyserverId).HasColumnName("processedbyserver_id");
            entity.Property(e => e.Reservationtoken)
                .HasMaxLength(255)
                .HasColumnName("reservationtoken");
            entity.Property(e => e.State)
                .IsRequired()
                .HasColumnType("enum('waitingforsubmission','inprogress','submitted')")
                .HasColumnName("state");
            entity.Property(e => e.Submissionlogfile)
                .HasColumnType("text")
                .HasColumnName("submissionlogfile");
            entity.Property(e => e.Submitresult)
                .HasColumnType("enum('successful','error')")
                .HasColumnName("submitresult");
            entity.Property(e => e.Submitted)
                .HasColumnType("datetime")
                .HasColumnName("submitted");
            entity.Property(e => e.Txid)
                .HasMaxLength(255)
                .HasColumnName("txid");
            entity.Property(e => e.Type)
                .HasColumnType("enum('nftrandom','nftspecific','smartcontractauction','smartcontractdirectsale')")
                .HasColumnName("type");

            entity.HasOne(d => d.Nftproject).WithMany(p => p.Submissions)
                .HasForeignKey(d => d.NftprojectId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("submissions_nftproject");

            entity.HasOne(d => d.Processedbyserver).WithMany(p => p.Submissions)
                .HasForeignKey(d => d.ProcessedbyserverId)
                .HasConstraintName("submissions_backgroundserver");
        });

        modelBuilder.Entity<Tokenreward>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tokenrewards");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Mincount).HasColumnName("mincount");
            entity.Property(e => e.Policyid)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("policyid");
            entity.Property(e => e.Reward).HasColumnName("reward");
            entity.Property(e => e.State)
                .IsRequired()
                .HasColumnType("enum('active','notactive')")
                .HasColumnName("state");
            entity.Property(e => e.Tokennameinhex)
                .HasMaxLength(255)
                .HasColumnName("tokennameinhex");
        });

        modelBuilder.Entity<Tooltiphelpertext>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("tooltiphelpertexts");

            entity.HasIndex(e => e.Description, "tooltiphelpertextsdescription").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .IsRequired()
                .HasColumnName("description");
            entity.Property(e => e.Link)
                .HasColumnType("text")
                .HasColumnName("link");
            entity.Property(e => e.Subtitle)
                .HasMaxLength(255)
                .HasColumnName("subtitle");
            entity.Property(e => e.Text).HasColumnName("text");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("transactions")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.Created, "transactiondate");

            entity.HasIndex(e => e.CustomerId, "transactions_customers");

            entity.HasIndex(e => e.WalletId, "transactions_customerwallets");

            entity.HasIndex(e => e.NftaddressId, "transactions_nftaddresses");

            entity.HasIndex(e => e.NftprojectId, "transactions_nftprojects");

            entity.HasIndex(e => e.PreparedpaymenttransactionId, "transactions_preparedpaymenttransactions");

            entity.HasIndex(e => e.RefererId, "transactions_referer");

            entity.HasIndex(e => e.Transactionid, "transactions_txhash");

            entity.HasIndex(e => e.Transactiontype, "transactiontype");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Ada).HasColumnName("ada");
            entity.Property(e => e.Cbor)
                .HasColumnType("text")
                .HasColumnName("cbor");
            entity.Property(e => e.Checkforconfirmdate)
                .HasColumnType("datetime")
                .HasColumnName("checkforconfirmdate");
            entity.Property(e => e.Cip68referencetokenaddress)
                .HasMaxLength(255)
                .HasColumnName("cip68referencetokenaddress");
            entity.Property(e => e.Cip68referencetokenminutxo).HasColumnName("cip68referencetokenminutxo");
            entity.Property(e => e.Coin)
                .HasDefaultValueSql("'ADA'")
                .HasColumnType("enum('ADA','SOL','APT','BTC')")
                .HasColumnName("coin");
            entity.Property(e => e.Confirmed).HasColumnName("confirmed");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Customerproperty)
                .HasMaxLength(255)
                .HasColumnName("customerproperty");
            entity.Property(e => e.Discount)
                .HasDefaultValueSql("'0'")
                .HasColumnName("discount");
            entity.Property(e => e.Discountcode)
                .HasMaxLength(255)
                .HasColumnName("discountcode");
            entity.Property(e => e.Eurorate)
                .HasColumnType("float(12,4)")
                .HasColumnName("eurorate");
            entity.Property(e => e.Fee)
                .HasDefaultValueSql("'0'")
                .HasColumnName("fee");
            entity.Property(e => e.Incomingtxblockchaintime).HasColumnName("incomingtxblockchaintime");
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(255)
                .HasColumnName("ipaddress");
            entity.Property(e => e.Metadata)
                .HasColumnType("text")
                .HasColumnName("metadata");
            entity.Property(e => e.Metadatastandard)
                .HasColumnType("enum('cip25','cip68','solana','aptos')")
                .HasColumnName("metadatastandard");
            entity.Property(e => e.Mintingcostsada)
                .HasDefaultValueSql("'0'")
                .HasColumnName("mintingcostsada");
            entity.Property(e => e.Mintingcostsaddress)
                .HasMaxLength(255)
                .HasColumnName("mintingcostsaddress");
            entity.Property(e => e.NftaddressId).HasColumnName("nftaddress_id");
            entity.Property(e => e.Nftcount).HasColumnName("nftcount");
            entity.Property(e => e.NftprojectId).HasColumnName("nftproject_id");
            entity.Property(e => e.Nmkrcosts).HasColumnName("nmkrcosts");
            entity.Property(e => e.Originatoraddress)
                .HasMaxLength(255)
                .HasColumnName("originatoraddress");
            entity.Property(e => e.Paymentmethod)
                .IsRequired()
                .HasDefaultValueSql("'ADA'")
                .HasColumnType("enum('ADA','FIAT','ETH','SOL','APT')")
                .HasColumnName("paymentmethod");
            entity.Property(e => e.PreparedpaymenttransactionId).HasColumnName("preparedpaymenttransaction_id");
            entity.Property(e => e.Priceintokensmultiplier).HasColumnName("priceintokensmultiplier");
            entity.Property(e => e.Priceintokenspolicyid)
                .HasMaxLength(255)
                .HasColumnName("priceintokenspolicyid");
            entity.Property(e => e.Priceintokensquantity).HasColumnName("priceintokensquantity");
            entity.Property(e => e.Priceintokenstokennamehex)
                .HasMaxLength(255)
                .HasColumnName("priceintokenstokennamehex");
            entity.Property(e => e.Projectada)
                .HasDefaultValueSql("'0'")
                .HasColumnName("projectada");
            entity.Property(e => e.Projectaddress)
                .HasMaxLength(255)
                .HasColumnName("projectaddress");
            entity.Property(e => e.Projectincomingtxhash)
                .HasMaxLength(255)
                .HasColumnName("projectincomingtxhash");
            entity.Property(e => e.Receiveraddress)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("receiveraddress");
            entity.Property(e => e.RefererCommission)
                .HasDefaultValueSql("'0'")
                .HasColumnName("referer_commission");
            entity.Property(e => e.RefererId).HasColumnName("referer_id");
            entity.Property(e => e.Senderaddress)
                .HasMaxLength(255)
                .HasColumnName("senderaddress");
            entity.Property(e => e.Serverid).HasColumnName("serverid");
            entity.Property(e => e.Stakeaddress)
                .HasMaxLength(255)
                .HasColumnName("stakeaddress");
            entity.Property(e => e.Stakereward)
                .HasDefaultValueSql("'0'")
                .HasColumnName("stakereward");
            entity.Property(e => e.State)
                .HasColumnType("enum('signed','submitted','confirmed')")
                .HasColumnName("state");
            entity.Property(e => e.Stopresubmitting).HasColumnName("stopresubmitting");
            entity.Property(e => e.Telemetrytooktime).HasColumnName("telemetrytooktime");
            entity.Property(e => e.Tokenreward)
                .HasDefaultValueSql("'0'")
                .HasColumnName("tokenreward");
            entity.Property(e => e.Transactionblockchaintime).HasColumnName("transactionblockchaintime");
            entity.Property(e => e.Transactionid).HasColumnName("transactionid");
            entity.Property(e => e.Transactiontype)
                .IsRequired()
                .HasColumnType("enum('paidonftaddress','mintfromcustomeraddress','paidtocustomeraddress','paidfromnftaddress','consolitecustomeraddress','paidfailedtransactiontocustomeraddress','doublepaymentsendbacktobuyer','paidonprojectaddress','fiatpayment','mintfromnftmakeraddress','burning','decentralmintandsend','decentralmintandsale','royaltsplit','unknown','directsale','auction','buymints','refundmints')")
                .HasColumnName("transactiontype");
            entity.Property(e => e.WalletId).HasColumnName("wallet_id");

            entity.HasOne(d => d.Customer).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("transactions_customers");

            entity.HasOne(d => d.Nftaddress).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.NftaddressId)
                .HasConstraintName("transactions_nftaddresses");

            entity.HasOne(d => d.Nftproject).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.NftprojectId)
                .HasConstraintName("transactions_nftprojects");

            entity.HasOne(d => d.Preparedpaymenttransaction).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.PreparedpaymenttransactionId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("transactions_preparedpaymenttransactions");

            entity.HasOne(d => d.Referer).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.RefererId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("transactions_referer");

            entity.HasOne(d => d.Wallet).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.WalletId)
                .HasConstraintName("transactions_customerwallets");
        });

        modelBuilder.Entity<TransactionNft>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("transaction_nfts")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.TransactionId, "transactionnfts_transactions");

            entity.HasIndex(e => e.NftId, "transactions_nfts");

            entity.HasIndex(e => e.NftarchiveId, "transactionsnfts_nftarchives");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Checkforconfirmdate)
                .HasColumnType("datetime")
                .HasColumnName("checkforconfirmdate");
            entity.Property(e => e.Confirmed).HasColumnName("confirmed");
            entity.Property(e => e.Ispromotion).HasColumnName("ispromotion");
            entity.Property(e => e.Mintedontransaction).HasColumnName("mintedontransaction");
            entity.Property(e => e.Multiplier)
                .HasDefaultValueSql("'1'")
                .HasColumnName("multiplier");
            entity.Property(e => e.NftId).HasColumnName("nft_id");
            entity.Property(e => e.NftarchiveId).HasColumnName("nftarchive_id");
            entity.Property(e => e.Tokencount)
                .HasDefaultValueSql("'1'")
                .HasColumnName("tokencount");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.Transactionblockchaintime).HasColumnName("transactionblockchaintime");
            entity.Property(e => e.Txhash)
                .HasMaxLength(255)
                .HasColumnName("txhash");

            entity.HasOne(d => d.Nft).WithMany(p => p.TransactionNfts)
                .HasForeignKey(d => d.NftId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("transactions_nfts");

            entity.HasOne(d => d.Nftarchive).WithMany(p => p.TransactionNfts)
                .HasForeignKey(d => d.NftarchiveId)
                .HasConstraintName("transactionsnfts_nftarchives");

            entity.HasOne(d => d.Transaction).WithMany(p => p.TransactionNfts)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("transactionnfts_transactions");
        });

        modelBuilder.Entity<TransactionsAdditionalpayout>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("transactions_additionalpayouts");

            entity.HasIndex(e => e.TransactionId, "transactions_additionalpayouts_transactions");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Lovelace).HasColumnName("lovelace");
            entity.Property(e => e.Payoutaddress)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("payoutaddress");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");

            entity.HasOne(d => d.Transaction).WithMany(p => p.TransactionsAdditionalpayouts)
                .HasForeignKey(d => d.TransactionId)
                .HasConstraintName("transactions_additionalpayouts_transactions");
        });

        modelBuilder.Entity<Transactionstatistic>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("transactionstatistics");

            entity.Property(e => e.Counttx).HasColumnName("counttx");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.D1).HasColumnName("d1");
            entity.Property(e => e.NftprojectId).HasColumnName("nftproject_id");
            entity.Property(e => e.Sumada)
                .HasPrecision(45, 4)
                .HasColumnName("sumada");
            entity.Property(e => e.Sumcosts)
                .HasPrecision(47, 4)
                .HasColumnName("sumcosts");
            entity.Property(e => e.Sumfee)
                .HasPrecision(45, 4)
                .HasColumnName("sumfee");
            entity.Property(e => e.Summintcosts)
                .HasPrecision(45, 4)
                .HasColumnName("summintcosts");
            entity.Property(e => e.Sumprojectada)
                .HasPrecision(45, 4)
                .HasColumnName("sumprojectada");
            entity.Property(e => e.Sumtotal)
                .HasPrecision(48, 4)
                .HasColumnName("sumtotal");
        });

        modelBuilder.Entity<Txhashcache>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("txhashcache");

            entity.HasIndex(e => e.Txhash, "txhash").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.Transactionobject)
                .IsRequired()
                .HasColumnType("text")
                .HasColumnName("transactionobject");
            entity.Property(e => e.Txhash)
                .IsRequired()
                .HasColumnName("txhash");
        });

        modelBuilder.Entity<Updateprojectsid>(entity =>
        {
            entity.HasKey(e => e.Dummyid).HasName("PRIMARY");

            entity
                .ToTable("updateprojectsid")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.Id, "id");

            entity.Property(e => e.Dummyid).HasColumnName("dummyid");
            entity.Property(e => e.Id).HasColumnName("id");
        });

        modelBuilder.Entity<Usedaddressesonsalecondition>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("usedaddressesonsaleconditions");

            entity.HasIndex(e => e.SalecondtionsId, "usedaddresses_saleconditions");

            entity.HasIndex(e => new { e.Address, e.SalecondtionsId }, "usedaddressesaddress").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .IsRequired()
                .HasColumnName("address");
            entity.Property(e => e.Created)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("created");
            entity.Property(e => e.SalecondtionsId).HasColumnName("salecondtions_id");

            entity.HasOne(d => d.Salecondtions).WithMany(p => p.Usedaddressesonsaleconditions)
                .HasForeignKey(d => d.SalecondtionsId)
                .HasConstraintName("usedaddresses_saleconditions");
        });

        modelBuilder.Entity<Validationaddress>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("validationaddresses")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("address")
                .UseCollation("latin1_swedish_ci")
                .HasCharSet("latin1");
            entity.Property(e => e.Password)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("password")
                .UseCollation("latin1_swedish_ci")
                .HasCharSet("latin1");
            entity.Property(e => e.Privateskey)
                .IsRequired()
                .HasColumnName("privateskey")
                .UseCollation("latin1_swedish_ci")
                .HasCharSet("latin1");
            entity.Property(e => e.Privatevkey)
                .IsRequired()
                .HasColumnName("privatevkey")
                .UseCollation("latin1_swedish_ci")
                .HasCharSet("latin1");
            entity.Property(e => e.State)
                .IsRequired()
                .HasColumnType("enum('active','notactive')")
                .HasColumnName("state")
                .UseCollation("latin1_swedish_ci")
                .HasCharSet("latin1");
        });

        modelBuilder.Entity<Validationamount>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("validationamounts")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => new { e.ValidationaddressId, e.Lovelace }, "lovelace").IsUnique();

            entity.HasIndex(e => e.ValidationaddressId, "validationamounts_validationaddresses");

            entity.HasIndex(e => e.Uid, "validationamountsuid").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Lovelace).HasColumnName("lovelace");
            entity.Property(e => e.Optionalvalidationname)
                .HasMaxLength(255)
                .HasColumnName("optionalvalidationname");
            entity.Property(e => e.Senderaddress)
                .HasMaxLength(255)
                .HasColumnName("senderaddress");
            entity.Property(e => e.State)
                .IsRequired()
                .HasColumnType("enum('notvalidated','validated','expired')")
                .HasColumnName("state");
            entity.Property(e => e.Uid)
                .IsRequired()
                .HasColumnName("uid");
            entity.Property(e => e.ValidationaddressId).HasColumnName("validationaddress_id");
            entity.Property(e => e.Validuntil)
                .HasColumnType("datetime")
                .HasColumnName("validuntil");

            entity.HasOne(d => d.Validationaddress).WithMany(p => p.Validationamounts)
                .HasForeignKey(d => d.ValidationaddressId)
                .HasConstraintName("validationamounts_validationaddresses");
        });

        modelBuilder.Entity<Vestingoffer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("vestingoffers");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.Extendedapienabled).HasColumnName("extendedapienabled");
            entity.Property(e => e.Iagonenabled).HasColumnName("iagonenabled");
            entity.Property(e => e.Maxfiles).HasColumnName("maxfiles");
            entity.Property(e => e.Maxfilesize).HasColumnName("maxfilesize");
            entity.Property(e => e.Maxstorage).HasColumnName("maxstorage");
            entity.Property(e => e.Periodindays).HasColumnName("periodindays");
            entity.Property(e => e.Vesttokenada).HasColumnName("vesttokenada");
            entity.Property(e => e.Vesttokenassetname)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("vesttokenassetname");
            entity.Property(e => e.Vesttokenpolicyid)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("vesttokenpolicyid");
            entity.Property(e => e.Vesttokenquantity).HasColumnName("vesttokenquantity");
        });

        modelBuilder.Entity<Websitelog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("websitelog")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.CustomerId, "websitelog_customers");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Created)
                .HasColumnType("datetime")
                .HasColumnName("created");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Function)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("function")
                .UseCollation("utf8mb4_0900_ai_ci")
                .HasCharSet("utf8mb4");
            entity.Property(e => e.Parameter)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("parameter");
            entity.Property(e => e.Servername)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("servername")
                .UseCollation("utf8mb4_0900_ai_ci")
                .HasCharSet("utf8mb4");

            entity.HasOne(d => d.Customer).WithMany(p => p.Websitelogs)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("websitelog_customers");
        });

        modelBuilder.Entity<Websitesetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity
                .ToTable("websitesettings")
                .HasCharSet("utf8mb3")
                .UseCollation("utf8mb3_general_ci");

            entity.HasIndex(e => e.Key, "webseitesettingskey").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Boolvalue).HasColumnName("boolvalue");
            entity.Property(e => e.Key)
                .IsRequired()
                .HasColumnName("key");
            entity.Property(e => e.Stringvalue)
                .HasColumnType("text")
                .HasColumnName("stringvalue");
        });

        modelBuilder.Entity<Whitelabelstorecollection>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("whitelabelstorecollections");

            entity.HasIndex(e => e.NftprojectId, "whitelabelstorecollections_nftprojects");

            entity.HasIndex(e => e.StoresettingsId, "whitelabelstorecollections_storesettings");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Activefrom)
                .HasColumnType("datetime")
                .HasColumnName("activefrom");
            entity.Property(e => e.Activeto)
                .HasColumnType("datetime")
                .HasColumnName("activeto");
            entity.Property(e => e.Collectiondescription)
                .HasMaxLength(255)
                .HasColumnName("collectiondescription");
            entity.Property(e => e.Collectionname)
                .HasMaxLength(255)
                .HasColumnName("collectionname");
            entity.Property(e => e.Discordlink)
                .HasMaxLength(255)
                .HasColumnName("discordlink");
            entity.Property(e => e.Dropprojectuid)
                .HasMaxLength(255)
                .HasColumnName("dropprojectuid");
            entity.Property(e => e.Instagramlink)
                .HasMaxLength(255)
                .HasColumnName("instagramlink");
            entity.Property(e => e.Isdropinprogess)
                .HasDefaultValueSql("'0'")
                .HasColumnName("isdropinprogess");
            entity.Property(e => e.Nameofcreator)
                .HasMaxLength(255)
                .HasColumnName("nameofcreator");
            entity.Property(e => e.NftprojectId).HasColumnName("nftproject_id");
            entity.Property(e => e.Policyid)
                .IsRequired()
                .HasMaxLength(255)
                .HasColumnName("policyid");
            entity.Property(e => e.Previewimage)
                .HasMaxLength(255)
                .HasColumnName("previewimage");
            entity.Property(e => e.Showonfrontpage)
                .IsRequired()
                .HasDefaultValueSql("'1'")
                .HasColumnName("showonfrontpage");
            entity.Property(e => e.State)
                .IsRequired()
                .HasColumnType("enum('active','notactive')")
                .HasColumnName("state");
            entity.Property(e => e.StoresettingsId).HasColumnName("storesettings_id");
            entity.Property(e => e.Twritterlink)
                .HasMaxLength(255)
                .HasColumnName("twritterlink");
            entity.Property(e => e.Uid)
                .HasMaxLength(255)
                .HasColumnName("uid");

            entity.HasOne(d => d.Nftproject).WithMany(p => p.Whitelabelstorecollections)
                .HasForeignKey(d => d.NftprojectId)
                .HasConstraintName("whitelabelstorecollections_nftprojects");

            entity.HasOne(d => d.Storesettings).WithMany(p => p.Whitelabelstorecollections)
                .HasForeignKey(d => d.StoresettingsId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("whitelabelstorecollections_storesettings");
        });

        modelBuilder.Entity<Whitelabelstoresetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("whitelabelstoresettings");

            entity.HasIndex(e => new { e.StoresettingsId, e.NftprojectId }, "storesettings_stores").IsUnique();

            entity.HasIndex(e => e.NftprojectId, "whitelabelstoresettings_nftprojects");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.NftprojectId).HasColumnName("nftproject_id");
            entity.Property(e => e.StoresettingsId).HasColumnName("storesettings_id");
            entity.Property(e => e.Value)
                .IsRequired()
                .HasColumnType("text")
                .HasColumnName("value");

            entity.HasOne(d => d.Nftproject).WithMany(p => p.Whitelabelstoresettings)
                .HasForeignKey(d => d.NftprojectId)
                .HasConstraintName("whitelabelstoresettings_nftprojects");

            entity.HasOne(d => d.Storesettings).WithMany(p => p.Whitelabelstoresettings)
                .HasForeignKey(d => d.StoresettingsId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("whitelabelstoresettings_storesettings");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
