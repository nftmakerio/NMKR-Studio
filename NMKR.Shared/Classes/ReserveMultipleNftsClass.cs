using System;

namespace NMKR.Shared.Classes
{
    public class ReserveMultipleNftsClass
    {
        public Int64 Lovelace { get; set; }
        public ReserveNftsClass[] ReserveNfts { get; set; }
    }

    public class ReserveNftsClass
    {
        public int NftId { get; set; }
        public long Tokencount { get; set; }
        public long Multiplier { get; set; } = 1;
    }
}
