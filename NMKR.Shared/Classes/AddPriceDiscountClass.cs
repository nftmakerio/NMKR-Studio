using System.Collections.Generic;

namespace NMKR.Shared.Classes
{
    public class AddPriceDiscountPolicies
    {
       public string PolicyId { get; set; }
       public long Minvalue { get; set; }
    }
    public class AddPriceDiscountClass
    {
        public string Condition { get; set; }
        public string Description { get; set; }
        public bool Activated { get; set; }
        public float SendbackDiscount { get; set; }
        public int? Id { get; set; }
        public int NftProjectId { get; set; }
        public string Projectname { get; set; }

        public List<AddPriceDiscountPolicies> AddPolicyid = new();
        public string WhitelistedAddresses { get; set; }
        public string Operator { get; set; } = "OR";
        public string Referercode { get; set; }
        public string Couponcode { get; set; }
        public string Blockchain { get; set; }
    }
}