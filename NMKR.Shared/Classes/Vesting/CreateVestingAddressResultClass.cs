using System;

namespace NMKR.Shared.Classes.Vesting
{
    public class CreateVestingAddressResultClass
    {
        public string Address { get; set; }
        public DateTime LockedUntil { get; set; }
        public long LockedUntilSlot { get; set; }
        public string UnlockAddress { get; set; }
        public string Pkh { get; set; }
        public string Description { get; set; }
        public long ActualSlot { get; set; }
    }
}
