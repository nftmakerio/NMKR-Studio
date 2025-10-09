using NMKR.Shared.Enums;

namespace NMKR.Shared.Classes.Projects
{
    public class AdditionalPayoutWalletsClass
    {
        public string WalletAddress { get; set; }

        public double? Valuepercent { get; set; }

        public long? Valuetotal { get; set; }

        public string Custompropertycondition { get; set; }

        public Coin Coin { get; set; }
        public Blockchain Blockchain { get; set; }
    }
}
