using System;

namespace NMKR.Shared.Model;

public partial class Premintedpromotokenaddress
{
    public int Id { get; set; }

    public int BlockchainId { get; set; }

    public string Tokenname { get; set; }

    public string PolicyidOrCollection { get; set; }

    public string Address { get; set; }

    public string Seedphrase { get; set; }

    public string Privatekey { get; set; }

    public string Publickey { get; set; }

    public string Salt { get; set; }

    public long Totaltokens { get; set; }

    public string State { get; set; }

    public DateTime Lastcheck { get; set; }

    public DateTime? Blockdate { get; set; }

    public string Reservationtoken { get; set; }

    public string Lasttxhash { get; set; }

    public virtual Activeblockchain Blockchain { get; set; }
}
