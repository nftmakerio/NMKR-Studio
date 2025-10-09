using System;

namespace NMKR.Shared.Classes.Vesting
{
    public class CreateVestingAddressClass
    {
        public string UnlockAddress { get; set; }
        public DateTime LockedUntil { get; set; }
        public string Description { get; set; }
    }
}
