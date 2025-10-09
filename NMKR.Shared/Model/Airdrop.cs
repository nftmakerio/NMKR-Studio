using System;

namespace NMKR.Shared.Model;

public partial class Airdrop
{
    public int Id { get; set; }

    public int NftprojectId { get; set; }

    public DateTime Created { get; set; }

    public int? MintandsendId { get; set; }

    public string Message { get; set; }

    public string Recevieraddress { get; set; }

    public string Uid { get; set; }

    public int? NftId { get; set; }

    public virtual Mintandsend Mintandsend { get; set; }

    public virtual Nft Nft { get; set; }

    public virtual Nftproject Nftproject { get; set; }
}
