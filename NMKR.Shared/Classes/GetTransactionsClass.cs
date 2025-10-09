using System;
using NMKR.Shared.Enums;

namespace NMKR.Shared.Classes
{
    public class GetTransactionsClass
    {
       public DateTime Created { get; set; }
       public string State { get; set; }
        public int NftprojectId { get; set; }
        public long Ada { get; set; }
        public long Fee { get; set; }
        public long Mintingcostsada { get; set; }
        public long Projectada { get; set; }
        public string Projectincomingtxhash { get; set; }
        public string Receiveraddress { get; set; }
        public string Senderaddress { get; set; }
        public string Transactionid { get; set; }
        public string Transactiontype { get; set; }
        public string Projectaddress { get; set; }
        public double Eurorate { get; set; }
        public long Nftcount   { get; set; }
        public long Tokencount { get; set; }
       public string Originatoraddress { get; set; }
        public long Stakereward { get; set; }
        public string Stakeaddress { get; set; }
        public long AdditionalPayoutWallets { get; set; }
        public bool Confirmed { get; set; }
        public long Priceintokensquantity { get; set; }
        public string Priceintokenspolicyid { get; set; }
        public string Priceintokenstokennamehex { get; set; }
        public long Priceintokensmultiplier { get; set; }
        public long Nmkrcosts { get; set; }
        public long Discount { get; set; }
        public string CustomerProperty { get; set; }
        public Blockchain Blockchain { get; set; }
        public GetTransactionNftsClass[] TransactionNfts { get; set; }
        public string Coin { get; set; }
        public string Projectname { get; set; }
        public string NftProjectUid { get; set; }
    }

    public class GetTransactionNftsClass
    {
        public string AssetName { get; set; }
        public string Fingerprint { get; set; }
        public long TokenCount { get; set; }
        public long Multiplier { get; set; }
        public string TxHashSolanaTransaction { get; set; }
        public bool Confirmed { get; set; }
    }
    public class GetTransactionNftsCsvExportClass : GetTransactionNftsClass
    {
       public string Transactionid { get; set; }

       public GetTransactionNftsCsvExportClass(GetTransactionNftsClass nft, string transactionid)
       {
            AssetName = nft.AssetName;
            Fingerprint = nft.Fingerprint;
            TokenCount = nft.TokenCount;
            Multiplier = nft.Multiplier;
            TxHashSolanaTransaction = nft.TxHashSolanaTransaction;
            Confirmed = nft.Confirmed;
            Transactionid = transactionid;
        }
    }
}
