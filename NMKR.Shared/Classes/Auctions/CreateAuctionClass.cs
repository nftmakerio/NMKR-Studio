using System;

namespace NMKR.Shared.Classes.Auctions
{
    public class CreateAuctionClass
    {
        public string AuctionName { get; set; }
        public string PayoutWallet { get; set; }
        public DateTime AuctionRunsUntil { get; set; }
        public long MinimumBidInAda { get; set; }
    }

}
