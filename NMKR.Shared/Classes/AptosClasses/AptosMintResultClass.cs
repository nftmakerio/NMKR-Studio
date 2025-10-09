namespace NMKR.Shared.Classes.AptosClasses
{
    public class AptosMintResultClass
    {
        public string state { get; set; }
        public string mintTransactionHash { get; set; }
        public string mintSender { get; set; }
        public long mintFee { get; set; }
        public string transferTransactionHash { get; set; }
        public string transferSender { get; set; }
        public long transferFee { get; set; }
    }
}
