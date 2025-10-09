using System.Collections.Generic;

namespace NMKR.Shared.Model;

public partial class Nftgroup
{
    public int Id { get; set; }

    public int NftprojectId { get; set; }

    public string Groupname { get; set; }

    public int Totaltokens1 { get; set; }

    public int Tokensreserved1 { get; set; }

    public int Tokenssold1 { get; set; }

    public virtual Nftproject Nftproject { get; set; }

    public virtual ICollection<Nft> Nfts { get; set; } = new List<Nft>();

    public virtual ICollection<NftsArchive> NftsArchives { get; set; } = new List<NftsArchive>();

    public virtual ICollection<Pricelist> Pricelists { get; set; } = new List<Pricelist>();
}
