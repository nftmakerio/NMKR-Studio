using NMKR.Shared.Model;

namespace NMKR.Shared.Classes
{
    public class PromotionClass
    {
        public string PolicyId { get; set; }
        public string Metadata { get; set; }
        public string SKey { get; set; }
        public string VKey { get; set; }
        public long Tokencount { get; set; }
        public string TokennameHex { get; set; }
        public Nft PromotionNft { get; set; }
        public string PolicyScriptfile { get; set; }
        public string Token
        {
            get
            {
                return Tokencount + " " + PolicyId + "." + TokennameHex;
            }
        }
    }
}
