namespace NMKR.Shared.DbSyncModel;

public partial class UtxoByronView
{
    public long? Id { get; set; }

    public long? TxId { get; set; }

    public short? Index { get; set; }

    public string Address { get; set; }

    public byte[] AddressRaw { get; set; }

    public bool? AddressHasScript { get; set; }

    public byte[] PaymentCred { get; set; }

    public long? StakeAddressId { get; set; }

    public decimal? Value { get; set; }

    public byte[] DataHash { get; set; }

    public long? InlineDatumId { get; set; }

    public long? ReferenceScriptId { get; set; }
}
