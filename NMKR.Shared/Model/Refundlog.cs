using System;

namespace NMKR.Shared.Model;

public partial class Refundlog
{
    public int Id { get; set; }

    public int NftprojectId { get; set; }

    public string Senderaddress { get; set; }

    public string Receiveraddress { get; set; }

    public string Txhash { get; set; }

    public DateTime Created { get; set; }

    public string Refundreason { get; set; }

    public string Log { get; set; }

    public string State { get; set; }

    public string Outgoingtxhash { get; set; }

    public long? Lovelace { get; set; }

    public long? Fee { get; set; }

    public long Nmkrcosts { get; set; }

    public string Coin { get; set; }

    public virtual Nftproject Nftproject { get; set; }
}
