using System;
using NMKR.Shared.Enums;

namespace NMKR.Shared.Classes
{
    public class CreateBurningEndpointClass
    {
        public string Address { get; set; }
        public DateTime Validuntil { get; set; }
        public Blockchain Blockchain { get; set; }=Blockchain.Cardano;
    }
}
