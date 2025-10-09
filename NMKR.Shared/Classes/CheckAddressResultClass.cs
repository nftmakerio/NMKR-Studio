using System;
using NMKR.Shared.Enums;

namespace NMKR.Shared.Classes
{
    public class CheckAddressResultClass
    {
        public string State { get; set; }
        [Obsolete]
        public long Lovelace { get; set; }
        public long ReceivedAptosOctas { get; set; }
        public long ReceivedSolanaLamports { get; set; }
        public long ReceivedCardanoLovelace { get; set; }
        public Coin Coin { get; set; }
        public long HasToPay { get; set; }
        public Tokens[] AdditionalPriceInTokens { get; set; }
        public DateTime? PayDateTime { get; set; }
        public DateTime? ExpiresDateTime { get; set; }
        public string Transaction { get; set; }
        public string SenderAddress { get; set; }

        public NFT[] ReservedNft { get; set; }
        public string RejectReason { get; set; }
        public string RejectParameter { get; set; }
        public long? StakeReward { get; set; }
        public long? Discount { get; set; }
        public string CustomProperty { get; set; }
        public long? TokenReward { get; set; }
        public long CountNftsOrTokens { get; set; }
        public string ReservationType { get; set; }
    }
}
