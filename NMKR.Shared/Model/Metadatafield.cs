namespace NMKR.Shared.Model;

public partial class Metadatafield
{
    public int Id { get; set; }

    public int NftprojectId { get; set; }

    public string Metadataname { get; set; }

    public string Metadatatype { get; set; }

    public virtual Nftproject Nftproject { get; set; }
}
