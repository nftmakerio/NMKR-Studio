using NMKR.Shared.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using CardanoSharp.Wallet.Extensions;
using CardanoSharp.Wallet.Models;
using CardanoSharp.Wallet.Models.Addresses;
using CardanoSharp.Wallet.Models.Transactions;
using NMKR.Shared.Enums;
using NMKR.Shared.Classes.Bitcoin;

namespace NMKR.Shared.Classes
{
    public enum Dataproviders
    {
        Default,
        Koios,
        Blockfrost,
        Cli,
        Maestro
    }

    public class TxInTokensClass
    {
        public string PolicyId { get; set; }
        public string Tokenname { get; set; }
        public string TokennameHex { get; set; }
        public long Quantity { get; set; }

        public string TokenHex()
        {
            return Quantity + " " + PolicyId + "." + TokennameHex;
        }

        public string Token()
        {
            return Quantity + " " + PolicyId + "." + Tokenname;
        }

        public void ChangeQuantity(long addQuantity)
        {
            Quantity += addQuantity;
            if (Quantity < 0)
                Quantity = 0;
        }

        public string Fingerprint => Bech32Engine.GetFingerprint(PolicyId, TokennameHex);
    }
    public class TxInClass
    {
        public string TxHash { get; set; }
        public long TxId { get; set; }
        public long Lovelace { get; set; }
        public List<TxInTokensClass> Tokens { get; set; } = new List<TxInTokensClass>();

        public void AddTokens(string policyId,  string tokenNameHex, long quantity)
        {
            /*
            ArrayHelper.Add(ref Tokens, new TxInTokensClass()
            {
                PolicyId = policyId,
                Tokenname = tokenNameHex.FromHex(),
                TokennameHex = tokenNameHex,
                Quantity = quantity
            }); */

            Tokens.Add(new TxInTokensClass()
            {
                PolicyId = policyId,
                Tokenname = tokenNameHex.FromHex(),
                TokennameHex = tokenNameHex,
                Quantity = quantity
            });
        }

        public void AddTokens(string unit, long quantity)
        {
            if (unit.Length > 56)
                AddTokens(unit.Substring(0, 56), unit.Substring(56), quantity);
        }


        public string TxHashId => TxHash + "#" + TxId;

        public DateTime? TXTimestamp { get; set; }

        public long TokenSum
        {
            get
            {
                if (Tokens == null || !Tokens.Any())
                    return 0;

                return Tokens.Count;
            }
        }

        public string ScriptPubKey { get; set; }
        public long? Confirmations { get; set; }

        //  public long Slot { get; set; }
    }

    public class AllTxInAddressesClass
    {
        public TxInAddressesClass[] TxInAddresses { get; set; }

        public IEnumerable<Utxo> ToCardanosharpUtxos()
        {
            List<Utxo> utxo = new List<Utxo>();

            foreach (var txInAddressesClass in TxInAddresses.OrEmptyIfNull())
            {
                utxo.AddRange(txInAddressesClass.ToCardanosharpUtxos());
            }
            return utxo;
        }
    }


    public class TxInAddressesClass
    {
        private TxInClass[] _txIn;  
        public string Address { get; set; }
        public string StakeAddress { get; set; }

        public void AddTxIn(string txhash, long txid)
        {
            ArrayHelper.Add(ref _txIn, new TxInClass()
            {
                TxHash = txhash,
                TxId = txid
            });
        }
        public void AddTxIn(string txhash, long txid, long lovelace)
        {
            ArrayHelper.Add(ref _txIn, new TxInClass()
            {
                TxHash = txhash,
                TxId = txid,
                Lovelace = lovelace
            });
        }
        public TxInClass[] TxIn
        {
            get => _txIn;
            set => _txIn = value;
        }

        public long LovelaceSummary
        {
            get
            {
                if (!_txIn.Any())
                    return 0;

                long sum = 0;
                Array.ForEach(_txIn, i => sum += i.Lovelace);
                return sum;
            }
        }

        public int TokensSum
        {
            get
            {
                if (!_txIn.Any())
                    return 0;
                
                int sum = 0;

              //  _txIn.Sum(x => x.Tokens.Count);

                Array.ForEach(_txIn, i => sum += i.Tokens?.Count ?? 0);
                return sum;
            }
        }
       
        public Dataproviders DataProvider { get; set; }
        public long TotalTokenSum { get
        {
            return _txIn.Sum(x => x.TokenSum);
        } }

        public string GetFirstTxHash()
        {
            if (_txIn == null || !_txIn.Any())
                return "";

            return TxIn.First().TxHashId; // Important - use the sorted TxIn
        }


        public string GetTxInTokens(string txhash)
        {
            var txin = _txIn.FirstOrDefault(x => x.TxHashId == txhash);
            if (txin == null)
                return "";

            if (txin.Tokens == null || !txin.Tokens.Any())
                return "";

            string tokens = "";
            foreach (var txInTokensClass in txin.Tokens)
            {
                if (!string.IsNullOrEmpty(tokens))
                    tokens += " + ";
                tokens += txInTokensClass.TokenHex();
            }

            return tokens;
        }

        public string GetTxInTokens()
        {
            string tokens = "";
            foreach (var txInClass in _txIn)
            {
                var tok=GetTxInTokens(txInClass.TxHashId);
                if (!string.IsNullOrEmpty(tok))
                {
                    if (!string.IsNullOrEmpty(tokens))
                        tokens += " + ";
                    tokens += tok;
                }
            }

            return tokens;
        }

      
        public TxInTokensClass[] GetAllTokens()
        {
                List<TxInTokensClass> tokens = new List<TxInTokensClass>();

                foreach (var txInClass in _txIn.OrEmptyIfNull())
                {
                        foreach (var txInTokensClass in txInClass.Tokens.OrEmptyIfNull())
                        {
                            var token = tokens.FirstOrDefault(x => x.PolicyId == txInTokensClass.PolicyId && x.Tokenname == txInTokensClass.Tokenname);
                            if (token == null)
                            {
                                tokens.Add(new TxInTokensClass()
                                {
                                    PolicyId = txInTokensClass.PolicyId,
                                    Tokenname = txInTokensClass.Tokenname,
                                    TokennameHex = txInTokensClass.TokennameHex,
                                    Quantity = txInTokensClass.Quantity
                                });
                            }
                            else
                            {
                                token.ChangeQuantity(txInTokensClass.Quantity);
                            }
                        }
                }
                return tokens.ToArray();
        }

        public AssetsAssociatedWithAccount[] GetAllAssetsAssociatedWithAccounts()
        {
            var tokens = GetAllTokens();

            return tokens.Select(token => new AssetsAssociatedWithAccount(token.PolicyId,  token.TokennameHex, token.Quantity, Blockchain.Cardano, address: Address)).ToArray();
        }

        public long GetLovelace(string txhash)
        {
            var txin = _txIn.FirstOrDefault(x => x.TxHashId == txhash);
            if (txin == null)
                return 0;

            return txin.Lovelace;
        }
        public string GetTxHashId(int ix)
        {
            if (_txIn == null || !_txIn.Any())
                return "";
            if (ix >= _txIn.Length)
                return "";

            return TxIn[ix].TxHashId; // Important - use the TxIn (the sorted output)
        }

        public string GetTxHashesString()
        {
            return TxIn.Aggregate("", (current, txInClass) => current + (txInClass.TXTimestamp + " - " + txInClass.TxHashId + Environment.NewLine));
        }

        public IEnumerable<Utxo> ToCardanosharpUtxos()
        {
            List<Utxo> utxo = new List<Utxo>();

            foreach (var txInClass in TxIn.OrEmptyIfNull())
            {
                Balance b = new Balance() {Lovelaces = (ulong) txInClass.Lovelace};

                foreach (var token in txInClass.Tokens.OrEmptyIfNull())
                {
                    b.Assets ??= new List<Asset>();

                    b.Assets.Add(new Asset(){Name = token.TokennameHex, PolicyId = token.PolicyId, Quantity = token.Quantity});
                }
                var u = new Utxo() { TxHash = txInClass.TxHash, TxIndex = (uint)txInClass.TxId, Balance = b, OutputAddress = Address};
                utxo.Add(u);
            }
            return utxo;
        }

        public IEnumerable<TransactionOutput> ToTransactionOutput(string receiverAddress)
        {
            List<TransactionOutput> toutput = new List<TransactionOutput>();
            Dictionary<byte[], NativeAsset> assets = new Dictionary<byte[], NativeAsset>();

            foreach (var txInClass in TxIn.OrEmptyIfNull())
            {
                foreach (var txInTokensClass in txInClass.Tokens.OrEmptyIfNull())
                {
                    assets.Add(txInTokensClass.PolicyId.HexToByteArray(), new NativeAsset()
                    {
                        Token = new Dictionary<byte[], long>()
                        {
                            {txInTokensClass.TokennameHex.HexToByteArray(), txInTokensClass.Quantity}
                        }
                    });
                }
            }

            var t1 = new TransactionOutput()
            {
                Address = new Address(receiverAddress).GetBytes(),
                Value = new TransactionOutputValue()
                {
                    Coin = (ulong) LovelaceSummary,
                    MultiAsset = assets,
                }
            };
            toutput.Add(t1);


            return toutput;
        }

        internal void AddTxIn(BitcoinGetUtxosDatum bitcoinGetUtxosDatum)
        {
            ArrayHelper.Add(ref _txIn, new TxInClass()
            {
                TxHash = bitcoinGetUtxosDatum.Txid,
                TxId = bitcoinGetUtxosDatum.Vout??0,
                Lovelace = bitcoinGetUtxosDatum.Satoshis??0,
                ScriptPubKey= bitcoinGetUtxosDatum.ScriptPubkey,
                Confirmations= bitcoinGetUtxosDatum.Confirmations
            });
        }
        internal void AddTxIn(TxInClass txin)
        {
            ArrayHelper.Add(ref _txIn, txin);
        }
    }
}
