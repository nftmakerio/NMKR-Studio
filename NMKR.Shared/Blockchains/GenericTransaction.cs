using NMKR.Shared.Enums;

namespace NMKR.Shared.Blockchains
{
    public class GenericTransaction
    {
        public string Block { get; set; }
        public long? Fees { get; set; }
        public string Hash { get; set; }
        public long? Index { get; set; }
        public Blockchain Blockchain { get; set; }
        public ulong Slot { get; set; }
    }
}
