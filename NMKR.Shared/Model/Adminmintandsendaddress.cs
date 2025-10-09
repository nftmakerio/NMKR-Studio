using System;

namespace NMKR.Shared.Model;

public partial class Adminmintandsendaddress
{
    public int Id { get; set; }

    public string Address { get; set; }

    public string Privateskey { get; set; }

    public string Privatevkey { get; set; }

    public long Lovelace { get; set; }

    public bool Addressblocked { get; set; }

    public int Blockcounter { get; set; }

    public string Salt { get; set; }

    public string Lasttxhash { get; set; }

    public DateTime? Lasttxdate { get; set; }

    public string Reservationtoken { get; set; }

    public string Coin { get; set; }

    public string Seed { get; set; }

    public DateTime Lastcheckforutxo { get; set; }
}
