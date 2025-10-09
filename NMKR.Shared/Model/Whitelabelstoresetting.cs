namespace NMKR.Shared.Model;

public partial class Whitelabelstoresetting
{
    public int Id { get; set; }

    public int StoresettingsId { get; set; }

    public string Value { get; set; }

    public int NftprojectId { get; set; }

    public virtual Nftproject Nftproject { get; set; }

    public virtual Storesetting Storesettings { get; set; }
}
