using System;

namespace NMKR.Shared.Classes
{
    public class NFT
    {
        public int Id { get; set; }
        public string Uid { get; set; }
        public string Name { get; set; }
        public string Displayname { get; set; }
        public string Detaildata { get; set; }
        public string IpfsLink { get; set; }
      //  public string MetaData { get; set; }
        public string GatewayLink { get; set; }
        public string State { get; set; }
        public bool Minted { get; set; }
        public string PolicyId { get; set; }
        public string AssetId { get; set; }
        public string Assetname { get; set; }
        public string Fingerprint { get; set; }
        public string InitialMintTxHash { get; set; }
        public string Series { get; set; }
        public long Tokenamount { get; set; }
        public long? Price { get; set; }
        public DateTime? Selldate { get; set; }
        public string PaymentGatewayLinkForSpecificSale { get; set; }
        public long? PriceSolana { get; set; }
        public long? PriceAptos { get; set; }
    }
}
