namespace NMKR.Shared.DbSyncModel;

public partial class Asset
{
    public byte[] AssetId { get; set; }

    public byte[] AssetName { get; set; }

    public int? Decimals { get; set; }

    public string Description { get; set; }

    public string Fingerprint { get; set; }

    public int? FirstAppearedInSlot { get; set; }

    public string Logo { get; set; }

    public string MetadataHash { get; set; }

    public string Name { get; set; }

    public byte[] PolicyId { get; set; }

    public string Ticker { get; set; }

    public string Url { get; set; }
}
