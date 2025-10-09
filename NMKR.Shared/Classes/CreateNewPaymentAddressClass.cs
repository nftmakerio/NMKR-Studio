using NMKR.Shared.Enums;

namespace NMKR.Shared.Classes
{
    public class CreateNewPaymentAddressClass
    {

        public CreateNewPaymentAddressClass()
        {

        }
        public CreateNewPaymentAddressClass(string address, string privatevkey, string privateskey, string seedphrase, Blockchain blockchain)
        {
            this.Address = address;
            this.privatevkey = privatevkey;
            this.privateskey = privateskey;
            this.SeedPhrase = seedphrase;
            this.Blockchain = blockchain;
        }

        public string stakevkey;
        public string stakeskey;
        public byte[] pkh;
        public int ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string Address { get; set; }
        public string privatevkey { get; set; }
        public string privateskey { get; set; }
        public string expandedPrivateKey { get; set; }
        public string SeedPhrase { get; set; }
        public Blockchain Blockchain { get; set; }
    }
}
