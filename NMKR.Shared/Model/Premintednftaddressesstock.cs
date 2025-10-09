#nullable disable

namespace NMKR.Shared.Model
{
    public partial class Premintednftaddressesstock
    {
        public int Id { get; set; }
        public int PremintednftaddressesId { get; set; }
        public int NftId { get; set; }
        public string Tokenname { get; set; }

        public virtual Premintednftsaddress Premintednftaddresses { get; set; }
    }
}
