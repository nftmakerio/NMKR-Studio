using System.Collections.Generic;

namespace NMKR.Shared.Classes
{
    public class AddSaleCondtionClass
    {
        public string Condition { get; set; }
        public string Description { get; set; }
        public long? Maxvalue { get; set; }
        public bool Activated { get; set; }
        public int? Id { get; set; }
        public int NftProjectId { get; set; }
        public string Projectname { get; set; }

        public List<string> AddPolicyid = new();
        public string WhitelistedAddresses { get; set; }
        public bool CumulateMaxCountPerStakeAddress { get; set; } = true;
        public string BlacklistedAddresses { get; set; }
        public string Blockchain { get; set; }
    }
}
