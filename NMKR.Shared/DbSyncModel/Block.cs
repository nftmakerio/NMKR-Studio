using System;

namespace NMKR.Shared.DbSyncModel;

public partial class Block
{
    public long Id { get; set; }

    public byte[] Hash { get; set; }

    public int? EpochNo { get; set; }

    public long? SlotNo { get; set; }

    public int? EpochSlotNo { get; set; }

    public int? BlockNo { get; set; }

    public long? PreviousId { get; set; }

    public long SlotLeaderId { get; set; }

    public int Size { get; set; }

    public DateTime Time { get; set; }

    public long TxCount { get; set; }

    public int ProtoMajor { get; set; }

    public int ProtoMinor { get; set; }

    public string VrfKey { get; set; }

    public byte[] OpCert { get; set; }

    public long? OpCertCounter { get; set; }
}
