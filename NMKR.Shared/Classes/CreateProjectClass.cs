using System;

namespace NMKR.Shared.Classes
{
    public class CreateProjectClass
    {
        public string Projectname  { get; set; }
        public string Description { get; set; }
        public string Projecturl { get; set; }
        public string TokennamePrefix { get; set; }
        public bool PolicyExpires { get; set; }
        public DateTime? PolicyLocksDateTime { get; set; }
       // public string Version { get; set; }
        public string PayoutWalletaddress { get; set; }
        public long MaxNftSupply { get; set; }
        public PolicyClass Policy { get; set; }
        public string Metadata { get; set; }
        public int AddressExpiretime { get; set; }
    }

    public class PolicyClass
    {
        public string PolicyId { get; set; }
        public string PrivateVerifykey { get; set; }
        public string PrivateSigningkey { get; set; }
        public string PolicyScript { get; set; }
    }

}


