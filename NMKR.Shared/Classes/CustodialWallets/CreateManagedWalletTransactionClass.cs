using System.Collections.Generic;
using System.Linq;
using CardanoSharp.Wallet.Extensions;
using CardanoSharp.Wallet.Models.Addresses;
using CardanoSharp.Wallet.Models.Transactions;

namespace NMKR.Shared.Classes.CustodialWallets
{
    public class CreateManagedWalletTransactionClass : MakeTransactionBaseClass
    {
        public TransactionReceiversClass[] Receivers { get; set; }
      
        public IEnumerable<TransactionOutput> ToTransactionOutput()
        {
            List<TransactionOutput> toutput = new List<TransactionOutput>();

            foreach (var receiver in Receivers.OrEmptyIfNull())
            {
                Dictionary<byte[], NativeAsset> assets = new Dictionary<byte[], NativeAsset>();


                foreach (var token in receiver.SendTokens.OrEmptyIfNull())
                {
                    assets.Add(token.PolicyId.HexToByteArray(), new NativeAsset()
                    {
                        Token = new Dictionary<byte[], long>()
                        {
                            {token.AssetNameInHex.HexToByteArray(), token.Quantity}
                        }
                    });
                }

                var t1 = new TransactionOutput()
                {
                    Address = new Address(receiver.ReceiverAddress).GetBytes(),
                    Value = new TransactionOutputValue()
                    {
                        Coin = (ulong) receiver.ReceiverLovelace,
                        MultiAsset = assets,
                    }
                };
                toutput.Add(t1);
            }


            return toutput;
        }
    }

    public class SendAllAssetsTransactionClass : MakeTransactionBaseClass
    {
        public string ReceiverAddress { get; set; }

        public CreateManagedWalletTransactionClass ToCreateManagedWalletTransactionClass(TxInAddressesClass utxo)
        {
            List<TransactionTokensClass> sendtokens = new List<TransactionTokensClass>();
            foreach (var txInClass in utxo.TxIn)
            {
                foreach (var txInTokensClass in txInClass.Tokens)
                {
                    sendtokens.Add(new TransactionTokensClass()
                    {
                        AssetNameInHex = txInTokensClass.TokennameHex, PolicyId = txInTokensClass.PolicyId,
                        Quantity = txInTokensClass.Quantity
                    });
                }
            }

            CreateManagedWalletTransactionClass cmtc = new CreateManagedWalletTransactionClass()
            {
                Senderaddress = Senderaddress,
                Walletpassword = Walletpassword,
                Receivers = new TransactionReceiversClass[]
                {
                    new TransactionReceiversClass()
                    {
                        ReceiverAddress = ReceiverAddress,
                        ReceiverLovelace = utxo.LovelaceSummary,
                        SendTokens = sendtokens.ToArray()
                    }
                }
            };
            return cmtc;
        }
    }

    public class MakeTransactionBaseClass
    {
        public string Senderaddress { get; set; }
        public string Walletpassword { get; set; }
    }


    public class TransactionReceiversClass
    {
        public string ReceiverAddress { get; set; }
        public long ReceiverLovelace { get; set; }
        public TransactionTokensClass[] SendTokens { get; set; }

        public string ToSendTokenString()
        {
            if (SendTokens == null || !SendTokens.Any())
                return "";

            string sendtokenstring = "";
            foreach (var sendToken in SendTokens)
            {
                if (!string.IsNullOrEmpty(sendtokenstring))
                    sendtokenstring += " + ";
                sendtokenstring += $"{sendToken.Quantity} {sendToken.PolicyId}.{sendToken.AssetNameInHex}";
            }

            return sendtokenstring;
        }
    }
    public class TransactionTokensClass
    {
        public string PolicyId { get; set; }
        public string AssetNameInHex { get; set; }
        public long Quantity { get; set; }
    }
}
