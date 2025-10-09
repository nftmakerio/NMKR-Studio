namespace NMKR.Shared.Classes
{
    public class TransactionAddressClass
    {
        public string Pkh { get; set; }
        public AddressTxInClass[] Addresses { get; set; }
        public string CollateralTxIn { get; set; }
        public string ChangeAddress { get; set; }
    }

    public class AddressTxInClass
    {
        public string Address { get; set; }
        public TxInClass[] Utxo { get; set; }
    }
    public class BuyerClass
    {
        public TransactionAddressClass Buyer { get; set; }
        public long BuyerOffer { get; set; }

    }

    public class SellerClass
    {
        public TransactionAddressClass Seller { get; set; }

    }

    public class MintAndSendReceiverClass
    {
        public string ReceiverAddress { get; set; }
    }

}
