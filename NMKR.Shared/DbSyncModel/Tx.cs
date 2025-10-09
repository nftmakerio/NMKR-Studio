namespace NMKR.Shared.DbSyncModel;

public partial class Tx
{
    public long Id { get; set; }

    public byte[] Hash { get; set; }

    public long BlockId { get; set; }

    public int BlockIndex { get; set; }

    public decimal OutSum { get; set; }

    public decimal Fee { get; set; }

    public long Deposit { get; set; }

    public int Size { get; set; }

    public decimal? InvalidBefore { get; set; }

    public decimal? InvalidHereafter { get; set; }

    public bool ValidContract { get; set; }

    public int ScriptSize { get; set; }
}
