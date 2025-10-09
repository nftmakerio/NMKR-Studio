namespace NMKR.Shared.DbSyncModel;

public partial class Datum
{
    public long Id { get; set; }

    public byte[] Hash { get; set; }

    public long TxId { get; set; }

    public string Value { get; set; }

    public byte[] Bytes { get; set; }
}
