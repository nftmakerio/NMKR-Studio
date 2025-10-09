using System;

namespace NMKR.Shared.Model;

public partial class Whitelabelstorecollection
{
    public int Id { get; set; }

    public int NftprojectId { get; set; }

    public int StoresettingsId { get; set; }

    public string Policyid { get; set; }

    public string State { get; set; }

    public string Collectionname { get; set; }

    public string Collectiondescription { get; set; }

    public string Nameofcreator { get; set; }

    public string Twritterlink { get; set; }

    public string Instagramlink { get; set; }

    public string Discordlink { get; set; }

    public DateTime? Activefrom { get; set; }

    public DateTime? Activeto { get; set; }

    public bool? Showonfrontpage { get; set; }

    public bool? Isdropinprogess { get; set; }

    public string Dropprojectuid { get; set; }

    public string Uid { get; set; }

    public string Previewimage { get; set; }

    public virtual Nftproject Nftproject { get; set; }

    public virtual Storesetting Storesettings { get; set; }
}
