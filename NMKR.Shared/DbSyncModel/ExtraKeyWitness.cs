namespace NMKR.Shared.DbSyncModel;

public partial class ExtraKeyWitness
{
    public long Id { get; set; }

    public byte[] Hash { get; set; }

    public long TxId { get; set; }
}
