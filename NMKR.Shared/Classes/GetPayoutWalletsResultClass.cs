using System;

namespace NMKR.Shared.Classes
{
    public enum PayoutWalletState
    {
        Active,
        NotActive,
        Blocked,
        ConfirmationExpired
    }
    public class GetPayoutWalletsResultClass
    {
        public string WalletAddress { get; set; }
        public DateTime Created { get; set; }
        public PayoutWalletState State { get; set; }
        public string Comment { get; set; }
    }
}
