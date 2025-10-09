using NMKR.Shared.Enums;
using System;
using System.Globalization;

namespace NMKR.Shared.Classes
{
    public class TokensBaseClass
    {
        public long CountToken { get; set; }
        public string PolicyId { get; set; }
        public string AssetNameInHex { get; set; }
    }

    public class Tokens : TokensBaseClass
    {
        public long Multiplier { get; set; }
        public long TotalCount { get; set; }
        public string AssetName { get; set; }
        public long Decimals { get; set; }
    }

    public class GetPaymentAddressResultClass
    {
        public string PaymentAddress { get; set; }
        public int PaymentAddressId { get; set; }
        public DateTime Expires { get; set; }

        public string AdaToSend
        {
            get
            {
                if (Currency == Coin.ADA.ToString() || Currency == Coin.USD.ToString() ||
                    Currency == Coin.JPY.ToString() || Currency == Coin.EUR.ToString())
                {
                    return PriceInLovelace == -1 ? "0" : (PriceInLovelace / (double)1000000).ToString(CultureInfo.InvariantCulture);
                }

                return null;
            }
        }
        public string SolToSend
        {
            get
            {
                if (Currency == Coin.SOL.ToString() || Currency == Coin.USD.ToString() ||
                    Currency == Coin.JPY.ToString() || Currency == Coin.EUR.ToString())
                {
                    return PriceInLamport == -1 ? "0" : (PriceInLamport / (double)1000000000).ToString(CultureInfo.InvariantCulture);
                }

                return null;
            }
        }
        public string AptToSend
        {
            get
            {
                if (Currency == Coin.APT.ToString() || Currency == Coin.USD.ToString() ||
                    Currency == Coin.JPY.ToString() || Currency == Coin.EUR.ToString())
                {
                    return PriceInOcta == -1 ? "0" : (PriceInOcta / (double)100000000).ToString(CultureInfo.InvariantCulture);
                }

                return null;
            }
        }
        public string Debug { get; set; }
        public double PriceInEur { get; set; }
        public double PriceInUsd { get; set; }
        public double PriceInJpy { get; set; }
        public DateTime Effectivedate { get; set; }
        public long PriceInLovelace { get; set; }
        public Tokens[] AdditionalPriceInTokens { get; set; }
     //   public string PaymentGatewayUrl { get; set; }
        public long SendbackToUser { get; set; }
        public string Revervationtype { get; set; }
        public string Currency { get; set; } = Coin.ADA.ToString();
        public long PriceInLamport { get; set; }
        public long PriceInOcta { get; set; }
        public long PriceInSatoshi { get; set; }

    }


}
