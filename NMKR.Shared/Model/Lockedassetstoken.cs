namespace NMKR.Shared.Model;

public partial class Lockedassetstoken
{
    public int Id { get; set; }

    public int LockedassetsId { get; set; }

    public string Policyid { get; set; }

    public string Tokennameinhex { get; set; }

    public long Count { get; set; }

    public virtual Lockedasset Lockedassets { get; set; }
}
