namespace NMKR.Shared.Classes
{
    public class EUR
    {
        public string TYPE { get; set; }

        public string MARKET { get; set; }

        public string FROMSYMBOL { get; set; }

        public string TOSYMBOL { get; set; }

        public string FLAGS { get; set; }

        public double PRICE { get; set; }

        public string LASTUPDATE { get; set; }

        public double MEDIAN { get; set; }

        public string LASTVOLUME { get; set; }

        public string LASTVOLUMETO { get; set; }

        public string LASTTRADEID { get; set; }

       

     

        public string CONVERSIONTYPE { get; set; }

        public string CONVERSIONSYMBOL { get; set; }

     

        public string IMAGEURL { get; set; }
    }

    public class USD
    {
        public string TYPE { get; set; }

        public string MARKET { get; set; }

        public string FROMSYMBOL { get; set; }

        public string TOSYMBOL { get; set; }

        public string FLAGS { get; set; }

        public double PRICE { get; set; }

        public string LASTUPDATE { get; set; }

        public double MEDIAN { get; set; }

        public string LASTVOLUME { get; set; }

        public string LASTVOLUMETO { get; set; }

        public string LASTTRADEID { get; set; }

    

        public string CONVERSIONTYPE { get; set; }

        public string CONVERSIONSYMBOL { get; set; }

        public double SUPPLY { get; set; }

        public double MKTCAP { get; set; }

     

        public string IMAGEURL { get; set; }
    }

    public class PRICES
    {
        public EUR EUR { get; set; }

        public USD USD { get; set; }
        public USD JPY { get; set; }
    }


    public class RAW
    {
        public PRICES BTC { get; set; }

        public PRICES APT { get; set; }

        public PRICES HBAR { get; set; }

        public PRICES ADA { get; set; }

        public PRICES SOL { get; set; }
        public PRICES SONY { get; set; }
        public PRICES ETH { get; set; }
        public PRICES MATIC { get; set; }
    }

    public class CryptoComparePricesClass
    {
        public RAW RAW { get; set; }

        //   [JsonPropertyName("DISPLAY")]
        //   public DISPLAY DISPLAY { get; set; }
    }
}