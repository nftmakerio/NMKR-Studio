namespace NMKR.Shared.Model;

public partial class Nftprojectadaaddress
{
    public int Id { get; set; }

    public int NftprojectsId { get; set; }

    public string Address { get; set; }

    public string Privateskey { get; set; }

    public string Privatevkey { get; set; }

    public long? Lovelage { get; set; }

    public virtual Nftproject Nftprojects { get; set; }
}
