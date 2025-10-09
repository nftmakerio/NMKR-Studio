using System.Collections.Generic;

namespace NMKR.Shared.Classes
{
    public class SmartContractsPayoutsClass
    {
        public string address { get; set; }
        public long lovelace { get; set; }
        public string tokens { get; set; }
        public ReceiverTypes receivertype { get; set; }
    }

    public enum ReceiverTypes
    {
        buyer,
        seller,
        marketplace,
        royalties,
        referer,
        nmkr,
        unknown
    }

    public class SmartContractAuctionsParameterClass
    {
        public string costsfile;
        public TxInAddressesClass[] utxopaymentaddress { get; set; }
        public string changeaddress { get; set; }
        public long bidamount { get; set; }
        public string scripthash { get; set; }
        public string scriptDatumHash { get; set; }
        public string protocolParamsFile { get; set; }
        public string matxrawfile { get; set; }
        public string scriptfile { get; set; }
        public string redeemerfile { get; set; }
        public string signerhash { get; set; }
        public string collateraltxin { get; set; }
        public string olddatumfile { get; set; }
        public string newdatumfile { get; set; }

        public long? tokencount { get; set; }
        public string sendbackmessagefile { get; set; }

        public string legacyaddress { get; set; }
        public long lockamount { get; set; }
        public string policyidAndTokenname { get; set; }
        public long startslot { get; set; }
        public long next10slots { get; set; }
        public string utxoScript { get; set; }
        public List<SmartContractsPayoutsClass> receiver { get; set; } = new List<SmartContractsPayoutsClass>();
        public long? smartcontractmemvalue { get; set; }
        public long? smartcontracttimevalue { get; set; }
    }
}
