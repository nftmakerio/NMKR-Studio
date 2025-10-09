using System;
using System.Collections.Generic;

namespace NMKR.Shared.Model;

public partial class Mintandsend
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public int NftprojectId { get; set; }

    public string Receiveraddress { get; set; }

    public DateTime Created { get; set; }

    public string State { get; set; }

    public string Reservationtoken { get; set; }

    public DateTime? Executed { get; set; }

    public string Transactionid { get; set; }

    public long Reservelovelace { get; set; }

    public bool Onlinenotification { get; set; }

    public string Buildtransaction { get; set; }

    public bool? Usecustomerwallet { get; set; }

    public bool Remintandburn { get; set; }

    public bool Confirmed { get; set; }

    public string Coin { get; set; }

    public int Retry { get; set; }

    public float Requiredcoupons { get; set; }

    public virtual ICollection<Airdrop> Airdrops { get; set; } = new List<Airdrop>();

    public virtual Customer Customer { get; set; }

    public virtual Nftproject Nftproject { get; set; }

    public virtual ICollection<Preparedpaymenttransaction> Preparedpaymenttransactions { get; set; } = new List<Preparedpaymenttransaction>();
}
