namespace NMKR.Shared.Model;

public partial class Metadata
{
    public int Id { get; set; }

    public int NftId { get; set; }

    public string Placeholdername { get; set; }

    public string Placeholdervalue { get; set; }

    public virtual Nft Nft { get; set; }
}
