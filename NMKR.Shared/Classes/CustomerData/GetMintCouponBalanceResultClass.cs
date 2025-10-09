using System;

namespace NMKR.Shared.Classes.CustomerData
{
    public class GetMintCouponBalanceResultClass
    {
        public float MintCouponBalanceCardano { get; set; }
        public DateTime EffectiveDateTime { get; set; }
        public string MintPurchaseAddressCardano { get; set; }
    }
}
