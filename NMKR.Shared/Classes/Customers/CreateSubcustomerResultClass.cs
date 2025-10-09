using System;
using NMKR.Shared.Enums;

namespace NMKR.Shared.Classes.Customers
{
    public class SubcustomerMintcouponPayinAddresses
    {
       public Blockchain Blockchain { get; set; }
        public string Address { get; set; }
        public string Network { get; set; }
        public Coin Coin { get; set; }
        public double PricePerMintCoupon { get; set; }
    }

    public class CreateSubcustomerResultClass : CreateSubcustomerClass
    {
        public int SubcustomerId { get; set; }
        public DateTime Created { get; set; }
        public SubcustomerMintcouponPayinAddresses[] MintcouponPayinAddresses { get; set; }
    }
}
