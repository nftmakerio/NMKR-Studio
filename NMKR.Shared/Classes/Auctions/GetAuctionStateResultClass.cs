using System;

namespace NMKR.Shared.Classes.Auctions
{
    public sealed class GetAuctionStateResultClass
    {
        public string Auctionname { get; set; }
        public string AuctionType { get; set; }

        public string Address { get; set; }
        public long Minbet { get; set; }
        public long Actualbet { get; set; }
        public DateTime Runsuntil { get; set; }
        public string Selleraddress { get; set; }
        public string Highestbidder { get; set; }
        public DateTime Created { get; set; }
        public string State { get; set; }
        public float? Royaltyfeespercent { get; set; }
        public string Royaltyaddress { get; set; }
        public float? Marketplacefeepercent { get; set; }

        public AuctionsNft[] AuctionsNfts { get; set; }
        public AuctionsHistory[] Auctionshistories { get; set; }
        public string Uid { get; set; }
    }
    public sealed class AuctionsNft
    {
        public string Policyid { get; set; }
        public string Tokennamehex { get; set; }
        public string Ipfshash { get; set; }
        public string Metadata { get; set; }
        public long Tokencount { get; set; }
    }
    public sealed class AuctionsHistory
    {
        public string Txhash { get; set; }
        public string Senderaddress { get; set; }
        public long Bidamount { get; set; }
        public DateTime Created { get; set; }
        public string State { get; set; }
        public string Returntxhash { get; set; }
    }
}
