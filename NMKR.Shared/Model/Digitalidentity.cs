using System;

namespace NMKR.Shared.Model;

public partial class Digitalidentity
{
    public int Id { get; set; }

    public int NftprojectId { get; set; }

    public string Policyid { get; set; }

    public DateTime Created { get; set; }

    public string State { get; set; }

    public string Didprovider { get; set; }

    public string Didjsonresult { get; set; }

    public DateTime? Didresultreceived { get; set; }

    public string Tokenjson { get; set; }

    public string Resultmessage { get; set; }

    public string Ipfshash { get; set; }

    public virtual Nftproject Nftproject { get; set; }
}
