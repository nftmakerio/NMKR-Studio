using NMKR.Shared.Functions;

namespace NMKR.Shared.Classes
{
    public class PkhPercentageClass
    {
        public string PublicKeyHash { get; set; }
        public float Percentage { get; set; }
        public string EncryptedSKey { get; set; }
        public string Salt { get; set; }
        public string Address { get; set; }

        public string TokenPolicyId { get; set; }
        public string TokenNameHex { get; set; }
        public long? TokenCount { get; set; }

        public string StakeKeyHash
        {
            get
            {
                if (string.IsNullOrEmpty(Address))
                    return "";

                var stake = Bech32Engine.GetStakeFromAddress(Address);
                if (string.IsNullOrEmpty(stake))
                    return "";

                return GlobalFunctions.GetPkhFromAddress(stake);
            }
        }
    }
}
