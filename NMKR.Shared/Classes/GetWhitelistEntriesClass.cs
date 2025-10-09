using System;

namespace NMKR.Shared.Classes
{
    public class GetWhitelistEntriesClass
    {
        public string Addresss { get; set; }
        public string Stakeaddress { get; set; }
        public long CountNftsOrTokens { get; set; }
        public DateTime Created { get; set; }
        public long TotalSoldNftsOrTokens { get; set; }
        public SoldNftsOrTokensFromWhitelist[] SoldNftsOrTokens { get; set; }
    }


    public class SoldNftsOrTokensFromWhitelist
    {
        public string Usedaddress { get; set; }
        public string Originatoraddress { get; set; }
        public string Transactionid { get; set; }
        public DateTime Created { get; set; }
        public long Countnft { get; set; }
    }
}
