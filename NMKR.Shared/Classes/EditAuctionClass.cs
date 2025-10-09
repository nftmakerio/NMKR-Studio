using System;

namespace NMKR.Shared.Classes
{
    public sealed class EditAuctionClass
    {
        public int? Id { get; set; }
        public string AuctionName { get; set; }
        public string SellerAddress { get; set; }
        public double MinBet { get; set; }
        public DateTime? RunsUntil { get; set; }
        public TimeSpan? RunsUntilTime { get; set; }
    }
}
