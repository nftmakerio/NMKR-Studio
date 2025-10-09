using System;

namespace NMKR.Shared.Classes.Auctions
{
    public class GetAuctionsClass
    {
        public string Auctionname { get; set; }
        public string AuctionType { get; set; }

        public string Address { get; set; }
        public DateTime Created { get; set; }
        public string State { get; set; }
        public string Uid { get; set; }
        public DateTime RunsUntil { get; set; }
    }
}
