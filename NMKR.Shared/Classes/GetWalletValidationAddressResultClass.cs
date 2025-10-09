using System;

namespace NMKR.Shared.Classes
{
    public class GetWalletValidationAddressResultClass
    {
        public string ValidationUId { get; set; }
        public string Address { get; set; }
        public DateTime Expires { get; set; }
        public long Lovelace { get; set; }
    }
}
