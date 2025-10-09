using System;
using NMKR.Shared.Enums;

namespace NMKR.Shared.Classes
{
    public class NftDetailsClass
    {
        public int Id { get; set; }
        public string Ipfshash { get; set; }
        public string State { get; set; }
        public string Name { get; set; }
        public string Displayname { get; set; }
        public string Detaildata { get; set; }
        public bool Minted { get; set; }
        public string Receiveraddress { get; set; }
        public DateTime? Selldate { get; set; }
        public string Soldby { get; set; }
        public DateTime? Reserveduntil { get; set; }
        public string Policyid { get; set; }
        public string Assetid { get; set; }
        public string Assetname { get; set; }
        public string Fingerprint { get; set; }
        public string Initialminttxhash { get; set; }
        public string Title { get; set; }
        public string Series { get; set; }
        public string IpfsGatewayAddress { get; set; }
        public string Metadata { get; set; }
        public long? SinglePrice { get; set; }
        public string Uid { get; set; }
        public string PaymentGatewayLinkForSpecificSale { get; set; }
        public long? SendBackCentralPaymentInLovelace { get; set; }
        public long? PriceInLovelaceCentralPayments { get; set; }
        public string UploadSource { get; set; }
        public long? PriceInLamportCentralPayments { get; set; }
        public long? SinglePriceSolana { get; set; }
        public long? PriceInOctsCentralPayments { get; set; }
        public Blockchain MintedOnBlockchain { get; set; }
        public long? Mintingfees { get; set; }
    }
}
