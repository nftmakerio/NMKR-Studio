using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NMKR.Shared.Classes.Projects;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using NMKR.Shared.Functions.Extensions;
using NMKR.Shared.Model;

namespace NMKR.Shared.Classes
{
    public class CreateNewProjectClass : CreateProjectFromTemplateClass
    {
        public IpfsWebUploadFilesClass ProjectLogo { get; set; }
        public string Projecturl { get; set; }

        [StringLength(15, MinimumLength = 0)]
        [RegularExpression("^[a-zA-Z0-9_]*$", ErrorMessage ="Only alphanumeric Characters are allowed")]
        public string TokennamePrefix { get; set; }
       
        public int ExpireTime { get; set; } = 20;
      

        [Range(1, Int64.MaxValue, ErrorMessage = "The field {0} must be greater than {1}.")]
        public long MaxNftSupply { get; set; }

        public double? MarketplaceWhitelabelFee { get; set; }

        [ValidTwitterHandle(ErrorMessage = "Twitterhandle is not correct (must be more than 4 characters long and can be up to 15 characters)")]
        public string Twitterhandle { get; set; }
        public int ft_nonft_project { get; set; } = 0;


        public Nftproject ToNftProject()
        {
            Nftproject res = new Nftproject()
            {
                Description = Description,
                Solanasymbol = base.SolanaSymbol,
                Projectname = Projectname,
                Projecturl = Projecturl,
                Tokennameprefix = TokennamePrefix,
                Expiretime = ExpireTime,
                Enablesolana=solana,
                Enablecardano=cardano,
                Enabledcoins = (cardano?Coin.ADA+" " : "")+ (solana?Coin.SOL+" ":""),
                Maxsupply = MaxNftSupply,
                Twitterhandle = Twitterhandle,
                Created = DateTime.Now,
            };
            return res;
        }

        public void SetBlockchains(string projectEnabledcoins)
        {
            var coins = projectEnabledcoins.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
            List<Blockchain> blockchains = new();
            foreach (var c in coins)
            {
                blockchains.Add((c.ToEnum<Coin>()).ToBlockchain());
            }

            Blockchainselection = blockchains;
        }
    }

    public class CreateNewProjectBlockchainsClass
    {
        public event EventHandler DataChanged;
        public string PolicyId { get; set; }
        public string PolicyScript { get; set; }
        public string PrivateSigningkey { get; set; }
        public bool PolicyExpires { get; set; }
        public bool CreateSolanaVerifiedCollection { get; set; }
        public bool IntegrateSolanaCollectionAddressInMetadata { get; set; }
        public bool IntegrateCardanoPolicyIdInMetadata { get; set; }
        public DateTime? PolicyExpiration { get; set; }
        public int NewPolicy { get; set; }
        public int? SellerFeeBasisPoints { get; set; }
        public bool NewPolicy1
        {
            get { return NewPolicy == 0; }
            set {}
        }

        public string PrivateVerifykey { get; set; }

        public int CardanoWalletId { get; set; }
        public int AptosWalletId { get; set; }
        public int BitcoinWalletId { get; set; }
        public string AptosCollectionName { get; set; }

        public int SolanaWalletId { get; set; }
        public string SolanaSymbol { get; set; }
        public string Solanacollectionfamily { get; set; }
        public IpfsWebUploadFilesClass? AptosCollectionImage
        {
            get => _aptosCollectionImage;
            set
            {
                _aptosCollectionImage = value;
                DataChanged?.Invoke(null, new NmkrChangeEventArgs() { Value = value });
            }
        }
        public IpfsWebUploadFilesClass? SolanaCollectionImage
        {
            get => _solanaCollectionImage;
            set
            {
                _solanaCollectionImage = value;
                DataChanged?.Invoke(null, new NmkrChangeEventArgs() { Value = value });
            }
        }
        private IpfsWebUploadFilesClass? _solanaCollectionImage;
        private IpfsWebUploadFilesClass? _aptosCollectionImage;


        [RequiredIfAll("Cip68ReferenceAddressType", new[] { 0 }, "CipStandard", new[] { 1 }, ErrorMessage = "Cip68 Reference Address is required.")]
        [ValidCardanoAddressOrAdaHandle(ErrorMessage = "Address is not a valid cardano address or ADA Handle")]
        public string Cip68ReferenceAddress { get; set; }

        public int Cip68ReferenceAddressType { get; set; } = -1;
        public int CipStandard { get; set; } = 0;
        public string Cip68Extrafield { get; set; }
        public int Cip68ExtrafieldType { get; set; } = 0;

    }

    public class CreateNewProject3Class
    {
        [Required]
        public string MetaData { get; set; }

        public int CipStandard { get; set; }
    }
}
