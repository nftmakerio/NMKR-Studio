using System.Collections.Generic;
using NMKR.Shared.Model;

namespace NMKR.Shared.Classes
{
    public class NewMintAndSendClass
    {
        public Nftproject NftProject { get; set; }
        public List<Mintandsend> MintAndSends { get; set; } 
        public List<Nftreservation> NftReservations { get; set; }
        public int CountNfts { get; set; }
    }
}
