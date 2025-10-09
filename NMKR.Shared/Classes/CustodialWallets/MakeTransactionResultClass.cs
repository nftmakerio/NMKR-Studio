using System;

namespace NMKR.Shared.Classes.CustodialWallets
{
    public enum MakeTransactionResults
    {
        error,
        success,
    }
    public class MakeTransactionResultClass
    {
        public MakeTransactionResults State { get; set; }
        public string ErrorMessage { get; set; }
        public string TxHash { get; set; }
        public DateTime Executed { get; set; }
        public long? Fee { get; set; }
    }
}
