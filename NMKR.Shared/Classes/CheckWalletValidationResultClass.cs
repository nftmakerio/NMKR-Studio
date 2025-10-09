using System;

namespace NMKR.Shared.Classes
{
    public class CheckWalletValidationResultClass
    {
        public string ValidationResult { get; set; }
        public string SenderAddress { get; set; }
        public string StakeAddress { get; set; }
        public long Lovelace { get; set; }
        public string Validationaddress { get; set; }
        public DateTime ValidUntil { get; set; }
        public string ValidationName { get; set; }
    }
}
