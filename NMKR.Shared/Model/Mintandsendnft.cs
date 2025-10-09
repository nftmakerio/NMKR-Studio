#nullable disable

namespace NMKR.Shared.Model
{
    public partial class Mintandsendnft
    {
        public int Id { get; set; }
        public int MintandsendId { get; set; }
        public int NftId { get; set; }
        public int Tokencount { get; set; }

        public virtual Mintandsend Mintandsend { get; set; }
        public virtual Nft Nft { get; set; }
    }
}
