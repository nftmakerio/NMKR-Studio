using System;

namespace NMKR.Shared.Classes
{
    public class CheckCustomerAddressAddressesClass
    {
        public string Address { get; set; }
        public string Seed { get; set; }
        public string PublicKey { get; set; }
        public string PrivateKey { get; set; }
        public int Blockcounter { get; set; }
        public DateTime? LastcheckForUtxo { get; set; }
        public bool Addressblocked { get; set; }
        public long Amount { get; set; }
        public bool InternalAccount { get; set; }
        public int CustomerId { get; set; }
        public string Salt { get; set; }
        public string MintCouponsReceiverAddress { get; set; }
        public float PriceMintCoupons { get; set; }
    }
}
