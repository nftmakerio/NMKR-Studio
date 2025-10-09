namespace NMKR.Shared.DbSyncModel;

public partial class PoolUpdate
{
    public long Id { get; set; }

    public long HashId { get; set; }

    public int CertIndex { get; set; }

    public byte[] VrfKeyHash { get; set; }

    public decimal Pledge { get; set; }

    public long ActiveEpochNo { get; set; }

    public long? MetaId { get; set; }

    public double Margin { get; set; }

    public decimal FixedCost { get; set; }

    public long RegisteredTxId { get; set; }

    public long RewardAddrId { get; set; }
}
