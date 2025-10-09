using System;
using System.ComponentModel.DataAnnotations;

namespace NMKR.Shared.Classes
{

    public class EditProjectClass
    {
        [Required]
        [StringLength(255, MinimumLength = 1)]
        public string Projectname { get; set; }
        public string ProjectLogo { get; set; }
        public string Projecturl { get; set; }

        [StringLength(15, MinimumLength = 0)]
        [RegularExpression(@"^[^\u0022]+$|^$", ErrorMessage = "Quotation marks are not allowed")]
        public string TokennamePrefix { get; set; }
        public string Twitterhandle { get; set; }
        public string Twitterurl { get; set; }
        public string Discordurl { get; set; }

        //  [Range(1, 20, ErrorMessage = "The field {0} must be greater than {1}.")]
        public int MaxMintAndSendCount { get; set; }

        public string Description { get; set; }
        [Required]
        public int ExpireTime { get; set; } = 20;

        public int WalletId { get; set; }
        public int SolanaWalletId { get; set; }
        public int AptosWalletId { get; set; }
        public int BitcoinWalletId { get; set; }

        [Range(1, 9999999999999999999, ErrorMessage = "The field {0} must be greater than {1}.")]
        public long MaxNftSupply { get; set; }

        public string PolicyId { get; set; }
        public int SettingsId { get; set; }
        public string MinUtxo { get; set; }
        public string MintAndSendMinUtxo { get; set; }
        public string State { get; set; }
        public bool EnableCrossSalesOnPaymentgateway { get; set; }
        public bool EnableFiatPaymentOnPaymentgateway { get; set; }
        public bool EnableDecentralPayments { get; set; }
        public int FiatWalletUSDC { get; set; }
        public bool DisableManualMintingbutton { get; set; }
        public bool DisableRandomSales { get; set; }
        public bool DisableSpecificSales { get; set; }
        public long? NftsBlocked { get; set; }
        public DateTime? PolicyExpiration { get; set; }
        public bool PolicyExpires { get; set; }
        public string PrivateVKey { get; set; }
        public int CustomerId {get; set; }
        public string ProjectType { get; set; }
        public double? MarketplaceWhitelabelFee { get; set; }
        public int SmartcontractMarketplaceSettingsId { get; set; }
        public string NmkrAccountOptions { get; set; }
        public bool DontDisableThePayinAdress { get; set; }
        public string CrossmintCollectionId { get; set; }
        public int Checkfiat { get; set; }
        public bool UseFrankenProtection { get; set; }
        public string StorageProvider { get; set; }
        public bool Cip68 { get; set; }
        public string MetaDatax { get; set; }
        public string Enabledcoins { get; set; }
    }
}
