using System;
using System.Globalization;

namespace NMKR.Shared.Classes
{
    public class PricelistClass
    {
        public long CountNft { get; set; }
        public long PriceInLovelace { get; set; }
        public string AdaToSend => (PriceInLovelace / (double)1000000).ToString(CultureInfo.InvariantCulture);
        public float PriceInEur { get; set; }
        public float PriceInUsd { get; set; }
        public float PriceInJpy { get; set; }
        public DateTime Effectivedate { get; set; }
        public Tokens[] AdditionalPriceInTokens { get; set; }
        public string PaymentGatewayLinkForRandomNftSale { get; set; }
        public string Currency { get; set; }

        public long SendBackCentralPaymentInLovelace { get; set; }

        public string SendBackCentralPaymentInAda => (SendBackCentralPaymentInLovelace / (double)1000000).ToString(CultureInfo.InvariantCulture);
        public long PriceInLovelaceCentralPayments { get; set; }
        public string AdaToSendCentralPayments => (PriceInLovelaceCentralPayments / (double)1000000).ToString(CultureInfo.InvariantCulture);

        public long PriceInLamport { get; set; }

        public string SolToSend => (PriceInLamport / (double)1000000000).ToString(CultureInfo.InvariantCulture);
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public long PriceInOctas { get; set; }
        public long PriceInSatoshis { get; set; }
        public string AptToSend => (PriceInOctas / (double)100000000).ToString(CultureInfo.InvariantCulture);
        public string BtcToSend => (PriceInSatoshis / (double)100000000).ToString(CultureInfo.InvariantCulture);
        public bool FreeMint { get; set; }
    }
}
