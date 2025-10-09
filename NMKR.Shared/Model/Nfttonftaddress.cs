namespace NMKR.Shared.Model;

public partial class Nfttonftaddress
{
    public int Id { get; set; }

    public int NftId { get; set; }

    public int NftaddressesId { get; set; }

    public long Tokencount { get; set; }

    public virtual Nft Nft { get; set; }

    public virtual Nftaddress Nftaddresses { get; set; }
}
