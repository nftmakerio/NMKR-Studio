namespace NMKR.Shared.DbSyncModel;

public partial class Redeemer
{
    public long Id { get; set; }

    public long TxId { get; set; }

    public long UnitMem { get; set; }

    public long UnitSteps { get; set; }

    public decimal? Fee { get; set; }

    public int Index { get; set; }

    public byte[] ScriptHash { get; set; }

    public long RedeemerDataId { get; set; }
}
