namespace NMKR.Shared.Classes
{
    public class NftCountsClass
    {
        public long NftTotal { get; set; }
        public long Sold { get; set; }
        public long Free { get; set; }
        public long Reserved { get; set; }
        public long Error { get; set; }
        public long Blocked { get; set; }
        public long TotalTokens { get; set; }
        public long TotalBlocked { get; set; }
        public long UnknownOrBurnedState { get; set; }
    }
}
