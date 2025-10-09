using System;

namespace NMKR.Shared.Model;

public partial class Submission
{
    public int Id { get; set; }

    public string State { get; set; }

    public string Matxsigned { get; set; }

    public string Txid { get; set; }

    public string Reservationtoken { get; set; }

    public int NftprojectId { get; set; }

    public string Type { get; set; }

    public int? ProcessedbyserverId { get; set; }

    public DateTime Created { get; set; }

    public DateTime? Submitted { get; set; }

    public string Submitresult { get; set; }

    public string Submissionlogfile { get; set; }

    public virtual Nftproject Nftproject { get; set; }

    public virtual Backgroundserver Processedbyserver { get; set; }
}
