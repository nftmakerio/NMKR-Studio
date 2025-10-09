using System;
using System.Collections.Generic;

namespace NMKR.Shared.Model;

public partial class Nftreservation
{
    public int Id { get; set; }

    public int NftId { get; set; }

    public string Reservationtoken { get; set; }

    public DateTime Reservationdate { get; set; }

    public int Reservationtime { get; set; }

    public long Tc { get; set; }

    public int? Serverid { get; set; }

    public bool Mintandsendcommand { get; set; }

    /// <summary>
    /// not used at the moment
    /// </summary>
    public long Multiplier { get; set; }

    public virtual Nft Nft { get; set; }

    public virtual ICollection<Preparedpaymenttransaction> Preparedpaymenttransactions { get; set; } = new List<Preparedpaymenttransaction>();
}
