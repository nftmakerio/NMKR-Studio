namespace NMKR.Shared.Model;

public partial class Tokenreward
{
    public int Id { get; set; }

    public string Policyid { get; set; }

    public string Tokennameinhex { get; set; }

    public long Reward { get; set; }

    public string State { get; set; }

    public long Mincount { get; set; }
}
