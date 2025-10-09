namespace NMKR.Shared.DbSyncModel;

public partial class StakeAddress
{
    public long Id { get; set; }

    public byte[] HashRaw { get; set; }

    public string View { get; set; }

    public byte[] ScriptHash { get; set; }
}
