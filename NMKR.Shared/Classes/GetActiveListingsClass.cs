namespace NMKR.Shared.Classes
{
    public class GetActiveListingsClass
    {
        public string PolicyId { get; set; }
        public string AssetNameInHex { get; set; }
        public string SmartcontractName { get; set; }
        public string Fingerprint { get; set; }
      //  public long PriceInLovelace { get; set; }

      public string Buylink { get; set; }
      public string TxHashAndId { get; set; }
      public string SmartcontractAddress { get; set; }
    }
}
