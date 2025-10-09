namespace NMKR.Shared.Classes
{
    public class MintManuallyClass
    {
        public string PolicyId { get; set; }
        public string Tokenname { get; set; }
        public string Prefix { get; set; }

        public string Metadata { get; set; }
        public bool BurnResult { get; set; }
        public string ReceiverAddress { get; set; }
        public int Projectid { get; set; }
        public string SenderAddress { get; set; }
        public string SenderSKey { get; set; }
        public string SenderVKey { get; set; }
        public long LovelaceForReceiver { get; set; }
    }
}
