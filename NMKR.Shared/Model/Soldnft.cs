using System;

namespace NMKR.Shared.Model;

public partial class Soldnft
{
    public int Id { get; set; }

    public int NftId { get; set; }

    public int Tokencount { get; set; }

    public DateTime Created { get; set; }

    public int? ServerId { get; set; }

    public virtual Nft Nft { get; set; }

    public virtual Backgroundserver Server { get; set; }
}
