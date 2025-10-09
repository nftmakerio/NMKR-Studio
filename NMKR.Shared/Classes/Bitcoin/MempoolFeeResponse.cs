namespace NMKR.Shared.Classes.Bitcoin
{
    public class MempoolFeeResponse
    {
        public decimal fastestFee { get; set; }
        public decimal halfHourFee { get; set; }
        public decimal hourFee { get; set; }
    }
}
