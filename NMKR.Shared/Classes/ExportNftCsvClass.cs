namespace NMKR.Shared.Classes
{
    public class ExportNftCsvClass
    {
            public int Id { get; set; }
            public string Uid { get; set; }
            public string Tokenname { get; set; }
            public string Displayname { get; set; }
            public string Detaildata { get; set; }
            public string IpfsHash { get; set; }
            public string State { get; set; }
            public bool Minted { get; set; }
            public string PolicyId { get; set; }
            public string AssetId { get; set; }
            public string Assetname { get; set; }
            public string SpecificPaymentGatewayLink { get; set; }
            public string Fingerprint { get; set; }
            public string IagonId { get; set; }
    }
}
