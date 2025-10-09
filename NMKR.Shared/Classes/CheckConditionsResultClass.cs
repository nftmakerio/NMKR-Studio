using NMKR.Shared.Functions;

namespace NMKR.Shared.Classes
{
    public class CheckConditionsResultClass
    {
        public bool ConditionsMet { get; set; }
        public string RejectReason { get; set; }
        public string RejectParameter { get; set; }
        public FrankenAddressProtectionClass SendBackAddress { get; set; }
        public AssetsAssociatedWithAccount[]   AssetsAssociatedWithAccount { get; set; }
        public string StakeAddress { get; set; }
    }
}
