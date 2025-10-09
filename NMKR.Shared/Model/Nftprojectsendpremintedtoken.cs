namespace NMKR.Shared.Model;

public partial class Nftprojectsendpremintedtoken
{
    public int Id { get; set; }

    public int NftprojectId { get; set; }

    public int BlockchainId { get; set; }

    public string PolicyidOrCollection { get; set; }

    public string Tokenname { get; set; }

    public long Countokenstosend { get; set; }

    public string State { get; set; }

    public bool Sendwithmintandsend { get; set; }

    public bool Sendwithapiaddresses { get; set; }

    public bool Sendwithmultisigtransactions { get; set; }

    public bool Sendwithpayinaddresses { get; set; }

    public virtual Activeblockchain Blockchain { get; set; }

    public virtual Nftproject Nftproject { get; set; }
}
