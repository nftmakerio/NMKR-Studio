using System;
using NMKR.Shared.Enums;

namespace NMKR.Shared.Classes
{
    public class NewRatesClass
    {
        public Coin Coin { get; set; }
        public double BtcRate { get; set; }
        public double UsdRate { get; set; }
        public double EurRate { get; set; }
        public double JpyRate { get; set; }
        public DateTime EffectiveDate { get; set; }

    }
}
