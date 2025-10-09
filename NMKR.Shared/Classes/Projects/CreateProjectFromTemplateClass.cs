using NMKR.Shared.Functions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using NMKR.Shared.Enums;
using NMKR.Shared.Model;

namespace NMKR.Shared.Classes.Projects
{
    public class CreateProjectFromTemplateClass
    {
        public event EventHandler DataChanged;
        public event EventHandler BlockchainChanged;

        [Required]
        [StringLength(64, MinimumLength = 1)]
        [RegularExpression("^[a-zA-Z0-9_ .,öäüÖÄÜß#]*$", ErrorMessage = "Some Characters are not allowed")]
        public string Projectname
        {
            get => _projectname;
            set
            {
                _projectname = value;
                DataChanged?.Invoke(null, new NmkrChangeEventArgs() { Value = value });
            }
        }
        private string _projectname;
        public string Projectdescription { get; set; }
        public string Projecturl { get; set; }
    
     
        [StringLength(255, MinimumLength = 0)]
        public string Description { get; set; }
       
        public string Password = GlobalFunctions.GetGuid();
        public string Uid = Guid.NewGuid().ToString();
        public string StorageProvider = "IPFS";
        public bool cardano
        {
            get
            {
                return _blockchainselection.Exists(x => x == Blockchain.Cardano);
            }
        }
        public bool solana
        {
            get
            {
                return _blockchainselection.Exists(x => x == Blockchain.Solana);
            }
        }
        public bool aptos
        {
            get
            {
                return _blockchainselection.Exists(x => x == Blockchain.Aptos);
            }
        }
        public bool bitcoin
        {
            get
            {
                return _blockchainselection.Exists(x => x == Blockchain.Bitcoin);
            }
        }
        public bool ethereum
        {
            get
            {
                return _blockchainselection.Exists(x => x == Blockchain.Ethereum);
            }
        }
        public bool hedara
        {
            get
            {
                return _blockchainselection.Exists(x => x == Blockchain.Hedara);
            }
        }
        public IEnumerable<Blockchain> Blockchainselection
        {
            get => _blockchainselection; 
            set
            {
                _blockchainselection = value.ToList();
                DataChanged?.Invoke(null, new NmkrChangeEventArgs(){Value = value});
                BlockchainChanged?.Invoke(null, new NmkrChangeEventArgs() { Value = value });
            }
        }

        private List<Blockchain> _blockchainselection = new List<Blockchain>();

        [StringLength(10, MinimumLength = 0)]
        [RegularExpression("^[a-zA-Z0-9_]*$", ErrorMessage = "Only alphanumeric Characters are allowed")]
       // [RequiredIfAll("solana", new[] { true }, ErrorMessage = "Solana Symbol is required.")]
        public string SolanaSymbol { get=>_solanaSymbol;
            set
            {
                _solanaSymbol = value;
                DataChanged?.Invoke(null, new NmkrChangeEventArgs() { Value = value });
            }
        }
        private string _solanaSymbol;

        public int CardanoWalletId { get; set; }
        public int SolanaWalletId { get; set; }
        public int AptosWalletId { get; set; }
        public int BitcoinWalletId { get; set; }

        public string Solanacollectionfamily { get; set; }
        public MetadatatemplateToggle Template
        {
            get => _template;
            set
            {
                _template = value;
                DataChanged?.Invoke(null, new NmkrChangeEventArgs() { Value = value });
            }
        }

        public bool PolicyExpires
        {
            get => _policyExpires;
            set
            {
                _policyExpires = value;
                DataChanged?.Invoke(null, new NmkrChangeEventArgs() { Value = value });
            }
        }

        private bool _policyExpires;

        public long MaxNftSupply
        {
            get => _maxNftSupply;
            set
            {
                _maxNftSupply = value;
                DataChanged?.Invoke(null, new NmkrChangeEventArgs() { Value = value });
            }
        }

        private long _maxNftSupply;

        public DateTime? PolicyExpiration
        {
            get => _policyExpiration;
            set
            {
                _policyExpiration = value;
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
        public int SellerFeeBasisPoints
        {
            get => _sellerFeeBasisPoints;
            set
            {
                _sellerFeeBasisPoints = value;
                DataChanged?.Invoke(null, new NmkrChangeEventArgs() { Value = value });
            } }

        public bool IsNftProject
        {
            get
            {
                if (_template == null)
                    return false;
                if (_template.Projecttype=="nft")
                    return true;

                return false;
            }
        }
        public bool IsFtProject
        {
            get
            {
                if (_template == null)
                    return false;
                if (_template.Projecttype == "ft")
                    return true;

                return false;
            }
        }

        public IpfsWebUploadFilesClass? AptosCollectionImage
        {
            get => _aptosCollectionImage;
            set
            {
                _aptosCollectionImage = value;
                DataChanged?.Invoke(null, new NmkrChangeEventArgs() { Value = value });
            }
        }

        public string AptosCollectionName { get; set; }

        private IpfsWebUploadFilesClass? _aptosCollectionImage;

        private int _sellerFeeBasisPoints;

        private DateTime? _policyExpiration;

        private MetadatatemplateToggle _template;
        public List<Customerwallet> wallets = new();
    }
}
