#nullable disable

namespace NMKR.Shared.Model
{
    public partial class Bunringendpoint
    {
        public int Id { get; set; }
        public string Address { get; set; }
        public string Skey { get; set; }
        public string Vkey { get; set; }
        public long Lovelace { get; set; }
        public string Password { get; set; }
    }
}
