namespace NMKR.Shared.DbSyncModel;

public partial class Script
{
    public long Id { get; set; }

    public long TxId { get; set; }

    public byte[] Hash { get; set; }

    public string Json { get; set; }

    public byte[] Bytes { get; set; }

    public int? SerialisedSize { get; set; }
}
