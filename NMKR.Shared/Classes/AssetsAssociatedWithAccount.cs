using NMKR.Shared.Classes.Blockfrost;
using NMKR.Shared.Enums;
using NMKR.Shared.Functions;
using Newtonsoft.Json;

namespace NMKR.Shared.Classes
{
    public class AssetsAssociatedWithAccount
    {
        public AssetsAssociatedWithAccount()
        {

        }
        public AssetsAssociatedWithAccount(string policyidOrCollection, string assetnameinhex, long? quantity, Blockchain blockchain, string address="", string symbol="")
        {
            Unit = blockchain == Blockchain.Cardano ? policyidOrCollection +  assetnameinhex : policyidOrCollection;
            Quantity = quantity??0;
            Blockchain = blockchain;
            _assetnameinhex = assetnameinhex;
          /*  if (assetnameinhex.ToLower().StartsWith("000de140") || assetnameinhex.ToLower().StartsWith("000643b0") ||
                assetnameinhex.ToLower().StartsWith("0014df10"))
          */
                _policyidOrCollection = policyidOrCollection;
            Address = address;
            SolanaSymbol = symbol;
        }
        public AssetsAssociatedWithAccount(BlockfrostAssetsResultClass bfr, string stakeAddress)
        {
            Blockchain = Blockchain.Cardano;
            Unit = bfr.Unit;
            Quantity = bfr.Quantity??1;
            _policyidOrCollection = Unit.Substring(0, 56);
            _assetnameinhex = Unit.Substring(56);
            Address = stakeAddress;
        }

        public CardanoCipTypes CardanoCipType
        {
            get
            {
                if (Blockchain != Blockchain.Cardano)
                    return CardanoCipTypes.None;
                if (string.IsNullOrEmpty(_assetnameinhex))
                    return CardanoCipTypes.Cip25;
                if (_assetnameinhex.ToLower().StartsWith("000de140"))
                    return CardanoCipTypes.Cip68NftUserToken;
                if (_assetnameinhex.ToLower().StartsWith("000643b0"))
                    return CardanoCipTypes.Cip68ReferenceToken;
                if (_assetnameinhex.ToLower().StartsWith("0014df10"))
                    return CardanoCipTypes.Cip68FtUserToken;
                return CardanoCipTypes.Cip25;
            }
        }
        public string SolanaSymbol { get; set; }

        public string Address { get; set; }

        private string _assetnameinhex;

        private string _policyidOrCollection;
        // Format: Policy-Id . Tokenname in hex
        [JsonProperty("unit", NullValueHandling = NullValueHandling.Ignore)]
        public string Unit { get; set; }

        [JsonProperty("quantity", NullValueHandling = NullValueHandling.Ignore)]
        public long Quantity { get; set; }
        public Blockchain Blockchain { get; }

        [JsonProperty("fingerprint", NullValueHandling = NullValueHandling.Ignore)]
        public string Fingerprint
        {
            get
            {
                if (Blockchain == Blockchain.Cardano)
                {
                    return Bech32Engine.GetFingerprint(Unit.Substring(0, 56),
                        Unit.Length > 56 ? Unit.Substring(56) : "");
                }
                return null;
            }
        }

        [JsonProperty("assetname", NullValueHandling = NullValueHandling.Ignore)]
        public string AssetName
        {
            get
            {
                if (Blockchain == Blockchain.Cardano)
                {
                    var n = Unit.Substring(56);
                    if (CardanoCipType != CardanoCipTypes.Cip25)
                        n = Unit.Substring(64);
                    return n.FromHex();
                }

                if (Blockchain == Blockchain.Solana)
                    return _assetnameinhex.FromHex();
                return null;
            }
        }

        [JsonProperty("policyid", NullValueHandling = NullValueHandling.Ignore)]
        public string PolicyIdOrCollection
        {
            get => _policyidOrCollection;
            set => _policyidOrCollection = value;
        }

        [JsonProperty("assetnameinhex", NullValueHandling = NullValueHandling.Ignore)]
        public string AssetNameInHex
        {
            get => _assetnameinhex;
            set => _assetnameinhex = value;
        }
      
    }
}
