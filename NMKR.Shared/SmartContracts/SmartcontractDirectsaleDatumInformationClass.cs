using NMKR.Shared.Classes;

namespace NMKR.Shared.SmartContracts
{
    public class SmartcontractDirectsaleDatumInformationClass
    {
        public long TotalPriceInLovelace { get; set; }
        public string SmartContractName { get; set; }
        public string SmartContractAddress { get; set; }
        public string NmkrPayLink { get; set; }
        public string PreparedPaymentTransactionId { get; set; }
        public string DatumCbor { get; set; }
        public SmartcontractDirectsaleReceiverClass[] Receivers { get; set; }
    }

    public class SmartcontractDirectsaleReceiverClass
    {
        public string Pkh { get; set; }
        public string Address { get; set; }
        public long AmountInLovelace { get; set; }
        public Tokens[] Tokens { get; set; }
        public string RecevierType { get; set; } = "unknown";
    }
}
