using System.Collections.Generic;
using NMKR.Shared.Model;

namespace NMKR.Shared.Classes
{
    public class CardanoTransactionClass
    {
        public TxInAddressesClass UtxoPaymentAddress { get; set; }
        public string SenderAddress { get; set; }
        public long MintingFees { get; set; }
        public long NftCosts { get; set; }
        public string ReceiverAddressNft { get; set; }
        public string ReceiverAddressProject { get; set; }
        public string ReceiverAddressMinting { get; set; }
        public string Guid { get; set; }
        public string PolicyKeyFile { get; set; }
        public string MatxrawFilename { get; set; }
        public string MatxSignedFilename { get; set; }
        public string PolicyscriptFilename { get; set; }
        public string MetadataFilename { get; set; }

        public long TTL { get; set; }

        public List<string> SignFiles = new();

        public long Fee { get; set; }
        public BuildTransactionClass buildtransaction = new();
        public int CountWitness { get; set; }
        public Nft[] nft { get; set; }

        public bool Result { get; set; }
        public string TokennamePrefix { get; set; }
        public string Policyid { get; set; }
    }
}
