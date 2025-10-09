namespace NMKR.Shared.DbSyncModel;

public partial class TxMetadatum
{
    public long Id { get; set; }

    public decimal Key { get; set; }

    public string Json { get; set; }

    public byte[] Bytes { get; set; }

    public long TxId { get; set; }
}
