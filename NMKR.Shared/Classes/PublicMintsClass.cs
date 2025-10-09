using NMKR.Shared.Enums;
using System;

namespace NMKR.Shared.Classes
{
    public enum PublicMintState
    {
        Upcoming,
        Active,
        SoldOut,
        Ended
    }
    public class PublicMintsClass
    {
        public string ProjectName { get; set; }
        public string ProjectDescription { get; set; }
        public string ProjectImage { get; set; }
        public string ProjectUrl { get; set; }
        public DateTime ProjectCreated { get; set; }
        public string CreaterName { get; set; }
        public DateTime MintStart { get; set; }
        public PublicMintState MintState { get; set; }
        public PricelistClass[] Pricelist { get; set; }
        public long TotalNfts { get; set; }
        public long ReservedNfts { get; set; }
        public long SoldNfts { get; set; }
        public Blockchain[] Blockchains { get; set; }
    }
}
