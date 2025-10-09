#nullable disable

namespace NMKR.Shared.Model
{
    public partial class Burningendpoint
    {
        public int Id { get; set; }
        public string Address { get; set; }
        public string Privateskey { get; set; }
        public string Privatevkey { get; set; }
        public long Lovelace { get; set; }
        public string Salt { get; set; }
    }
}
