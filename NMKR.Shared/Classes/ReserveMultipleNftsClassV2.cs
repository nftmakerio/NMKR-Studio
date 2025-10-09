using System;

namespace NMKR.Shared.Classes
{
    public class ReserveMultipleNftsClassV2
    {
        public ReserveNftsClassV2[] ReserveNfts { get; set; }
    }

    public class ReserveNftsClassV2
    {
        public long? Lovelace { get; set; }
        public string NftUid { get; set; }
        [Obsolete]
        public int? NftId { get; set; }
        public long? Tokencount { get; set; }
    }

    public class ReservedNftsClassV2
    {
        public string NftUid { get; set; }
        public long Tokencount { get; set; }
        public string TokennameHex { get; set; }
        public string PolicyId { get; set; }
        public int? NftId { get; set; }
        public Int64? Lovelace { get; set; }
    }
    
}
