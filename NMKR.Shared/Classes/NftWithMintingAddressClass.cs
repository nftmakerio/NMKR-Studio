using NMKR.Shared.Model;

namespace NMKR.Shared.Classes
{
    public class NftWithMintingAddressClass
    {
        public Nft Nft { get; set; }
        public string MintingAddress { get; set; }

        public NftWithMintingAddressClass(Nft nft, string mintingAddress)
        {
            Nft = nft;
            MintingAddress = mintingAddress;
        }
        public NftWithMintingAddressClass()
        {

        }
    }
    public class NftIdWithMintingAddressClass
    {
        public int NftId { get; set; }
        public string MintingAddress { get; set; }

        public NftIdWithMintingAddressClass(int nftId, string mintingAddress)
        {
            NftId = nftId;
            MintingAddress = mintingAddress;
        }

        public NftIdWithMintingAddressClass()
        {

        }

        public NftIdWithMintingAddressClass(NftWithMintingAddressClass nft)
        {
            NftId = nft.Nft.Id;
            MintingAddress = nft.MintingAddress;
        }
    }
}